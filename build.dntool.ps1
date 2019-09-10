
# TODO: Prerequisites
[CmdletBinding()]
Param(
    [string]$Script = "setup.cake",
    [string]$Target = "build",
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity,
    [switch]$ShowDescription,
    [Alias("WhatIf", "Noop")]
    [switch]$DryRun,
    [switch]$Experimental,
    [version]$CakeVersion = '0.33.0',
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

if(!($env:PATH -match ".dotnet") -and $IsLinux) {
    if(Test-Path "~/.dotnet/tools") {
        $toolsPath = (Resolve-Path "~/.dotnet/tools").Path
        $env:PATH = $env:PATH + ":$toolsPath"
    } else {
        Write-Warning "Couldn't find dotnet core tools directory"
    }
}

# TODO: Pull in specific version of cake listed in $CakeVersion
if(Get-Command "dotnet-cake" -ErrorAction SilentlyContinue) {
    Write-Information "Running dotnet-cake"
    dotnet-cake $Script --target=$Target
} else {
    Write-Error "Could not find dotnet-cake to run build script"
    Write-Information "Using PATH: $($env:PATH)"
}
