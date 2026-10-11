using UnityEditor;
using System.IO;
using System.Linq;

public class SetupBuildSettings
{
    [MenuItem("Tools/Pasit/Setup Build Settings")]
    public static void Setup()
    {
        string[] preferredOrder = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/StageSelection.unity",
            "Assets/Scenes/Stage1.unity",
            "Assets/Scenes/Stage2.unity",
            "Assets/Scenes/Win.unity",
            "Assets/Scenes/GameOver.unity",
            "Assets/Scenes/Options.unity",
            "Assets/Scenes/Credits.unity"
        };

        var allScenes = Directory.GetFiles("Assets/Scenes", "*.unity")
            .Select(f => f.Replace("\\", "/"))
            .ToList();

        var orderedScenes = preferredOrder
            .Where(f => allScenes.Contains(f))
            .Concat(allScenes.Except(preferredOrder))
            .Select(f => new EditorBuildSettingsScene(f, true))
            .ToArray();

        EditorBuildSettings.scenes = orderedScenes;
        UnityEngine.Debug.Log("Build settings updated with " + orderedScenes.Length + " scenes (MainMenu at index 0).");
    }
}
