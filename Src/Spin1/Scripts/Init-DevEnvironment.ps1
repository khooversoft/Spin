#
# Initialize dev Azure environment
#

param (
    [string] $Subscription = "Default Subscription / Directory",

    [string] $ResourceGroupName = "dev-spin-rg",

    [string] $Location = "westus2",

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
$secretId = "$Environment-spin-secrets"

Write-Host "Initializing the $Environment environment";

& .\New-ResourceGroup.ps1 -ResourceGroupName $resourceGroupName;

& .\New-ServiceBus.ps1  -ResourceGroupName $resourceGroupName;
& .\Set-ServiceBusAuthorizationRule.ps1 -ResourceGroupName $ResourceGroupName;

& .\New-ServiceBusQueue.ps1 -ResourceGroupName $ResourceGroupName -QueueName "Fe-Node";
& .\New-ServiceBusQueue.ps1 -ResourceGroupName $ResourceGroupName -QueueName "Account-Node";
& .\New-ServiceBusQueue.ps1 -ResourceGroupName $ResourceGroupName -QueueName "Artifact-Node";

& .\Set-ServiceBusSecret.ps1 -ResourceGroupName $ResourceGroupName -SecretId $secretId;

Write-Host "Completed";
