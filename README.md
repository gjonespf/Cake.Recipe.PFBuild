# Cake.Recipe.PFBuild

An opinionated build of Cake.Recipe (with upstream pull requests from many contributors) focussing on dotnet core support and self-build.

```pwsh
# Current test build:
./build.ps1 -target build

# Test Dotnet Core build:
pwsh ./build.dntool.ps1 -target init

# Test Dotnet Core build:
dotnet-cake setup.cake --target="build" --verbosity=diagnostic
```


