#
# Create service bus
#

param (
    [string] $Subscription = "Default Subscription / Directory",

    [string] $ResourceGroupName = "dev-spin-rg",

    [string] $NamespaceName = "dev-spin-bus",

    [string] $Location = "westus2",

    [string] $RuleName = "SendReceive"
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
if (!$CurrentNamespace)
{
    Write-Error "The namespace $NamespaceName already does not exist";
    return;
}

$rule = Get-AzServiceBusAuthorizationRule -ResourceGroupName dev-spin-rg -Namespace "dev-spin-bus" -AuthorizationRuleName $RuleName -ErrorAction SilentlyContinue;

if( $rule )
{
    Write-Host "Rule $RuleName already exist";
    return;
}

Write-Host "Create authorization rule $RuleName";

New-AzServiceBusAuthorizationRule -ResourceGroup $ResourceGroupName -NamespaceName $NamespaceName -AuthorizationRuleName $RuleName -Rights @("Listen","Send");

Write-Host "Completed";
