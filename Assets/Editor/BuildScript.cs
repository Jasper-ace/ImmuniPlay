using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        Debug.Log("========== BUILD STARTED ==========");

        string buildFolder = Path.Combine(Directory.GetCurrentDirectory(), "build");

        if (!Directory.Exists(buildFolder))
            Directory.CreateDirectory(buildFolder);

        string apkPath = Path.Combine(buildFolder, "ImmuniPlay.apk");

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new System.Exception("No scenes are enabled in Build Settings.");

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("========== BUILD SUCCESS ==========");
            Debug.Log($"APK Location: {apkPath}");
            Debug.Log($"Build Size : {report.summary.totalSize} bytes");
        }
        else
        {
            Debug.LogError("========== BUILD FAILED ==========");
            throw new System.Exception("Android build failed.");
        }
    }
}