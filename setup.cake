Environment.SetVariableNames();

BuildParameters.PrintParameters(Context);
ToolSettings.SetToolSettings(context: Context);

#load project.cake

// Simplified...
Build.RunVanilla();
