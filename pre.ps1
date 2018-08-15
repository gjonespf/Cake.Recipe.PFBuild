## === TWEAK - Create an initial version file, or build will fail first time ===
function Create-InitialVersionFile {
    $versionPath = "$PSScriptRoot/Cake.Recipe/Cake.Recipe/Content/version.cake"
    if(!(Test-Path $versionPath)) {
        cp ./version.cake $versionPath
    }
}
Create-InitialVersionFile
## === TWEAK - Create an initial version file, or build will fail first time ===
