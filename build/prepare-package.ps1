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

New-Item $packageBuildPlace -ItemType Directory | Out-Null 
New-Item $targetsDir -ItemType Directory | Out-Null 
New-Item $libDir -ItemType Directory | Out-Null 

"Copying items."

Copy-Item "build\AspectInjector.nuspec" $packageBuildPlace
Copy-Item "build\AspectInjector.targets" $targetsDir
Copy-Item "build\AspectInjector.props" $targetsDir
Get-ChildItem $binDir -Filter "*.dll" -Recurse | %{ Copy-Item $_.Fullname $targetsDir }
Copy-Item (Join-Path $binDir "AspectInjector.Broker.dll") $libDir

"Done."

