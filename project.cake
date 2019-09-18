
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
	.IsDependentOn("Publish-LocalPublicNuget")
	//.IsDependentOn("Publish-GitHubNuget")
	.Does(() => {
        Information("TASK: Publish");
	});


