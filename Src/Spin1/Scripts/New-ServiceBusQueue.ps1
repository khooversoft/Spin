#
# Create service bus
#

param (
    [string] $Subscription = "Default Subscription / Directory",

    [string] $ResourceGroupName = "dev-spin-rg",

    [string] $NamespaceName = "dev-spin-bus",

    [Parameter(Mandatory=$true)]
    [string] $QueueName
)

$ErrorActionPreference = "Stop";

$currentSubscriptionName = (Get-AzContext).Subscription.Name;
if( $currentSubscriptionName -ne $Subscription )
{
    Set-AzContext -SubscriptionName $Subscription;
}

# Query to see if the namespace currently exists
$CurrentNamespace = Get-AzServiceBusNamespace -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName -ErrorAction SilentlyContinue;

if (!$CurrentNamespace)
{
    Write-Error "The namespace $NamespaceName does not exists";
    return;
}

New-AzServiceBusQueue -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName -Name $QueueName;
