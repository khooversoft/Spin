param(
    [ValidateSet('reset', 'package', 'create')]
    [string] $Cmd
)

if( $cmd -eq 'reset')
{
    & .\bin\Debug\net7.0\SpinClusterCmd.exe load D:\Sources\Spin\Src\Spin2\SpinClusterCmd\Data\Scenario_001_Setup.json
    exit;
}

if( $cmd -eq 'package')
{
    & .\bin\Debug\net7.0\SpinClusterCmd.exe package create D:\Sources\Spin\Src\Spin2\SpinClusterCmd\Data\Load-smartc-v1-package.json
    exit;
}

if( $cmd -eq 'create')
{
    & .\bin\Debug\net7.0\SpinClusterCmd.exe schedule add D:\Sources\Spin\Src\Spin2\SpinClusterCmd\Data\CreateLoan-Command.json
    exit;
}
