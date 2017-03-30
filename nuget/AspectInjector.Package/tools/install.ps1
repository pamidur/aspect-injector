param($installPath, $toolsPath, $package, $project)

foreach ($reference in $project.Object.References)
{
    if($reference.Name -eq "AspectInjector.Broker")
    {
        if($reference.CopyLocal -eq $true)
        {
            $reference.CopyLocal = $false;
        }
    }
}