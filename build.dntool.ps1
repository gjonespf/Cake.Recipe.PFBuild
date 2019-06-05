
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
    [switch]$Mono,
    [version]$CakeVersion = '0.27.2',
    [switch]$UseNetCore,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

dotnet-cake $Script --target=$Target
