using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        Debug.Log("========== IMMUNIPLAY ANDROID BUILD ==========");

        // Force Android platform
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("Switching platform to Android...");

            BuildPipeline.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android
            );
        }


        // Create build folder
        string buildFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "build"
        );

        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }


        string apkPath = Path.Combine(
            buildFolder,
            "ImmuniPlay.apk"
        );


        Debug.Log("APK OUTPUT:");
        Debug.Log(apkPath);


        // Get enabled scenes
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();


        if (scenes.Length == 0)
        {
            throw new System.Exception(
                "No scenes enabled in Build Settings."
            );
        }


        Debug.Log("Scenes:");

        foreach(string scene in scenes)
        {
            Debug.Log(scene);
        }


        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,

            locationPathName = apkPath,

            target = BuildTarget.Android,

            options = BuildOptions.None
        };


        Debug.Log("Starting Unity Android Build...");


        BuildReport report =
            BuildPipeline.BuildPlayer(options);


        Debug.Log(
            "Build Result: " + report.summary.result
        );


        if(report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception(
                "Android build failed."
            );
        }


        if(!File.Exists(apkPath))
        {
            throw new System.Exception(
                "APK was not created:\n" + apkPath
            );
        }


        FileInfo file = new FileInfo(apkPath);


        Debug.Log("==============================");
        Debug.Log("BUILD SUCCESS");
        Debug.Log("APK:");
        Debug.Log(apkPath);
        Debug.Log("SIZE:");
        Debug.Log(file.Length + " bytes");
        Debug.Log("==============================");
    }
}