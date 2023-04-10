#
# Get service bus account secret and update secret file
#

param (
    [string] $Subscription = "Default Subscription / Directory",

    [string] $ResourceGroupName = "dev-spin-rg",

    [string] $NamespaceName = "dev-spin-bus",

    [string] $SecretId = "dev-spin-secrets"
)

$ErrorActionPreference = "Stop";

# Switch to subscription if required
$currentSubscriptionName = (Get-AzContext).Subscription.Name;
if( $currentSubscriptionName -ne $Subscription )
{
    Set-AzContext -SubscriptionName $Subscription;
}

$keys = Get-AzServiceBusKey -ResourceGroupName $ResourceGroupName -Namespace $NamespaceName -Name SendReceive;

$secretValue = $keys.PrimaryKey;
if ( !$secretValue )
{
    Write-Host "The PrimaryKey does not  exist";
    return;
}

Write-Host "Setting key value to secret file $SecretId";

$jsonHash = @{
    "BusNamespace" = @{
        "AccessKey" = $secretValue
    }
}

$json = $jsonHash | ConvertTo-Json;
$secretFile = [System.IO.Path]::Combine($env:APPDATA, "Microsoft\UserSecrets", $SecretId, "secrets.json");

$folder = Split-Path -Parent -Path $secretFile;

if( -not (test-path $folder) )
{
    New-Item -Path $folder -ItemType Directory;
}

$json | Out-File -FilePath $secretFile -Force;

Write-Host "Write secret file $secretFile";
Write-Host "Completed";
