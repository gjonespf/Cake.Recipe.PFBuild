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

#load project.cake

// Simplified...
Build.RunVanilla();
