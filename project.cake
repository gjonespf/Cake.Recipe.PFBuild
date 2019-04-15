#load setup.selfbootstrap.cake

// Manual addins, for breaking changes
#load pfhelpers.cake
//#load "nuget:https://nuget.powerfarming.co.nz/api/odata?package=Cake.Recipe.PFHelpers&version=0.2.0"

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "Cake.Recipe.PF",
                            repositoryOwner: "gjones@powerfarming.co.nz",
                            repositoryName: "Cake.Recipe.PF",
                            nuspecFilePath: "Cake.Recipe/Cake.Recipe/Cake.Recipe.PF.nuspec",
                            shouldPostToMicrosoftTeams: true,
                            shouldRunGitVersion: true,
                            shouldRunIngestIssues: true
                            );

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

Task("Init")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-Cake")
    .IsDependentOn("Generate-Version-File-PF");
    
 BuildParameters.Tasks.BuildTask
     .IsDependentOn("Init");

BuildParameters.Tasks.PackageTask
	.IsDependentOn("Generate-Version-File-PF")
	.IsDependentOn("Create-NuGet-Package")
	;

Task("Test")
    .Does(() => {
        Information("TASK: Test");
    });

Task("Publish")
	.IsDependentOn("Generate-Version-File-PF")
	.IsDependentOn("Publish-Artifacts")
	.IsDependentOn("Publish-LocalNuget")
	.Does(() => {
        Information("TASK: Publish");
	});


