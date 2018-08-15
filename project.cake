#load setup.selfbootstrap.cake

// Manual addins, for breaking changes
#load pfhelpers.cake
//#load "nuget:https://nuget.powerfarming.co.nz/api/odata?package=Cake.Recipe.PFHelpers&version=0.2.0"


Task("Publish")
	.IsDependentOn("Publish-Artifacts")
	.IsDependentOn("Publish-LocalNuget")
	.Does(() => {
        Information("TASK: Publish");
	});


//BuildParameters.Tasks.BuildTask.IsDependentOn("Build-Docker");
BuildParameters.Tasks.PackageTask.IsDependentOn("Create-NuGet-Package");
