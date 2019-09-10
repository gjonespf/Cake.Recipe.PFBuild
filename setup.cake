Environment.SetVariableNames();

// BuildParameters.Tasks.DefaultTask
//     .IsDependentOn("Build");

#load "project-tasks.cake"
#load "project.cake"

BuildParameters.PrintParameters(Context);
ToolSettings.SetToolSettings(context: Context);

// Simplified...
RunTarget(BuildParameters.Target);
