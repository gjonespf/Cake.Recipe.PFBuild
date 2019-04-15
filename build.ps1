#!/usr/bin/pwsh
##########################################################################
# This is the Cake bootstrapper script for PowerShell.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

<#
.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.
.DESCRIPTION
This Powershell script will download NuGet if missing, restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.
.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Verbosity
Specifies the amount of information to be displayed.
.PARAMETER Experimental
Tells Cake to use the latest Roslyn release.
.PARAMETER WhatIf
Performs a dry run of the build script.
No tasks will be executed.
.PARAMETER Mono
Tells Cake to use the Mono scripting engine.
.PARAMETER SkipToolPackageRestore
Skips restoring of packages.
.PARAMETER ScriptArgs
Remaining arguments are added here.
.LINK
http://cakebuild.net
#>

[CmdletBinding()]
Param(
    [string]$Script = "setup.cake",
    [string]$Target = "Default",
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = "Verbose",
    [switch]$Experimental,
    [Alias("DryRun","Noop")]
    [switch]$WhatIf,
    [switch]$Mono,
    [switch]$SkipToolPackageRestore,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

[Reflection.Assembly]::LoadWithPartialName("System.Security") | Out-Null
function MD5HashFile([string] $filePath)
{
    if ([string]::IsNullOrEmpty($filePath) -or !(Test-Path $filePath -PathType Leaf))
    {
        return $null
    }

    [System.IO.Stream] $file = $null;
    [System.Security.Cryptography.MD5] $md5 = $null;
    try
    {
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $file = [System.IO.File]::OpenRead($filePath)
        return [System.BitConverter]::ToString($md5.ComputeHash($file))
    }
    finally
    {
        if ($file -ne $null)
        {
            $file.Dispose()
        }
    }
}

Write-Host "Preparing to run build script..."

if(!$PSScriptRoot){
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
$CAKE_EXE = Join-Path $TOOLS_DIR "Cake/Cake.exe"
$NUGET_URL = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"
$PACKAGES_CONFIG_MD5 = Join-Path $TOOLS_DIR "packages.config.md5sum"

# Should we use mono?
$UseMono = "";
$monoCmd = ""
# FIXME: Temporarily, if mono exists use it until we can sort out pwsh/.net core
if($Mono.IsPresent -or (Get-Command "mono" -ErrorAction SilentlyContinue)) {
    Write-Host -Message "Using the Mono based scripting engine."
    $UseMono = "-mono"
    $monoCmd = "mono"
}

# Should we use the new Roslyn?
$UseExperimental = "";
if($Experimental.IsPresent -and !($Mono.IsPresent)) {
    Write-Verbose -Message "Using experimental version of Roslyn."
    $UseExperimental = "-experimental"
}

# Is this a dry run?
$UseDryRun = "";
if($WhatIf.IsPresent) {
    $UseDryRun = "-dryrun"
}

# Make sure tools folder exists
if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    Write-Verbose -Message "Creating tools directory..."
    New-Item -Path $TOOLS_DIR -Type directory | out-null
}

# Make sure that packages.config exist.
if (!(Test-Path $PACKAGES_CONFIG)) {
    Write-Verbose -Message "Downloading packages.config..."
    try { (New-Object System.Net.WebClient).DownloadFile("http://cakebuild.net/download/bootstrapper/packages", $PACKAGES_CONFIG) } catch {
        Throw "Could not download packages.config."
    }
}

# Try to use Get-Command
if (!($NUGET_EXE) -or !(Test-Path $NUGET_EXE)) {
    Write-Host -Message "No nuget found, trying to search for it in the usual places"
    $NUGET_EXE = (Get-Command "nuget").Source
}

# Try find NuGet.exe in path if not exists
if (!(Test-Path $NUGET_EXE)) {
    Write-Host -Message "Trying to find nuget.exe in PATH..."
    $existingPaths = $Env:Path -Split ';' | Where-Object { (![string]::IsNullOrEmpty($_)) -and (Test-Path $_) }
    $NUGET_EXE_IN_PATH = Get-ChildItem -Path $existingPaths -Filter "nuget.exe" | Select -First 1
    if ($NUGET_EXE_IN_PATH -ne $null -and (Test-Path $NUGET_EXE_IN_PATH.FullName)) {
        Write-Host -Message "Found in PATH at $($NUGET_EXE_IN_PATH.FullName)."
        $NUGET_EXE = $NUGET_EXE_IN_PATH.FullName
    }
}

# Try download NuGet.exe if not exists
if (!(Test-Path $NUGET_EXE)) {
    Write-Verbose -Message "Downloading NuGet.exe..."
    $NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
    try {
        (New-Object System.Net.WebClient).DownloadFile($NUGET_URL, $NUGET_EXE)
    } catch {
        Throw "Could not download NuGet.exe."
    }
}

# Save nuget.exe path to environment to be available to child processed
$NUGET_EXE = (Resolve-Path $NUGET_EXE).Path
$ENV:NUGET_EXE = $NUGET_EXE
Write-Verbose "Using nuget at: $NUGET_EXE"

# Restore tools from NuGet?
if(-Not $SkipToolPackageRestore.IsPresent) {
    Push-Location
    Set-Location $TOOLS_DIR

    # Check for changes in packages.config and remove installed tools if true.
    [string] $md5Hash = MD5HashFile($PACKAGES_CONFIG)
    if((!(Test-Path $PACKAGES_CONFIG_MD5)) -Or
      ($md5Hash -ne (Get-Content $PACKAGES_CONFIG_MD5 ))) {
        Write-Host -Message "Missing or changed package.config hash..."
        foreach($dir in $(gci * -Directory))
        {
            Write-Host "Removing directory: $($dir.FullName)"
            Get-Childitem $dir.FullName -Recurse | Remove-Item -Recurse -Force -Confirm:$false
            Remove-Item -Path $dir -Recurse -Force -Confirm:$false
        }
    }

    Write-Host -Message "Restoring tools from NuGet..."
    #  -Source https://www.myget.org/F/cake/api/v3/index.json"
    $nugetMono = $monoCmd
    # Don't run mono if on native
    if($NUGET_EXE -match "/usr/bin") {
        $nugetMono = ""
    }
    $NuGetOutput = Invoke-Expression "& $nugetMono `"$NUGET_EXE`" install -ExcludeVersion -PreRelease -OutputDirectory `"$TOOLS_DIR`" "

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while restoring NuGet tools."
    }
    else
    {
        $md5Hash | Out-File $PACKAGES_CONFIG_MD5 -Encoding "ASCII"
    }
    Write-Verbose -Message ($NuGetOutput | out-string)
    Pop-Location
}

# Make sure that Cake has been installed.
if (!(Test-Path $CAKE_EXE)) {
    Throw "Could not find Cake.exe at $CAKE_EXE"
}

# Start Cake
Write-Host "Running build script..."

$buildCmd = "& $monoCmd `"$CAKE_EXE`" `"$Script`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" $UseMono $UseDryRun $UseExperimental $ScriptArgs"
Write-Verbose "Using Build Command:"
Write-Verbose "$buildCmd"
Invoke-Expression $buildCmd
# Invoke-Expression "& `"$CAKE_EXE`" `"$Script`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" $UseMono $UseDryRun $UseExperimental $ScriptArgs"

exit $LASTEXITCODE

# SIG # Begin signature block
# MIII5wYJKoZIhvcNAQcCoIII2DCCCNQCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUAUgCeo9EPanRq/w/Qd4kjTHg
# Ag6gggY1MIIGMTCCBRmgAwIBAgIKYf5LqwAAAAACwjANBgkqhkiG9w0BAQUFADBt
# MRIwEAYKCZImiZPyLGQBGRYCbnoxEjAQBgoJkiaJk/IsZAEZFgJjbzEcMBoGCgmS
# JomT8ixkARkWDHBvd2VyZmFybWluZzElMCMGA1UEAxMccG93ZXJmYXJtaW5nLVBG
# TlotU1JWLTAyOC1DQTAeFw0xOTAyMTMwNDIxNTJaFw0yMDAyMTMwNDIxNTJaMIGo
# MRIwEAYKCZImiZPyLGQBGRYCbnoxEjAQBgoJkiaJk/IsZAEZFgJjbzEcMBoGCgmS
# JomT8ixkARkWDHBvd2VyZmFybWluZzEeMBwGA1UECxMVTmV3IFplYWxhbmQgV2hv
# bGVzYWxlMQswCQYDVQQLEwJJVDEVMBMGA1UECxMMU3RhZmYgQWRtaW5zMRwwGgYD
# VQQDDBNBZG1pbiB8IEdhdmluIEpvbmVzMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
# MIIBCgKCAQEAu9tcCzOM8SPtfYncaoBuRDWH1w3yhGUcfUdBWzGMWgslfrxEDZPd
# 3pEg80kKH0InzkiuVHwQYvSzfeOTD+eCvt3Qp5Lfb2n6yxZkJNu56VMYkB6ArRsI
# h2USYmrMd7DeNxZgcZMljnrfh2UdzybPZbcZm0AaSr9A2HFP5Iot12NWAA1UmSEa
# hNTq2SGPBuqaierVoWrETldAxhWKwdn1UiVEbDskMcZ2WTUEBUsAlCEW/ZqhGsww
# ATcm5tHz/7xqX15j7r+eCzqOGpHBB9NB/eBk8rki4GSwtaydbe4dfxVlh4dIcZNC
# 7i/bPf85Yzb0dAXxMYr77XZbCGAZcUkMrQIDAQABo4IClTCCApEwJQYJKwYBBAGC
# NxQCBBgeFgBDAG8AZABlAFMAaQBnAG4AaQBuAGcwCwYDVR0PBAQDAgeAMBMGA1Ud
# JQQMMAoGCCsGAQUFBwMDMB0GA1UdDgQWBBRaW+4O0YIwZIgjx01XkpL/b/rgkjAf
# BgNVHSMEGDAWgBTDKX3ixQYwhJKqfUODbUNBL5OTgTCB6QYDVR0fBIHhMIHeMIHb
# oIHYoIHVhoHSbGRhcDovLy9DTj1wb3dlcmZhcm1pbmctUEZOWi1TUlYtMDI4LUNB
# LENOPVBGTlotU1JWLTAyOCxDTj1DRFAsQ049UHVibGljJTIwS2V5JTIwU2Vydmlj
# ZXMsQ049U2VydmljZXMsQ049Q29uZmlndXJhdGlvbixEQz1wb3dlcmZhcm1pbmcs
# REM9Y28sREM9bno/Y2VydGlmaWNhdGVSZXZvY2F0aW9uTGlzdD9iYXNlP29iamVj
# dENsYXNzPWNSTERpc3RyaWJ1dGlvblBvaW50MIHYBggrBgEFBQcBAQSByzCByDCB
# xQYIKwYBBQUHMAKGgbhsZGFwOi8vL0NOPXBvd2VyZmFybWluZy1QRk5aLVNSVi0w
# MjgtQ0EsQ049QUlBLENOPVB1YmxpYyUyMEtleSUyMFNlcnZpY2VzLENOPVNlcnZp
# Y2VzLENOPUNvbmZpZ3VyYXRpb24sREM9cG93ZXJmYXJtaW5nLERDPWNvLERDPW56
# P2NBQ2VydGlmaWNhdGU/YmFzZT9vYmplY3RDbGFzcz1jZXJ0aWZpY2F0aW9uQXV0
# aG9yaXR5MD8GA1UdEQQ4MDagNAYKKwYBBAGCNxQCA6AmDCRhZG1pbi5nYXZpbi5q
# b25lc0Bwb3dlcmZhcm1pbmcuY28ubnowDQYJKoZIhvcNAQEFBQADggEBAHFQmVNg
# E6up95QxHQAhxfQ+pVwwuEXT6NGSp/HbTSmi7JCwdv8AOijZp+OyPSnzkuVD3UYZ
# JYIIE7Vj0hCAoougxzQYFMIwYctZpahQ//I+kvQpVC6xALDwY04AYYYMG5Wf8ORg
# 1+6YlYDpsiD4PlOuEtUs4ZdzT+d2tzbaxXcdYk7vVnLX16RLZyu+jPpJ/5bK5sKr
# mgun+Rp6/oPXwcYahIl+anjmvJ/5lX47KdE7oJCM9MNUtnztZOG/NJoKSENU8YC0
# tVaWJUMRHZtmYlZ9kBDG3HEyPeGKNIlGgEwyAXfPREjAHcwxVJThMEijrpr01PSA
# AYD7FbSD6VrlKLoxggIcMIICGAIBATB7MG0xEjAQBgoJkiaJk/IsZAEZFgJuejES
# MBAGCgmSJomT8ixkARkWAmNvMRwwGgYKCZImiZPyLGQBGRYMcG93ZXJmYXJtaW5n
# MSUwIwYDVQQDExxwb3dlcmZhcm1pbmctUEZOWi1TUlYtMDI4LUNBAgph/kurAAAA
# AALCMAkGBSsOAwIaBQCgeDAYBgorBgEEAYI3AgEMMQowCKACgAChAoAAMBkGCSqG
# SIb3DQEJAzEMBgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3
# AgEVMCMGCSqGSIb3DQEJBDEWBBQ9M5eLtx+9sllJhnpeev7pUHuJtjANBgkqhkiG
# 9w0BAQEFAASCAQAbCRAdhmUOgVAg5Dd/muPNtEehMcueGyXSJgIVUp2fnW+/DhH1
# kWdeqUhxCYnxsmwyWSOxrhLVLUPLuQlEky/XzPzzAengWdN5u+iE4g2RX4rTg8U7
# 9LllAdsM8RxKUKncLUZphJhOlHCuViqHUe0bO2tN2uq1ctCKmV+C555OEPEcrkRP
# 8YiCKa/rlEtyzXa+2hEbqeYkRZU+hHvtQ2uGO1BC2YZB8Pw5Db6ISTE0WMattmFx
# vRcvUQPeWFu0e3iIlQ7I8Xydl+KcFbUvBg7d9Cez3njMATj23UrmmmTTlfBSgoyv
# S8bbB6qI9hV0zdQGQxz7mKYqJm5hrbO1C3EH
# SIG # End signature block
