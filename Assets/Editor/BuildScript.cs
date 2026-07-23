using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        Debug.Log("========== IMMUNIPLAY ANDROID BUILD START ==========");


        // Current project path
        string projectPath = Directory.GetCurrentDirectory();

        Debug.Log("Project Path:");
        Debug.Log(projectPath);



        // Switch to Android platform
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("Switching platform to Android...");

            bool result = EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android
            );

            Debug.Log("Platform switch result: " + result);
        }



        // Create build folder
        string buildFolder = Path.Combine(
            projectPath,
            "build"
        );


        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }



        // APK output path
        string apkPath = Path.Combine(
            buildFolder,
            "ImmuniPlay.apk"
        );


        Debug.Log("APK Output:");
        Debug.Log(apkPath);



        // Get enabled scenes
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();



        Debug.Log("Scenes Count:");
        Debug.Log(scenes.Length);



        foreach(string scene in scenes)
        {
            Debug.Log("Scene:");
            Debug.Log(scene);
        }



        if (scenes.Length == 0)
        {
            throw new System.Exception(
                "No scenes found in Build Settings!"
            );
        }



        // Build settings
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,

            locationPathName = apkPath,

            target = BuildTarget.Android,

            options = BuildOptions.None
        };



        Debug.Log("========== START BUILD PLAYER ==========");



        BuildReport report = BuildPipeline.BuildPlayer(options);



        Debug.Log("========== BUILD RESULT ==========");

        Debug.Log(report.summary.result);

        Debug.Log("Errors:");
        Debug.Log(report.summary.totalErrors);



        if(report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception(
                "Android build failed!"
            );
        }



        if(!File.Exists(apkPath))
        {
            throw new System.Exception(
                "APK was not created:\n" + apkPath
            );
        }



        Debug.Log("APK CREATED SUCCESSFULLY:");

        Debug.Log(apkPath);



        Debug.Log("========== IMMUNIPLAY BUILD COMPLETE ==========");
    }
}