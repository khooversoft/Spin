#
# Remove dev Azure environment
#

param (
    [string] $Subscription = "Default Subscription / Directory",

    [string] $Environment = "dev"
)

$ErrorActionPreference = "Stop";

# Switch to subscription if required
$currentSubscriptionName = (Get-AzContext).Subscription.Name;
if( $currentSubscriptionName -ne $Subscription )
{
    Set-AzContext -SubscriptionName $Subscription;
}

$resourceGroupName = "$Environment-spin-rg";

Write-Host "Removing the $Environment environment by removing resource group $resourceGroupName";


& .\Remove-ResourceGroup.ps1 -ResourceGroupName $resourceGroupName;

Write-Host "Completed";
