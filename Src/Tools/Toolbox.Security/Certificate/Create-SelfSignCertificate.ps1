<# 
.NOTES 
    Name: Create-Certificate.ps1
    Author: Kelvin Hoover

.SYNOPSIS 
    Execute PowerShell self signed certificate to create certificate

#>

#Requires -RunAsAdministrator
 
Param (
    [ValidateSet(“JwtTest”, "JwtTest2")]
    [string] $Type = "JwtTest2",

    [switch] $Force
)

$ErrorActionPreference = "Stop";

$certTypes = @{
    "JwtTest" = @{
        "File" = "Toolbox.Security.Test.jwt.pfx"
        "DnsNames" = @("Toolbox.Security.Test.jwt.com")
        "CertLocation" = "cert:\LocalMachine\My"
    }
    "JwtTest2" = @{
        "File" = "Toolbox.Security.Test.jwt2.pfx"
        "DnsNames" = @("Toolbox.Security.Test.jwt2.com")
        "CertLocation" = "cert:\LocalMachine\My"
    }
}

Write-Host "Building certificate for $Type";

$selection = $certTypes[$Type];

if( -not $Force )
{
    if( Test-Path $selection.File )
    {
        throw "Cannot continue, file $($selection.File) already exists.  Delete it or use the -Force option";
    }
}

$password = $selection.Password

if( !$password )
{
    $password = Read-Host -Prompt "Enter password for PFX" -AsSecureString;
}
else
{
    $password = ConvertTo-SecureString -String $password -AsPlainText -Force;
}

Remove-Item $selection.File -ErrorAction SilentlyContinue

try
{
    $cert = New-SelfSignedCertificate -DnsName $selection.DnsNames -CertStoreLocation $selection.CertLocation -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider";

    $certPath = "$($selection.CertLocation)\$($cert.Thumbprint)";

    Export-PfxCertificate -Password $password -FilePath $selection.File -Cert $certPath;
}
finally
{
    if( $certPath )
    {
        Remove-Item $certPath;
    }
}
