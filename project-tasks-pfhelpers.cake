// Tasks copied from PFHelpers to reduce dependency
// TODO: Remove need for these...

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

#addin nuget:?package=Newtonsoft.Json&version=11.0.2
using Newtonsoft.Json;
public static string ProjectPropertiesFileName = "properties.json";

public class ProjectProperties
{
    public string ProjectName { get; set; }   
    public string ProjectCodeName { get; set; }   
    public string ProjectDescription { get; set; }
    public string ProjectUrl { get; set; }
    public string SourceControlUrl { get; set; }
    public string ProjectVersioning { get; set; }

    public string ProjectLocalPublicNugetServerUrl { get; set; }
    public string ProjectNugetGithubPackageFeed { get; set; }

    // TODO: Rework to be more generic and non docker focussed
    public string DockerDefaultUser { get;set; }
    public string DockerDefaultRemote { get;set; }
    public string DockerDefaultLocal { get;set; }
    public string DockerDefaultTag { get;set; }

    // TODO: Probably split optionals out into a generic property bag
    public string TeamsWebHook { get;set; }
}

public ProjectProperties LoadProjectProperties(DirectoryPath rootPath = null)
{
    if(rootPath == null)
        rootPath = Directory(".");
        
    // TODO: Some defaults from what we know?
    ProjectProperties props = new ProjectProperties(){

    };
    var propertiesFilePath = $"{rootPath.FullPath}/{ProjectPropertiesFileName}";
    if(FileExists(propertiesFilePath)) {
        var jsonData = String.Join(System.Environment.NewLine, System.IO.File.ReadAllLines(propertiesFilePath));
        props = JsonConvert.DeserializeObject<ProjectProperties>(jsonData);
    } else {
        Console.WriteLine($"Couldn't find properties file (using: '{propertiesFilePath}')");
    }

    // Defaults from environment otherwise
    props.ProjectName = props.ProjectName ?? EnvironmentVariable("ProjectName");
    props.ProjectCodeName = props.ProjectCodeName ?? EnvironmentVariable("ProjectCodeName");
    props.ProjectDescription = props.ProjectDescription ?? EnvironmentVariable("ProjectDescription");
    props.ProjectUrl = props.ProjectUrl ?? EnvironmentVariable("ProjectUrl");
    props.ProjectVersioning = props.ProjectVersioning ?? EnvironmentVariable("ProjectVersioning");
    props.ProjectLocalPublicNugetServerUrl = props.ProjectLocalPublicNugetServerUrl ?? EnvironmentVariable("ProjectLocalPublicNugetServerUrl");
    props.ProjectNugetGithubPackageFeed = props.ProjectNugetGithubPackageFeed ?? EnvironmentVariable("ProjectNugetGithubPackageFeed");

    props.TeamsWebHook = props.TeamsWebHook ?? EnvironmentVariable("TeamsWebHook");
    
    return props;
}

Setup<ProjectProperties>(setupContext => 
{
    try {
        Verbose("ProjectProperties - Setup");
        return LoadProjectProperties(null);
    } catch(Exception ex) {
        Error("ProjectProperties - Exception while setting up ProjectProperties: " +ex.Dump());
        return null;
    }
});

Task("ConfigureProjectProperties")
//    .Does(() => {
    .Does<ProjectProperties>(props => {

        // Set argument vars from ProjectProperties where applicable


        // Set env vars from ProjectProperties where applicable?

});


Task("Generate-Version-File-PF")
    // Sets up the artifact directory/build numbers
    //.IsDependentOn("PFInit")    
    .Does(() => {
        //PFBuildVersion = GetPFBuildVersion();
    });

Task("PFInit")
    .IsDependentOn("ConfigureProjectProperties")
    .Does(() => {
        //PFBuildVersion = GetPFBuildVersion();
    });

// TODO: Is this still even needed?
Task("Publish-Artifacts")
    .IsDependentOn("PFInit")
    .Does(() => {
        //var defaultArtifactPath = MakeAbsolute(Directory("./BuildArtifacts/"));
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
    .IsDependentOn("PFInit")
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

Task("Publish-LocalPublicNuget")
    .IsDependentOn("PFInit")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("ProjectLocalPublicNugetServerUrl");
        var ApiKey = EnvironmentVariable("LocalPublicNugetApiKey");
        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");
        var DestinationName = "Local Public Nuget";
        var keyExists = !string.IsNullOrEmpty(ApiKey)?"PRESENT":"ABSENT";
        Information($"Publishing to {DestinationName} with source: {SourceUrl} and key: {keyExists}");

        if(string.IsNullOrEmpty(SourceUrl) || string.IsNullOrEmpty(ApiKey)) {
            throw new ApplicationException("Environmental variables 'ProjectLocalPublicNugetServerUrl' and 'LocalPublicNugetApiKey' must be set to use this");
        }

        foreach(var nupkgFile in nupkgFiles)
        {
            // Push the package.
            NuGetPush(nupkgFile, new NuGetPushSettings {
                Source = SourceUrl,
                ApiKey = ApiKey,
                ArgumentCustomization = (args) => {
                    return args;
                }
            });
        }
    })
    .OnError(exception =>
    {
        // Let's not abort the build completely if the push fails, as it may be a conflict
        // TODO: Identify conflict?
    });

Task("Publish-GitHubNuget")
    .IsDependentOn("PFInit")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("ProjectNugetGitHubPackageFeed");
        var ApiUser = EnvironmentVariable("GITHUB_USERNAME");
        var ApiKey = EnvironmentVariable("GITHUB_API_TOKEN");
        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");
        var DestinationName = "GitHub Nuget";
        var keyExists = !string.IsNullOrEmpty(ApiKey)?"PRESENT":"ABSENT";
        Information($"Publishing to {DestinationName} with source: {SourceUrl} and key: {keyExists}");

        if(string.IsNullOrEmpty(SourceUrl) || string.IsNullOrEmpty(ApiKey)) {
            throw new ApplicationException("Environmental variables 'ProjectNugetGitHubPackageFeed', 'GITHUB_USERNAME' and 'GITHUB_API_TOKEN' must be set to use this");
        }

        foreach(var nupkgFile in nupkgFiles)
        {
            // Push the package.
            NuGetPush(nupkgFile, new NuGetPushSettings {
                Source = SourceUrl,
                ApiKey = ApiKey,
                ArgumentCustomization = (args) => {
                    return args;
                }
            });
        }
    })
    .OnError(exception =>
    {
        // Let's not abort the build completely if the push fails, as it may be a conflict
        // TODO: Identify conflict?
    });


