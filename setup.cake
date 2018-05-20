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
                            nuspecFilePath: "Cake.Recipe/Cake.Recipe/Cake.Recipe.nuspec",
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
	//.IsDependentOn("ATask")
    .Does(() => {
    });

// BuildParameters.Tasks.PackageTask.Task.Actions.Clear();
// BuildParameters.Tasks.PackageTask
// 	//.IsDependentOn("ATask")
//     .Does(() => {
//         Information("TASK: Package");
// 	});

BuildParameters.Tasks.BuildTask.Task.Actions.Clear();
BuildParameters.Tasks.BuildTask
	.Does(() => {
        Information("TASK: Build");
	});

Task("Publish")
	.IsDependentOn("Publish-Artifacts")
	.IsDependentOn("Publish-LocalNuget")
	.Does(() => {
        Information("TASK: Publish");
	});



// Simplified...
Build.RunNuGet();
