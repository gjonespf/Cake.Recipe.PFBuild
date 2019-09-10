// Tasks copied from PFHelpers to reduce dependency

Task("Generate-Version-File-PF")
    // Sets up the artifact directory/build numbers
    //.IsDependentOn("PFInit")    
    .Does(() => {
        //PFBuildVersion = GetPFBuildVersion();
    });

Task("PFInit")
    .Does(() => {
        //PFBuildVersion = GetPFBuildVersion();
    });

// TODO: Is this still even needed?
Task("Publish-Artifacts")
    .IsDependentOn("PFInit")
    .Does(() => {
        //var sourceArtifactPath = MakeAbsolute(Directory("./BuildArtifacts/"));
        if(!string.IsNullOrEmpty(BuildParameters.Paths.Directories.Build.ToString())) {
            Information("Copying artifacts to build artifact path: "+BuildParameters.Paths.Directories.Build);
            EnsureDirectoryExists(BuildParameters.Paths.Directories.Build);

            // var nupkgs = GetFiles(sourceArtifactPath+"/**/*.nupkg");
            // foreach(var filePath in nupkgs)
            // {
            //     CopyFile(filePath, BuildArtifactPath+"/"+filePath.GetFilename());
            // }
        } else {
            Error("No artifact path set!  Cannot publish artifacts!");
        }
    });

Task("Publish-LocalNuget")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("LocalNugetServerUrl");
        var ApiKey = EnvironmentVariable("LocalNugetApiKey");
        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");
        var DestinationName = "Local Nuget";
        var keyExists = !string.IsNullOrEmpty(ApiKey)?"PRESENT":"ABSENT";
        Information($"Publishing to {DestinationName} with source: {SourceUrl} and key: {keyExists}");

        if(string.IsNullOrEmpty(SourceUrl) || string.IsNullOrEmpty(ApiKey)) {
            throw new ApplicationException("Environmental variables 'LocalNugetServerUrl' and 'LocalNugetApiKey' must be set to use this");
        }

        foreach(var nupkgFile in nupkgFiles)
        {
            // Push the package.
            NuGetPush(nupkgFile, new NuGetPushSettings {
                Source = SourceUrl,
                ApiKey = ApiKey
            });
        }
    });

Task("BuildPackage")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    .Does(() =>
{
});

Task("BuildPackagePublish")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    .IsDependentOn("Publish")
    .Does(() => {
	});
