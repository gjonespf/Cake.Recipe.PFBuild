#!/usr/bin/pwsh

# Common preinit for all projects
. ./buildscripts/do-preinit.ps1

# Do your project specific pre stuff here

# Need to ensure submodules first
git submodule update

## === TWEAK - Create an initial version file, or build will fail first time ===
function Create-InitialVersionFile {
    $versionPath = "$PSScriptRoot/Cake.Recipe/Cake.Recipe/Content/version.cake"
    if(!(Test-Path $versionPath)) {
        cp ./version.cake $versionPath
    }
}
Create-InitialVersionFile
## === TWEAK - Create an initial version file, or build will fail first time ===

# Ignore this GitTools issue for now
$env:IGNORE_NORMALISATION_GIT_HEAD_MOVE="1"


