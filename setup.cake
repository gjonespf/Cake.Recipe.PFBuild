#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease
#load pfhelpers.cake

//#define CustomGitVersionTool
//private const string GitVersionTool = "#tool nuget:?package=GitVersion.CommandLine.DotNetCore&version=4.0.0-netstandard0001";

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "Cake.Recipe.PF",
                            repositoryOwner: "gjones@powerfarming.co.nz",
                            repositoryName: "Cake.Recipe.PF",
                            nugetFilePath: "Cake.Recipe.nuget"
                            shouldPostToMicrosoftTeams: true
                            );

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

Task("Init")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File")
	.Does(() => {
		Information("Init");
    });

BuildParameters.Tasks.CleanTask
    .IsDependentOn("PFInit")
    // .IsDependentOn("PFInit-Clean")
    .IsDependentOn("Generate-Version-File")
    .Does(() => {
    });

BuildParameters.Tasks.RestoreTask.Task.Actions.Clear();
BuildParameters.Tasks.RestoreTask
	//.IsDependentOn("Package-Docker")
    .Does(() => {
    });

// BuildParameters.Tasks.PackageTask.Task.Actions.Clear();
// BuildParameters.Tasks.PackageTask
// 	.IsDependentOn("Package-Docker");

// BuildParameters.Tasks.BuildTask.Task.Actions.Clear();
// BuildParameters.Tasks.BuildTask
// 	.IsDependentOn("Build-Docker");

Task("Publish")
	.IsDependentOn("Publish-Artifacts")
	.IsDependentOn("Publish-LocalNuget")
	.Does(() => {
	});



// Simplified...
Build.RunNuGet();
