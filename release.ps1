Param (  
  [String]$buildNumber = ($( Read-Host "Specify package buildNumber")),
  [string]$nugetkey = ($( Read-Host "Nuget key (optional)"))
)

$solutionFilename = "AspectInjector.sln"
$testsSolutionFilename = "AspectInjector.Test.sln"
$binDir = ".\AspectInjector.BuildTask\bin\Release"
$testsDll = ".\AspectInjector.Tests\bin\Release\AspectInjector.Tests.dll"
$packageBuildPlace = ".\packagebuildplace"

function Update-SourceVersion
{
  Param ([string]$Version)
  $NewVersion = 'AssemblyVersion("' + $Version + '")';
  $NewFileVersion = 'AssemblyFileVersion("' + $Version + '")';

  foreach ($o in $input) 
  {
    Write-output $o.FullName
    $TmpFile = $o.FullName + ".tmp"

     get-content $o.FullName | 
        %{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
        %{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion }  > $TmpFile

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

$buildargs = @( $solutionFilename, "/t:Rebuild", "/p:Configuration=Release;DefineConstants=Trace;Platform=Any CPU" )
& $msbuild $buildargs | out-null

if ($LastExitCode -ne 0) {
    throw "MSBuild failed with exit code $LastExitCode."
	break
}

if(test-path $mstest)
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
	"Could not find MSTest.exe. Skipping tests..."
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
Copy-Item "packagecontent\AspectInjector.props" $targetsDir
Copy-Item "packagecontent\install.ps1" $toolsDir
Copy-Item "README.md" (join-path $packageBuildPlace "readme.txt" )
Get-ChildItem $binDir -Filter "*.dll" -Recurse | %{ Copy-Item $_.Fullname $targetsDir }
Copy-Item (Join-Path $binDir "AspectInjector.Broker.dll") $libDir

"Updating nuspec"
$nuspec = join-path $packageBuildPlace "AspectInjector.nuspec"

Update-Nuspec (get-item $nuspec) $buildNumber

"Creating Package"
& $nuget pack $nuspec | out-null
if ($LastExitCode -ne 0) {
    throw "Nuget Pack failed with exit code $LastExitCode."
	break
}

if($nugetkey -ne ""){
	"Pushing package to nuget"
	$pkg = get-item ("AspectInjector." + $buildNumber + ".nupkg")
	& $nuget push $pkg $nugetkey -s "https://nuget.org/"
	Remove-Item $pkg -Force | Out-Null
}

"Cleanup"
Remove-Item $nuget -Force | Out-Null
Remove-Item $packageBuildPlace -Recurse -Force | Out-Null 
Update-AllAssemblyInfoFiles '9999.0.0'

"Done."

