Param (  
  [String] $buildNumber = ($( Read-Host "Specify package buildNumber")),
  [switch] $skiptests
)

[bool]$publish = $false

$tokens_file = ".\api.tokens"
$nuget_api_token = ""
$github_api_token = ""
$github_repo_uri = "https://api.github.com/repos/pamidur/aspect-injector"
$solutionFilename = "AspectInjector.sln"
$testsSolutionFilename = "AspectInjector.Test.sln"
$binDir = ".\AspectInjector.CommandLine\bin\Release"
$testsDll = ".\AspectInjector.Tests\bin\Release\AspectInjector.Tests.dll"
$packageBuildPlace = ".\packagebuildplace"

function Update-SourceVersion ([string]$semver)
{
	if($semver -like "*-*")	{
		$Version = ($semver -split "-") | select -First 1;
	}
	else{
		$Version = $semver;
	}

  $NewVersion = 'AssemblyVersion("' + $Version + '")';
  $NewFileVersion = 'AssemblyFileVersion("' + $Version + '")';	
  $NewFileInfoVersion = 'AssemblyInformationalVersion("' + $semver + '")';	

  foreach ($o in $input) 
  {
    Write-output $o.FullName
    $TmpFile = $o.FullName + ".tmp"

     get-content $o.FullName | 
        %{$_ -replace 'AssemblyVersion\("[^"]+"\)', $NewVersion } |
        %{$_ -replace 'AssemblyInformationalVersion\("[^"]+"\)', $NewFileInfoVersion } |
        %{$_ -replace 'AssemblyFileVersion\("[^"]+"\)', $NewFileVersion }  > $TmpFile

     move-item $TmpFile $o.FullName -force
  }
}

function Update-AllAssemblyInfoFiles ( $version )
{
  foreach ($file in "AssemblyInfo.cs", "AssemblyInfo.vb" ) 
  {
    get-childitem -recurse |? {$_.Name -eq $file} | Update-SourceVersion $version ;
  }
}

function Update-Nuspec ( $nuspec, $version )
{
    $TmpFile = $nuspec.FullName + ".tmp"

    get-content $nuspec.FullName |         
        %{$_ -replace '<version>[0-9]+(\.([0-9]+|\*)){1,3}<\/version>', "<version>$version</version>" }  > $TmpFile

    move-item $TmpFile $nuspec.FullName -force
}


while (($buildNumber -NotMatch "^[0-9]+\.[0-9]+\.[0-9]+$") -and ($buildNumber -NotMatch "^[0-9]+\.[0-9]+\.[0-9]+-[^-]+$")) {
    $buildNumber = Read-Host "Please enter valid version [major.minor.build(-suffix)]"
}


if(Test-Path $tokens_file){
	iex (Get-Content -Raw $tokens_file)

	"API keys are found."

	$yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes","Publish release."
	$no = New-Object System.Management.Automation.Host.ChoiceDescription "&No","Store package in root."
	$options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no) 
    
	$result = $host.ui.PromptForChoice("","Publish release to github and nuget?", $options, 1)

	$publish = $result -eq 0
}else{
	"API keys missing. Skipping publish..."
} 


"Releasing package"
$nuget = join-path $env:TEMP "nuget.exe"
If( -not (test-path $nuget)){
    "Downloading nuget.exe"
    (new-object net.webclient).DownloadFile('http://nuget.org/nuget.exe',$nuget)
}

"Resolving Dependencies"
& $nuget restore $solutionFilename

"Updating assemblies' versions"
Update-AllAssemblyInfoFiles $buildNumber

"Building app"
$msbuild = join-path (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0").MSBuildToolsPath "msbuild.exe"

$mstest = "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\mstest.exe"

if( ! (Test-Path $mstest)){
    $mstest = "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\mstest.exe"
}

$buildargs = @( $solutionFilename, "/t:Rebuild", "/p:Configuration=Release;DefineConstants=Trace;Platform=Any CPU" )
& $msbuild $buildargs | out-null

if ($LastExitCode -ne 0) {
    throw "MSBuild failed with exit code $LastExitCode."
    break
}

if((test-path $mstest) -and -not $skiptests)
{
    "Building tests"
    $buildargs = @( $testsSolutionFilename, "/t:Rebuild", "/p:Configuration=Release;Platform=Any CPU" )
    & $msbuild $buildargs | out-null

    if ($LastExitCode -ne 0) {
        throw "MSBuild failed with exit code $LastExitCode."
        break
    }

    $testargs = @("/noisolation", "/noresults", "/nologo", "/testcontainer:$testsDll" )
    & $mstest $testargs

    if ($LastExitCode -ne 0) {
        throw "MSTest failed with exit code $LastExitCode."
        break
    }
}else{
    "Skipping tests..."
}

"Creating folders."
if(Test-Path $packageBuildPlace ){
    "Cleaning up package-build-place."
    Remove-Item $packageBuildPlace -Recurse -Force | Out-Null 
}

$targetsDir = Join-Path $packageBuildPlace "build"
$libDir = Join-Path $packageBuildPlace "lib"
$toolsDir = Join-Path $packageBuildPlace "tools"

New-Item $packageBuildPlace -ItemType Directory | Out-Null 
New-Item $targetsDir -ItemType Directory | Out-Null 
New-Item $libDir -ItemType Directory | Out-Null 
New-Item $toolsDir -ItemType Directory | Out-Null 


"Copying items."
Copy-Item "packagecontent\AspectInjector.nuspec" $packageBuildPlace
Copy-Item "packagecontent\AspectInjector.targets" $targetsDir
Copy-Item "packagecontent\install.ps1" $toolsDir
Copy-Item "README.md" (join-path $packageBuildPlace "readme.txt" )
Get-ChildItem $binDir -Filter "*.dll" -Recurse | %{ Copy-Item $_.Fullname $targetsDir }
Get-ChildItem $binDir -Filter "*.exe" -Recurse | %{ Copy-Item $_.Fullname $targetsDir }
Copy-Item (Join-Path $binDir "AspectInjector.Broker.dll") $libDir
Copy-Item (Join-Path $binDir "AspectInjector.Broker.xml") $libDir

"Updating nuspec"
$nuspec = join-path $packageBuildPlace "AspectInjector.nuspec"

Update-Nuspec (get-item $nuspec) $buildNumber

"Creating Package"
& $nuget pack $nuspec | out-null
if ($LastExitCode -ne 0) {
    throw "Nuget Pack failed with exit code $LastExitCode."
    break
}



if($publish){

    $pkg = get-item ("AspectInjector." + $buildNumber + ".nupkg")

    if($nuget_api_token -ne ""){
        "Pushing package to nuget"	
        & $nuget push $pkg $nuget_api_token -s "https://nuget.org/"	
    }

    if($github_api_token -ne ""){
        "Creating release draft on github"

        $hash = iex "git log -1 --format=`"%H`""

        $release_info = @{
            tag_name=$buildNumber;
            name=$buildNumber;
            body='Fixes:
- (fill me in)

```ps
PM> Install-Package AspectInjector -Version ' +$buildNumber+'
```'
            target_commitish=$hash;
            "draft" = $true
        } | ConvertTo-Json -Compress

        ($release = ConvertFrom-Json (Invoke-WebRequest "$github_repo_uri/releases" -Method Post -Body $release_info -Headers @{"Authorization"="token $github_api_token";}).Content ) | Out-Null
    
        #"Uploading package to github"

        #Invoke-WebRequest ($release.upload_url -replace "{\?name}","?name=$($pkg.Name)" ) -Method Post -InFile $pkg -Headers @{"Authorization"="token $github_api_token";"Content-Type"="application/zip"} | Out-Null
    }
    Remove-Item $pkg -Force | Out-Null
}


"Cleanup"
Remove-Item $nuget -Force | Out-Null
Remove-Item $packageBuildPlace -Recurse -Force | Out-Null 
Update-AllAssemblyInfoFiles '9999.0.0'

"Done."

