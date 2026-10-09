using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Pasit;

public class SceneSetupAutomation
{
    [MenuItem("Tools/Pasit/Setup All Scenes and Assets (Q1-Q5)")]
    public static void SetupAll()
    {
        EnsureFolders();
        CreateMaterials();
        CreatePrefabs();

        SetupMainMenu();
        SetupStageSelection();
        SetupOptions();
        SetupCredits();
        SetupWin();
        SetupGameOver();
        SetupStage1();
        SetupStage2();

        SetupBuildSettings.Setup();

        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("==== ALL SCENES AND ASSETS SUCCESSFULLY SETUP FOR Q1 - Q5! ====");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    private static void CreateMaterials()
    {
        GetOrCreateMaterial("Mat_AddTime", new Color(0.2f, 0.9f, 0.2f));       // Green
        GetOrCreateMaterial("Mat_ReduceTime", new Color(0.9f, 0.2f, 0.2f));    // Red
        GetOrCreateMaterial("Mat_AddScore", new Color(1.0f, 0.85f, 0.1f));     // Gold
        GetOrCreateMaterial("Mat_ReduceScore", new Color(0.6f, 0.1f, 0.8f));   // Purple
        GetOrCreateMaterial("Mat_Platform", new Color(0.35f, 0.4f, 0.45f));   // Dark Slate
        GetOrCreateMaterial("Mat_Bridge", new Color(0.65f, 0.7f, 0.75f));     // Light Slate
        GetOrCreateMaterial("Mat_Player", new Color(0.1f, 0.5f, 0.9f));       // Blue
        AssetDatabase.SaveAssets();
    }

    private static void CreatePrefabs()
    {
        CreateItemPrefab("Item_AddTime", PrimitiveType.Sphere, "Mat_AddTime", Item.ItemType.AddTime, new Vector3(0.8f, 0.8f, 0.8f));
        CreateItemPrefab("Item_ReduceTime", PrimitiveType.Sphere, "Mat_ReduceTime", Item.ItemType.ReduceTime, new Vector3(0.8f, 0.8f, 0.8f));
        CreateItemPrefab("Item_AddScore", PrimitiveType.Cube, "Mat_AddScore", Item.ItemType.AddScore, new Vector3(0.7f, 0.7f, 0.7f));
        CreateItemPrefab("Item_ReduceScore", PrimitiveType.Cube, "Mat_ReduceScore", Item.ItemType.ReduceScore, new Vector3(0.7f, 0.7f, 0.7f));
    }

    private static void CreateItemPrefab(string name, PrimitiveType shape, string matName, Item.ItemType type, Vector3 scale)
    {
        string path = "Assets/Prefabs/" + name + ".prefab";
        GameObject go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.transform.localScale = scale;

        Collider col = go.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/" + matName + ".mat");
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;

        Item itemComp = go.AddComponent<Item>();
        itemComp.type = type;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    private static (GameObject camera, GameObject light, Canvas canvas, SceneController sceneCtrl) CreateSceneBase(string title)
    {
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        Camera cameraComp = cam.AddComponent<Camera>();
        cameraComp.clearFlags = CameraClearFlags.SolidColor;
        cameraComp.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
        cam.AddComponent<AudioListener>();
        cam.transform.position = new Vector3(0, 1, -10);

        GameObject light = new GameObject("Directional Light");
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1.0f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject ctrlGo = new GameObject("SceneController");
        SceneController sceneCtrl = ctrlGo.AddComponent<SceneController>();

        CreateText(canvas.transform, title, new Vector2(0, 380), new Vector2(1000, 100), 54, Color.white, TextAlignmentOptions.Center);

        return (cam, light, canvas, sceneCtrl);
    }

    private static TextMeshProUGUI CreateText(Transform parent, string text, Vector2 anchoredPos, Vector2 size, float fontSize, Color color, TextAlignmentOptions align)
    {
        GameObject textGo = new GameObject("Text_" + text);
        textGo.transform.SetParent(parent, false);
        RectTransform rt = textGo.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        return tmp;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size, SceneController sceneCtrl, string targetScene, bool isExit = false)
    {
        GameObject btnGo = new GameObject("Btn_" + label);
        btnGo.transform.SetParent(parent, false);
        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = btnGo.AddComponent<Image>();
        img.color = new Color(0.2f, 0.45f, 0.8f);

        Button btn = btnGo.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.25f, 0.5f, 0.85f);
        colors.highlightedColor = new Color(0.35f, 0.65f, 1f);
        colors.pressedColor = new Color(0.15f, 0.35f, 0.65f);
        btn.colors = colors;

        CreateText(btnGo.transform, label, Vector2.zero, size, 32, Color.white, TextAlignmentOptions.Center);

        if (isExit)
        {
            UnityAction action = new UnityAction(sceneCtrl.ExitGame);
            UnityEventTools.AddPersistentListener(btn.onClick, action);
        }
        else
        {
            UnityAction<string> action = new UnityAction<string>(sceneCtrl.LoadScene);
            UnityEventTools.AddStringPersistentListener(btn.onClick, action, targetScene);
        }

        return btn;
    }

    private static void SetupMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (_, _, canvas, ctrl) = CreateSceneBase("958321 Game Development 3 - Practical Examination");

        CreateText(canvas.transform, "Student Code: 652110... | Name: Pasit", new Vector2(0, 290), new Vector2(800, 50), 30, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.Center);

        CreateButton(canvas.transform, "Start", new Vector2(0, 120), new Vector2(340, 70), ctrl, "StageSelection");
        CreateButton(canvas.transform, "Options", new Vector2(0, 30), new Vector2(340, 70), ctrl, "Options");
        CreateButton(canvas.transform, "Credits", new Vector2(0, -60), new Vector2(340, 70), ctrl, "Credits");
        CreateButton(canvas.transform, "Exit", new Vector2(0, -150), new Vector2(340, 70), ctrl, "", true);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    private static void SetupStageSelection()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (_, _, canvas, ctrl) = CreateSceneBase("Stage Selection");

        CreateButton(canvas.transform, "Stage 1 - Capsule Player", new Vector2(0, 80), new Vector2(420, 80), ctrl, "Stage1");
        CreateButton(canvas.transform, "Stage 2 - Humanoid Player", new Vector2(0, -20), new Vector2(420, 80), ctrl, "Stage2");
        CreateButton(canvas.transform, "Back to Main Menu", new Vector2(0, -140), new Vector2(340, 70), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/StageSelection.unity");
    }

    private static void SetupOptions()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (_, _, canvas, ctrl) = CreateSceneBase("Options");

        CreateText(canvas.transform, "Volume Controls", new Vector2(0, 160), new Vector2(600, 60), 38, Color.yellow, TextAlignmentOptions.Center);
        CreateText(canvas.transform, "BGM Volume: 100%", new Vector2(0, 80), new Vector2(500, 50), 30, Color.white, TextAlignmentOptions.Center);
        CreateText(canvas.transform, "SFX Volume: 100%", new Vector2(0, 10), new Vector2(500, 50), 30, Color.white, TextAlignmentOptions.Center);

        CreateButton(canvas.transform, "Back to Main Menu", new Vector2(0, -140), new Vector2(340, 70), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Options.unity");
    }

    private static void SetupCredits()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (_, _, canvas, ctrl) = CreateSceneBase("Credits");

        string info = "Department of Digital Game\nCollege of Arts, Media and Technology\nChiang Mai University\n\nCourse 958321 Game Development 3\nStudent Name: Pasit\nGitHub: github.com/pasit789/GameDev_Exam";
        CreateText(canvas.transform, info, new Vector2(0, 70), new Vector2(900, 300), 28, Color.white, TextAlignmentOptions.Center);

        CreateButton(canvas.transform, "Back to Main Menu", new Vector2(0, -180), new Vector2(340, 70), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Credits.unity");
    }

    private static void SetupWin()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase("WIN!");
        cam.GetComponent<Camera>().backgroundColor = new Color(0.1f, 0.25f, 0.12f);

        var scoreTxt = CreateText(canvas.transform, "Score : 10000", new Vector2(0, 120), new Vector2(600, 80), 48, Color.green, TextAlignmentOptions.Center);
        scoreTxt.gameObject.AddComponent<ScoreDisplay>();

        CreateButton(canvas.transform, "Play Again (Stage 1)", new Vector2(0, -20), new Vector2(380, 75), ctrl, "Stage1");
        CreateButton(canvas.transform, "Stage Selection", new Vector2(0, -110), new Vector2(380, 75), ctrl, "StageSelection");
        CreateButton(canvas.transform, "Main Menu", new Vector2(0, -200), new Vector2(380, 75), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Win.unity");
    }

    private static void SetupGameOver()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase("GAME OVER");
        cam.GetComponent<Camera>().backgroundColor = new Color(0.28f, 0.1f, 0.1f);

        var scoreTxt = CreateText(canvas.transform, "Score : 0", new Vector2(0, 120), new Vector2(600, 80), 48, Color.red, TextAlignmentOptions.Center);
        scoreTxt.gameObject.AddComponent<ScoreDisplay>();

        CreateButton(canvas.transform, "Try Again (Stage 1)", new Vector2(0, -20), new Vector2(380, 75), ctrl, "Stage1");
        CreateButton(canvas.transform, "Stage Selection", new Vector2(0, -110), new Vector2(380, 75), ctrl, "StageSelection");
        CreateButton(canvas.transform, "Main Menu", new Vector2(0, -200), new Vector2(380, 75), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameOver.unity");
    }

    private static void SetupStage2()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (_, _, canvas, ctrl) = CreateSceneBase("Stage 2 - Humanoid Player Minigame");

        CreateText(canvas.transform, "Stage 2 Level Setup Placeholder\n(Ready for Starter Assets & Interaction System)", new Vector2(0, 80), new Vector2(800, 120), 32, Color.cyan, TextAlignmentOptions.Center);
        CreateButton(canvas.transform, "Back to Stage Selection", new Vector2(0, -120), new Vector2(400, 75), ctrl, "StageSelection");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Stage2.unity");
    }

    private static void SetupStage1()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Directional Light
        GameObject light = new GameObject("Directional Light");
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Level Design: Wide and Narrow platforms with falling gaps (Q.3)
        Material matPlatform = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Platform.mat");
        Material matBridge = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Bridge.mat");

        GameObject levelRoot = new GameObject("Level");

        // Platform 1 (Start - 10x10)
        CreatePlatform(levelRoot.transform, "Platform_Start", new Vector3(0, 0, 0), new Vector3(10, 1, 10), matPlatform);

        // Narrow Bridge 1 (Width 2.2, Length 16)
        CreatePlatform(levelRoot.transform, "Narrow_Bridge_1", new Vector3(0, 0, 13), new Vector3(2.2f, 0.8f, 16), matBridge);

        // Platform 2 (Middle - 8x8)
        CreatePlatform(levelRoot.transform, "Platform_Middle", new Vector3(0, 0, 25), new Vector3(8, 1, 8), matPlatform);

        // Narrow Bridge 2 (Width 1.6, Length 14)
        CreatePlatform(levelRoot.transform, "Narrow_Bridge_2", new Vector3(0, 0, 36), new Vector3(1.6f, 0.8f, 14), matBridge);

        // Platform 3 (End - 12x12)
        CreatePlatform(levelRoot.transform, "Platform_End", new Vector3(0, 0, 49), new Vector3(12, 1, 12), matPlatform);

        // Capsule Player (Q.2)
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "CapsulePlayer";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1.5f, 0);
        player.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Player.mat");

        // Character Controller & Script
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 0, 0);
        cc.radius = 0.5f;
        cc.height = 2f;
        player.AddComponent<CapsulePlayer>();

        // Camera following player
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>();
        cam.AddComponent<AudioListener>();
        cam.transform.SetParent(player.transform);
        cam.transform.localPosition = new Vector3(0, 2.8f, -4.5f);
        cam.transform.localRotation = Quaternion.Euler(18, 0, 0);

        // Spawn Items across the map (Q.4 & Q.5)
        GameObject itemsRoot = new GameObject("Items");
        GameObject pAddScore = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_AddScore.prefab");
        GameObject pAddTime = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_AddTime.prefab");
        GameObject pReduceTime = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_ReduceTime.prefab");
        GameObject pReduceScore = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_ReduceScore.prefab");

        // Total 25 AddScore items (25 * 500 = 12500 score -> enough to exceed 10000 winning condition!)
        // Platform 1 (6 items)
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                SpawnItem(pAddScore, itemsRoot.transform, new Vector3(i * 3f, 1.2f, j * 3f));
            }
        }

        // Bridge 1 (5 items)
        for (int k = 0; k < 5; k++)
        {
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 1.2f, 6f + k * 2.8f));
        }

        // Middle Platform (4 items)
        SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-2.5f, 1.2f, 23.5f));
        SpawnItem(pAddScore, itemsRoot.transform, new Vector3(2.5f, 1.2f, 23.5f));
        SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-2.5f, 1.2f, 26.5f));
        SpawnItem(pAddScore, itemsRoot.transform, new Vector3(2.5f, 1.2f, 26.5f));

        // Bridge 2 (4 items)
        for (int k = 0; k < 4; k++)
        {
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 1.2f, 31f + k * 3f));
        }

        // End Platform (6 items)
        for (int i = -1; i <= 1; i++)
        {
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(i * 3.5f, 1.2f, 47f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(i * 3.5f, 1.2f, 52f));
        }

        // AddTime Items (+3s)
        SpawnItem(pAddTime, itemsRoot.transform, new Vector3(-3f, 1.2f, 2f));
        SpawnItem(pAddTime, itemsRoot.transform, new Vector3(0, 1.2f, 18f));
        SpawnItem(pAddTime, itemsRoot.transform, new Vector3(0, 1.2f, 25f));
        SpawnItem(pAddTime, itemsRoot.transform, new Vector3(0, 1.2f, 42f));
        SpawnItem(pAddTime, itemsRoot.transform, new Vector3(0, 1.2f, 50f));

        // ReduceTime Items (-5s obstacles)
        SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(-0.6f, 1.2f, 12f));
        SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(0.6f, 1.2f, 35f));

        // ReduceScore Items (-500 obstacles)
        SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(0.6f, 1.2f, 15f));
        SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(-0.5f, 1.2f, 38f));

        // EventSystem & HUD Canvas
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject ctrlGo = new GameObject("SceneController");
        SceneController sceneCtrl = ctrlGo.AddComponent<SceneController>();

        // HUD: Label, Time, Score
        CreateText(canvas.transform, "Stage 1 - Capsule Player Minigame", new Vector2(0, 480), new Vector2(800, 60), 32, Color.white, TextAlignmentOptions.Center);

        // Top Left Time & Score
        GameObject hudPanel = new GameObject("HUD_Panel");
        hudPanel.transform.SetParent(canvas.transform, false);
        RectTransform rtHud = hudPanel.AddComponent<RectTransform>();
        rtHud.anchorMin = new Vector2(0, 1);
        rtHud.anchorMax = new Vector2(0, 1);
        rtHud.pivot = new Vector2(0, 1);
        rtHud.anchoredPosition = new Vector2(40, -40);
        rtHud.sizeDelta = new Vector2(400, 160);

        Image bgHud = hudPanel.AddComponent<Image>();
        bgHud.color = new Color(0, 0, 0, 0.6f);

        var timeTxt = CreateText(hudPanel.transform, "Time: 30", new Vector2(20, -20), new Vector2(360, 50), 36, Color.yellow, TextAlignmentOptions.Left);
        RectTransform rtTime = timeTxt.GetComponent<RectTransform>();
        rtTime.anchorMin = new Vector2(0, 1);
        rtTime.anchorMax = new Vector2(0, 1);
        rtTime.pivot = new Vector2(0, 1);
        rtTime.anchoredPosition = new Vector2(20, -15);

        var scoreTxt = CreateText(hudPanel.transform, "Score: 0", new Vector2(20, -75), new Vector2(360, 50), 36, Color.white, TextAlignmentOptions.Left);
        RectTransform rtScore = scoreTxt.GetComponent<RectTransform>();
        rtScore.anchorMin = new Vector2(0, 1);
        rtScore.anchorMax = new Vector2(0, 1);
        rtScore.pivot = new Vector2(0, 1);
        rtScore.anchoredPosition = new Vector2(20, -70);

        // Menu Button
        CreateButton(canvas.transform, "Main Menu", new Vector2(800, 470), new Vector2(220, 60), sceneCtrl, "MainMenu");

        // GameManager setup
        GameObject gmGo = new GameObject("GameManager");
        GameManager gm = gmGo.AddComponent<GameManager>();
        gm.timeText = timeTxt;
        gm.scoreText = scoreTxt;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Stage1.unity");
    }

    private static void CreatePlatform(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = pos;
        cube.transform.localScale = size;
        if (mat != null) cube.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void SpawnItem(GameObject prefab, Transform parent, Vector3 pos)
    {
        if (prefab == null) return;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.transform.position = pos;
    }
}
