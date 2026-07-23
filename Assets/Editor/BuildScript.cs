using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        Debug.Log("========== IMMUNIPLAY BUILD START ==========");


        string current = Directory.GetCurrentDirectory();

        Debug.Log("CURRENT DIRECTORY:");
        Debug.Log(current);



        Debug.Log("ACTIVE PLATFORM:");
        Debug.Log(EditorUserBuildSettings.activeBuildTarget);



        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("Switching to Android...");

            bool switched = BuildPipeline.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android
            );

            Debug.Log("Platform switch result: " + switched);
        }



        string buildFolder = Path.Combine(
            current,
            "build"
        );


        Debug.Log("BUILD FOLDER:");
        Debug.Log(buildFolder);



        Directory.CreateDirectory(buildFolder);



        string apkPath = Path.Combine(
            buildFolder,
            "ImmuniPlay.apk"
        );


        Debug.Log("APK PATH:");
        Debug.Log(apkPath);



        string[] scenes = EditorBuildSettings.scenes
            .Where(x => x.enabled)
            .Select(x => x.path)
            .ToArray();



        Debug.Log("SCENE COUNT:");
        Debug.Log(scenes.Length);



        foreach(string scene in scenes)
        {
            Debug.Log("SCENE:");
            Debug.Log(scene);
        }



        if(scenes.Length == 0)
        {
            throw new System.Exception(
                "NO ENABLED SCENES"
            );
        }



        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };


        Debug.Log("STARTING BUILD PLAYER");



        BuildReport report = BuildPipeline.BuildPlayer(options);



        Debug.Log("BUILD RESULT:");
        Debug.Log(report.summary.result);



        Debug.Log("ERRORS:");
        Debug.Log(report.summary.totalErrors);



        if(report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception(
                "BUILD FAILED"
            );
        }


        Debug.Log("APK EXISTS:");
        Debug.Log(File.Exists(apkPath));


        Debug.Log("========== BUILD FINISHED ==========");
    }
}