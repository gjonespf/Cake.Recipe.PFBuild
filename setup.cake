Environment.SetVariableNames();

#load project.cake

BuildParameters.PrintParameters(Context);
ToolSettings.SetToolSettings(context: Context);

// Simplified...
Build.RunVanilla();
