using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    public static void BuildWindows()
    {
        string[] scenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/GameOver.unity"
        };

        string buildDirectory = "Build";
        if (!System.IO.Directory.Exists(buildDirectory))
        {
            System.IO.Directory.CreateDirectory(buildDirectory);
        }

        string buildPath = buildDirectory + "/VoidProtocol.exe";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed with " + summary.totalErrors + " errors");
            EditorApplication.Exit(1);
        }
    }
}
