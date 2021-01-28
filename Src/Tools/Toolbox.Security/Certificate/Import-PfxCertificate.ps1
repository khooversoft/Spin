<# 
.NOTES 
    Name: Load PFX certificate into Windows Certificate store
    Author: Kelvin Hoover

.SYNOPSIS 
    Load PFX certificate into the Windows Certificate Store

.PARAMETER CertificateFile
    The *.PFX certificate file to import

.PARAMETER StoreLocation
    Location of the store, "CurrentUser" or "LocalMachine"

.PARAMETER StoreName
    Name of store, default = "My"

.PARAMETER Password
    The password for the PFX file, if not specified, the script will ask for it.

.PARAMETER PrivateKeyAccount
    The NT account to give read access to the PFX private key (default is "NT AUTHORITY\NETWORK SERVICE")

#>

#Requires -RunAsAdministrator

Param (
    [Parameter(Mandatory=$True)]
    [string] $CertificateFile,

    [ValidateSet(“CurrentUser”, "LocalMachine")]
    [String]
    
    $StoreLocation = "LocalMachine",

    [Security.SecureString] $Password,

    [string] $PrivateKeyAccount = 'NT AUTHORITY\NETWORK SERVICE'
)

$ErrorActionPreference = "Stop";

function Import-PfxFile
{
    Param (
        [Parameter(Mandatory=$True)]
        [string] $CertificateFile,

        [Parameter(Mandatory=$True)]
        [String] $StoreLocation,

        [Security.SecureString] $Password
    )

    $pfx = new-object -TypeName System.Security.Cryptography.X509Certificates.X509Certificate2($CertificateFile, $Password, “Exportable,PersistKeySet”);
    if ($Password -eq $null)
    {
        $Password = Read-Host “Enter the pfx password” -assecurestring;
    }

    #$pfx.import($CertificateFile, $Password, “Exportable,PersistKeySet”);
    $store = new-object System.Security.Cryptography.X509Certificates.X509Store("My", $StoreLocation);
    $store.open(“MaxAllowed”);
    $store.add($pfx);
    $store.close();

    Write-Host "Imported $CertificateFile, thumbprint=$($pfx.Thumbprint)." -ForegroundColor Cyan;

    return $pfx;
}

function Set-AccessRights
{
    Param (
        [Parameter(Mandatory=$True)]
        [System.Security.Cryptography.X509Certificates.X509Certificate2] $Pfx,

        [Parameter(Mandatory=$True)]
        [String] $PrivateKeyAccount
    )

    $privKeyCertFile = Get-Item -path "$ENV:ProgramData\Microsoft\Crypto\RSA\MachineKeys\*"  | where {$_.Name -eq $Pfx.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName};

    if( -not $privKeyCertFile )
    {
        Write-Error "Cannot find private key to change permissions for $CertificateName";
        return;
    }

    if( $privKeyCertFile.FullName )
    {
        $fileAcl = (Get-Item -Path $privKeyCertFile.FullName).GetAccessControl("Access") 
        $permission = $PrivateKeyAccount,"Read","Allow" 
        $accessRule = new-object System.Security.AccessControl.FileSystemAccessRule $permission 
        $fileAcl.AddAccessRule($accessRule) 
        Set-Acl $privKeyCertFile.FullName $fileAcl

        Write-Host "Permission set for $($privKeyCertFile.FullName)." -ForegroundColor Cyan;
    }
}

$CertificateFile = Resolve-Path $CertificateFile;
Write-Host "Processing $CertificateFile" -ForegroundColor Cyan;

if( !(Test-Path $CertificateFile) )
{
    Write-Host "Cannot locate $CertificateFile" -ForegroundColor Yellow;
    return;
}

$pfx = Import-PfxFile -CertificateFile $CertificateFile -StoreLocation $StoreLocation -Password $Password;

if( $pfx.PrivateKey )
{
    Set-AccessRights -Pfx $pfx -PrivateKeyAccount $PrivateKeyAccount;
}
