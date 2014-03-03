Param (  
  [String]$buildNumber = ($(throw "Specify package buildNumber."))
)

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



$version = Get-Content "build\Version.txt"

# replace shortcuts
$version = $version -replace '{BuildNumber}', $buildNumber

"Setting version to $version"

Update-AllAssemblyInfoFiles $version
Update-Nuspec (get-item "build\AspectInjector.nuspec") $version


