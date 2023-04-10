#
# Create service bus
#

param (
    [string] $Subscription = "Default Subscription / Directory",

    [string] $ResourceGroupName = "dev-spin-rg",

    [string] $NamespaceName = "dev-spin-bus",

    [string] $Location = "westus2"
)

$ErrorActionPreference = "Stop";

$currentSubscriptionName = (Get-AzContext).Subscription.Name;
if( $currentSubscriptionName -ne $Subscription )
{
    Set-AzContext -SubscriptionName $Subscription;
}

# Query to see if the namespace currently exists
$CurrentNamespace = Get-AzServiceBusNamespace -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName -ErrorAction SilentlyContinue;

# Check if the namespace already exists or needs to be created
if ($CurrentNamespace)
{
    Write-Host "The namespace $NamespaceName already exists in the $Location region:"

    # Report what was found
    Get-AzServiceBusNamespace -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName;
    return;
}

Write-Host "The $NamespaceName namespace does not exist.";
Write-Host "Creating the $NamespaceName namespace in the $Location region...";

New-AzServiceBusNamespace -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName -Location $Location;

$CurrentNamespace = Get-AzServiceBusNamespace -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName;

Write-Host "The $NamespaceName namespace in Resource Group $ResourceGroupName in the $Location region has been successfully created.";
Write-Host "Completed";
