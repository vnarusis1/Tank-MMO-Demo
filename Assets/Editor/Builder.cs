using UnityEditor;

public class Builder 
{
	static string applicationName = "TankBattle";
	static string targetDirectory = "Builds";

    
    static void BuildPlayerStandAloneMacOSX()
    {  
		BuildPlayer(
			FindEnabledEditorScenes(),
			targetDirectory + System.IO.Path.PathSeparator + applicationName + ".app", 
			BuildTarget.StandaloneOSXIntel,
			BuildOptions.None);
    }
	static void BuildPlayerStandAloneMacOSX64()
    {  
		BuildPlayer(
			FindEnabledEditorScenes(),
			targetDirectory + System.IO.Path.PathSeparator + applicationName + " (64 bit).app", 
			BuildTarget.StandaloneOSXIntel,
			BuildOptions.None);
    }
	public static void BuildStandAloneWindows()
    {  
		BuildPlayer(
			FindEnabledEditorScenes(),
			targetDirectory + System.IO.Path.PathSeparator + "Windows (32 bit)", 
			BuildTarget.StandaloneWindows,
			BuildOptions.None);
    }
	public static void BuildStandAloneWindows64()
    {  
		BuildPlayer(
			FindEnabledEditorScenes(),
			targetDirectory + System.IO.Path.PathSeparator + "Windows (64 bit)", 
			BuildTarget.StandaloneWindows64,
			BuildOptions.None);
    }
	public static void BuildWebPlayer()
	{
		BuildPlayer(
			FindEnabledEditorScenes(),
			targetDirectory + System.IO.Path.PathSeparator + "Web Player",
			BuildTarget.WebPlayer,
			BuildOptions.None);
	}
	public static void BuildPlayer_IOS()
	{
		BuildPlayer(
			FindEnabledEditorScenes(),
			targetDirectory + System.IO.Path.PathSeparator + "iOS",
			BuildTarget.iPhone,
			BuildOptions.None);
	}
	public static void BuildPlayer_Android()
	{
		BuildPlayer(
			FindEnabledEditorScenes(),
			targetDirectory + System.IO.Path.PathSeparator + "Android",
			BuildTarget.Android,
			BuildOptions.None);
	}
	public static void BuildPlayer_BlackBerry()
	{
	}
	
	
	#region Meat & Potatoes
	
	private static void BuildPlayer(string[] sceneList, string targetDirectory, BuildTarget buildTarget, BuildOptions buildOptions)
	{   
		// Switch the project to the target platform
		EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
		
		// Run the build
		string res = BuildPipeline.BuildPlayer(sceneList,targetDirectory,buildTarget,buildOptions);
          
		// Check for a response
		if (res.Length > 0) 
		{
			throw new System.Exception("Build Failed: " + res);
		}
	}
	
	
	private static string[] FindEnabledEditorScenes() 
	{
		System.Collections.Generic.List<string> EditorScenes = new System.Collections.Generic.List<string>();
		foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes) 
		{
			if (!scene.enabled) continue;
			EditorScenes.Add(scene.path);
		}
		return EditorScenes.ToArray();
	}
	
	#endregion
}