using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class ShooterBuild
{
    public static void ConfigureAndBuild()
    {
        ShooterSetup.Configure();
        Build();
    }

    [MenuItem("Tools/Space Shooter/Build Windows Demo")]
    public static void Build()
    {
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/Week5Lab.unity" },
            locationPathName = "Builds/Windows/MeteorWatch.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Windows build failed: " + report.summary.result);
    }
}
