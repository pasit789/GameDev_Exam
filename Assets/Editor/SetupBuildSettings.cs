using UnityEditor;
using System.IO;
using System.Linq;

public class SetupBuildSettings
{
    [MenuItem("Tools/Pasit/Setup Build Settings")]
    public static void Setup()
    {
        string[] sceneFiles = Directory.GetFiles("Assets/Scenes", "*.unity");
        
        var buildScenes = sceneFiles.Select(file => new EditorBuildSettingsScene(file, true)).ToArray();
        EditorBuildSettings.scenes = buildScenes;
        
        UnityEngine.Debug.Log("Build settings updated with " + buildScenes.Length + " scenes.");
    }
}
