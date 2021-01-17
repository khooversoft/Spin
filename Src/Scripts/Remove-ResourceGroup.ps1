#
# Remove test resource
#

param (
    [string] $Subscription = "Default Subscription / Directory",

    [string] $ResourceGroupName = "dev-spin-rg",

    [string] $Location = "westus2"
)

$ErrorActionPreference = "Stop";

# Switch to subscription if required
$currentSubscriptionName = (Get-AzContext).Subscription.Name;
if( $currentSubscriptionName -ne $Subscription )
{
    Set-AzContext -SubscriptionName $Subscription;
}

$existing = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue;

if( !$existing )
{
    Write-Host "Resource group $ResourceGroupName does not exist";
    return;
}

Remove-AzResourceGroup -Name $ResourceGroupName -Force;
Write-Host "Resource group $ResourceGroupName has been removed";
