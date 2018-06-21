# 
# Add_BuildNumber.ps1
#
$scriptpath = Split-Path $script:MyInvocation.MyCommand.Path

Set-Location $scriptpath

try
{
 (Get-Content RegisterWithDiscoveryService.sql) | 
 ForEach-Object {$_ -replace 'CurrentBuildNumber', $Env:BUILD_BUILDNUMBER} | 
 Set-Content RegisterWithDiscoveryService.sql -Force
 WRITE-OUTPUT $Env:BUILD_BUILDNUMBER
}
catch
{
 Write-Error "Unable to add the BuildNumber to the RegisterWithDiscoveryService.sql script."
 throw
}