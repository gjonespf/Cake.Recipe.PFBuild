//#load "nuget:https://nuget.powerfarming.co.nz/api/odata?package=Cake.Recipe&prerelease"
//#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease

#load setup.selfbootstrap.cake

//#load pfhelpers.cake
#load "nuget:https://nuget.powerfarming.co.nz/api/odata?package=Cake.Recipe.PFHelpers&version=0.2.0"

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "Cake.Recipe.PF",
                            repositoryOwner: "gjones@powerfarming.co.nz",
                            repositoryName: "Cake.Recipe.PF",
                            nuspecFilePath: "Cake.Recipe/Cake.Recipe/Cake.Recipe.PF.nuspec",
                            shouldPostToMicrosoftTeams: true
                            );

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

Task("Init")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-PF")
    .IsDependentOn("Generate-Version-File-Cake")
	.Does(() => {
		Information("Init");
    });

BuildParameters.Tasks.CleanTask
    .IsDependentOn("PFInit")
    // .IsDependentOn("PFInit-Clean")
    .IsDependentOn("Generate-Version-File-PF")
    .IsDependentOn("Generate-Version-File-Cake")
    .Does(() => {
    });

Task("Generate-Version-File-Cake")
    .Does(() => {
        var buildMetaDataCodeGen = TransformText(@"
        public class BuildMetaData
        {
            public static string Date { get; } = ""<%date%>"";
            public static string Version { get; } = ""<%version%>"";
        }",
        "<%",
        "%>"
        )
   .WithToken("date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
   .WithToken("version", BuildParameters.Version.SemVersion)
   .ToString();

    System.IO.File.WriteAllText(
        "./Cake.Recipe/Cake.Recipe/Content/version.cake",
        buildMetaDataCodeGen
        );
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
