Param (  
  [String]$packageBuildPlace = ($(throw "Specify package packageBuildPlace.")),
  [String]$binDir = "AspectInjector.BuildTask\bin\Release"
)

if(Test-Path $packageBuildPlace ){
	"Cleaning up package-build-place."
	Remove-Item $packageBuildPlace -Recurse -Force | Out-Null 
}

"Creating folders."

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
Copy-Item "documentation\readme.md" (join-path $packageBuildPlace "readme.txt" )
Get-ChildItem $binDir -Filter "*.dll" -Recurse | %{ Copy-Item $_.Fullname $targetsDir }
Copy-Item (Join-Path $binDir "AspectInjector.Broker.dll") $libDir

"Done."

