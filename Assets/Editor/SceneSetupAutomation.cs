using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using Pasit;

public class SceneSetupAutomation
{
    [MenuItem("Tools/Pasit/Setup Options Scene (Modern UI)")]
    public static void SetupOptionsOnly()
    {
        EnsureFolders();
        Pasit.Editor.UISpriteGenerator.GenerateSprites();
        SetupOptions();
        EditorSceneManager.OpenScene("Assets/Scenes/Options.unity");
        Debug.Log("==== OPTIONS SCENE SUCCESSFULLY UPGRADED TO MODERN UI! ====");
    }

    [MenuItem("Tools/Pasit/Setup All Scenes and Assets (Q1-Q5)")]
    public static void SetupAll()
    {
        EnsureFolders();
        Pasit.Editor.UISpriteGenerator.GenerateSprites();
        AudioGenerator.GenerateDefaultAudioFiles();
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
        if (!AssetDatabase.IsValidFolder("Assets/Audio")) AssetDatabase.CreateFolder("Assets", "Audio");
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
        eventSystem.AddComponent<InputSystemUIInputModule>();

        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject ctrlGo = new GameObject("SceneController");
        SceneController sceneCtrl = ctrlGo.AddComponent<SceneController>();

        if (!string.IsNullOrEmpty(title))
        {
            var txt = CreateText(canvas.transform, title, new Vector2(0, 380), new Vector2(1000, 100), 50, Color.white, TextAlignmentOptions.Center);
            txt.fontStyle = FontStyles.Bold;
        }

        return (cam, light, canvas, sceneCtrl);
    }

    private static TextMeshProUGUI CreateText(Transform parent, string text, Vector2 anchoredPos, Vector2 size, float fontSize, Color color, TextAlignmentOptions align, bool raycast = true)
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
        tmp.raycastTarget = raycast;
        return tmp;
    }

    private static Sprite GetUISprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/" + name + ".png");
    }

    private static Button CreateButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size, SceneController sceneCtrl, string targetScene, bool isExit = false, string spriteName = "UI_Button")
    {
        GameObject btnGo = new GameObject("Btn_" + label);
        btnGo.transform.SetParent(parent, false);
        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = btnGo.AddComponent<Image>();
        Sprite btnSprite = GetUISprite(spriteName) ?? GetUISprite("UI_Button");
        if (btnSprite != null)
        {
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.color = new Color(0.2f, 0.45f, 0.8f);
        }

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.75f, 0.90f, 1.0f, 1f);
        colors.pressedColor = new Color(0.55f, 0.75f, 0.95f, 1f);
        colors.selectedColor = Color.white;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.15f;
        btn.colors = colors;

        var txt = CreateText(btnGo.transform, label, Vector2.zero, size, 24, Color.white, TextAlignmentOptions.Center, false);
        txt.fontStyle = FontStyles.Bold;

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

    private static GameObject CreateModernSlider(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject sliderGo = DefaultControls.CreateSlider(res);
        sliderGo.name = name;
        sliderGo.transform.SetParent(parent, false);
        RectTransform rt = sliderGo.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Sprite trackSprite = GetUISprite("UI_SliderTrack");
        Sprite fillSprite = GetUISprite("UI_SliderFill");
        Sprite knobSprite = GetUISprite("UI_SliderKnob");

        Transform bg = sliderGo.transform.Find("Background");
        if (bg != null && trackSprite != null)
        {
            Image bgImg = bg.GetComponent<Image>();
            bgImg.sprite = trackSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = Color.white;
        }

        Transform fill = sliderGo.transform.Find("Fill Area/Fill");
        if (fill != null && fillSprite != null)
        {
            Image fillImg = fill.GetComponent<Image>();
            fillImg.sprite = fillSprite;
            fillImg.type = Image.Type.Sliced;
            fillImg.color = Color.white;
        }

        Transform handleArea = sliderGo.transform.Find("Handle Slide Area");
        if (handleArea != null)
        {
            RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
            handleAreaRt.offsetMin = new Vector2(16, 0);
            handleAreaRt.offsetMax = new Vector2(-16, 0);
        }

        Transform handle = sliderGo.transform.Find("Handle Slide Area/Handle");
        if (handle != null && knobSprite != null)
        {
            Image handleImg = handle.GetComponent<Image>();
            handleImg.sprite = knobSprite;
            handleImg.type = Image.Type.Simple;
            handleImg.color = Color.white;
            RectTransform handleRt = handle.GetComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0.5f, 0.5f);
            handleRt.anchorMax = new Vector2(0.5f, 0.5f);
            handleRt.sizeDelta = new Vector2(28, 28);
        }

        Slider sliderComp = sliderGo.GetComponent<Slider>();
        if (sliderComp != null)
        {
            sliderComp.minValue = 0f;
            sliderComp.maxValue = 1f;
            sliderComp.value = 0.8f;
        }

        return sliderGo;
    }

    private static (GameObject sliderGo, TextMeshProUGUI badgeText) CreateVolumeRow(Transform parent, string title, string icon, string sliderName, Vector2 pos, Vector2 size)
    {
        GameObject rowGo = new GameObject("Row_" + sliderName);
        rowGo.transform.SetParent(parent, false);
        RectTransform rowRt = rowGo.AddComponent<RectTransform>();
        rowRt.anchoredPosition = pos;
        rowRt.sizeDelta = size;

        Image rowImg = rowGo.AddComponent<Image>();
        Sprite rowSprite = GetUISprite("UI_RowCard");
        if (rowSprite != null)
        {
            rowImg.sprite = rowSprite;
            rowImg.type = Image.Type.Sliced;
        }
        rowImg.color = Color.white;

        // Label on left
        string labelText = string.IsNullOrEmpty(icon) ? title : $"{icon}  {title}";
        var labelTmp = CreateText(rowGo.transform, labelText, new Vector2(-size.x / 2f + 25f + 180f, 22f), new Vector2(360, 36), 21, new Color(0.95f, 0.96f, 0.98f), TextAlignmentOptions.Left, false);
        labelTmp.fontStyle = FontStyles.Bold;

        // Percentage Badge on right
        GameObject badgeGo = new GameObject("Badge_" + sliderName);
        badgeGo.transform.SetParent(rowGo.transform, false);
        RectTransform badgeRt = badgeGo.AddComponent<RectTransform>();
        badgeRt.anchoredPosition = new Vector2(size.x / 2f - 65f, 22f);
        badgeRt.sizeDelta = new Vector2(85, 34);

        Image badgeImg = badgeGo.AddComponent<Image>();
        Sprite badgeSprite = GetUISprite("UI_Badge");
        if (badgeSprite != null)
        {
            badgeImg.sprite = badgeSprite;
            badgeImg.type = Image.Type.Sliced;
        }
        badgeImg.color = Color.white;

        TextMeshProUGUI badgeTmp = CreateText(badgeGo.transform, "100%", Vector2.zero, new Vector2(85, 34), 19, new Color(0.2f, 0.85f, 1f), TextAlignmentOptions.Center, false);
        badgeTmp.fontStyle = FontStyles.Bold;

        // Slider across bottom
        GameObject sliderGo = CreateModernSlider(rowGo.transform, sliderName, new Vector2(0, -20f), new Vector2(size.x - 50f, 20f));

        return (sliderGo, badgeTmp);
    }

    private static void SetupMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase("");

        cam.GetComponent<Camera>().backgroundColor = new Color(0.06f, 0.08f, 0.11f);

        // Backdrop
        GameObject backdropGo = new GameObject("UI_Backdrop");
        backdropGo.transform.SetParent(canvas.transform, false);
        RectTransform bdRt = backdropGo.AddComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero;
        bdRt.anchorMax = Vector2.one;
        bdRt.sizeDelta = Vector2.zero;
        Image bdImg = backdropGo.AddComponent<Image>();
        bdImg.color = new Color(0.04f, 0.06f, 0.09f, 0.65f);
        bdImg.raycastTarget = false;

        // AudioManager setup with exam audio clips
        GameObject audioGo = new GameObject("AudioManager");
        AudioManager audioMgr = audioGo.AddComponent<AudioManager>();
        audioMgr.bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/BGM/Tiny_Blocks.wav")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/BGM_Default.wav");
        audioMgr.itemSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/SFX/Coin Pickup 4.wav")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/SFX_Item.wav");
        audioMgr.obstacleSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/SFX/Debuff Downgrade 1.wav")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/SFX_Obstacle.wav");

        // Central Glass Card Panel (820 x 860)
        GameObject cardGo = new GameObject("Panel_MenuCard");
        cardGo.transform.SetParent(canvas.transform, false);
        RectTransform cardRt = cardGo.AddComponent<RectTransform>();
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(820, 860);

        Image cardImg = cardGo.AddComponent<Image>();
        Sprite glassSprite = GetUISprite("UI_GlassCard");
        if (glassSprite != null)
        {
            cardImg.sprite = glassSprite;
            cardImg.type = Image.Type.Sliced;
        }
        cardImg.color = Color.white;

        // Header Tag: [ CAMT • 958321 ]
        var tagTmp = CreateText(cardGo.transform, "CAMT  *  DIGITAL GAME", new Vector2(0, 360), new Vector2(500, 30), 16, new Color(0.22f, 0.74f, 0.97f), TextAlignmentOptions.Center, false);
        tagTmp.fontStyle = FontStyles.Bold;
        tagTmp.characterSpacing = 8f;

        // Title
        var titleTmp = CreateText(cardGo.transform, "958321 Game Development 3\nPractical Exam", new Vector2(0, 290), new Vector2(760, 90), 38, Color.white, TextAlignmentOptions.Center, false);
        titleTmp.fontStyle = FontStyles.Bold;

        // Student Info Pill Badge
        GameObject badgeGo = new GameObject("Badge_StudentInfo");
        badgeGo.transform.SetParent(cardGo.transform, false);
        RectTransform badgeRt = badgeGo.AddComponent<RectTransform>();
        badgeRt.anchoredPosition = new Vector2(0, 205);
        badgeRt.sizeDelta = new Vector2(620, 44);
        Image badgeImg = badgeGo.AddComponent<Image>();
        badgeImg.sprite = GetUISprite("UI_Badge");
        badgeImg.type = Image.Type.Sliced;
        badgeImg.color = Color.white;

        var studentTmp = CreateText(badgeGo.transform, "Student Code: 672110109 | Name: Pasit", Vector2.zero, new Vector2(600, 40), 20, new Color(0.85f, 0.90f, 0.95f), TextAlignmentOptions.Center, false);
        studentTmp.fontStyle = FontStyles.Bold;

        // Divider Line
        GameObject divTop = new GameObject("Divider_Top");
        divTop.transform.SetParent(cardGo.transform, false);
        RectTransform divTopRt = divTop.AddComponent<RectTransform>();
        divTopRt.anchoredPosition = new Vector2(0, 160);
        divTopRt.sizeDelta = new Vector2(720, 3);
        Image divTopImg = divTop.AddComponent<Image>();
        divTopImg.sprite = GetUISprite("UI_Divider");
        divTopImg.color = Color.white;
        divTopImg.raycastTarget = false;

        // Menu Buttons
        CreateButton(cardGo.transform, "START GAME", new Vector2(0, 85), new Vector2(460, 72), ctrl, "StageSelection");
        CreateButton(cardGo.transform, "OPTIONS", new Vector2(0, -5), new Vector2(460, 72), ctrl, "Options");
        CreateButton(cardGo.transform, "CREDITS", new Vector2(0, -95), new Vector2(460, 72), ctrl, "Credits");
        CreateButton(cardGo.transform, "EXIT GAME", new Vector2(0, -185), new Vector2(460, 72), ctrl, "", true, "UI_Button_Red");

        // Bottom Divider Line
        GameObject divBottom = new GameObject("Divider_Bottom");
        divBottom.transform.SetParent(cardGo.transform, false);
        RectTransform divBottomRt = divBottom.AddComponent<RectTransform>();
        divBottomRt.anchoredPosition = new Vector2(0, -255);
        divBottomRt.sizeDelta = new Vector2(720, 3);
        Image divBottomImg = divBottom.AddComponent<Image>();
        divBottomImg.sprite = GetUISprite("UI_Divider");
        divBottomImg.color = Color.white;
        divBottomImg.raycastTarget = false;

        var footerTmp = CreateText(cardGo.transform, "CHIANG MAI UNIVERSITY  *  2026", new Vector2(0, -295), new Vector2(500, 30), 14, new Color(0.40f, 0.50f, 0.65f), TextAlignmentOptions.Center, false);
        footerTmp.characterSpacing = 4f;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    private static void SetupStageSelection()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase("");

        cam.GetComponent<Camera>().backgroundColor = new Color(0.06f, 0.08f, 0.11f);

        // Backdrop
        GameObject backdropGo = new GameObject("UI_Backdrop");
        backdropGo.transform.SetParent(canvas.transform, false);
        RectTransform bdRt = backdropGo.AddComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero;
        bdRt.anchorMax = Vector2.one;
        bdRt.sizeDelta = Vector2.zero;
        Image bdImg = backdropGo.AddComponent<Image>();
        bdImg.color = new Color(0.04f, 0.06f, 0.09f, 0.65f);
        bdImg.raycastTarget = false;

        // Card Panel (820 x 860)
        GameObject cardGo = new GameObject("Panel_StageCard");
        cardGo.transform.SetParent(canvas.transform, false);
        RectTransform cardRt = cardGo.AddComponent<RectTransform>();
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(820, 860);

        Image cardImg = cardGo.AddComponent<Image>();
        Sprite glassSprite = GetUISprite("UI_GlassCard");
        if (glassSprite != null)
        {
            cardImg.sprite = glassSprite;
            cardImg.type = Image.Type.Sliced;
        }
        cardImg.color = Color.white;

        // Tag: [ LEVEL SELECTION ]
        var tagTmp = CreateText(cardGo.transform, "MISSION SELECT", new Vector2(0, 360), new Vector2(500, 30), 16, new Color(0.22f, 0.74f, 0.97f), TextAlignmentOptions.Center, false);
        tagTmp.fontStyle = FontStyles.Bold;
        tagTmp.characterSpacing = 8f;

        // Title
        var titleTmp = CreateText(cardGo.transform, "STAGE SELECTION", new Vector2(0, 312), new Vector2(600, 60), 40, Color.white, TextAlignmentOptions.Center, false);
        titleTmp.fontStyle = FontStyles.Bold;

        // Divider
        GameObject divTop = new GameObject("Divider_Top");
        divTop.transform.SetParent(cardGo.transform, false);
        RectTransform divTopRt = divTop.AddComponent<RectTransform>();
        divTopRt.anchoredPosition = new Vector2(0, 265);
        divTopRt.sizeDelta = new Vector2(720, 3);
        Image divTopImg = divTop.AddComponent<Image>();
        divTopImg.sprite = GetUISprite("UI_Divider");
        divTopImg.color = Color.white;
        divTopImg.raycastTarget = false;

        // Stage 1 Container Row Card
        GameObject row1 = new GameObject("Row_Stage1");
        row1.transform.SetParent(cardGo.transform, false);
        RectTransform row1Rt = row1.AddComponent<RectTransform>();
        row1Rt.anchoredPosition = new Vector2(0, 140);
        row1Rt.sizeDelta = new Vector2(720, 150);
        Image row1Img = row1.AddComponent<Image>();
        row1Img.sprite = GetUISprite("UI_RowCard");
        row1Img.type = Image.Type.Sliced;
        row1Img.color = Color.white;

        var s1Title = CreateText(row1.transform, "STAGE 1 : CAPSULE RUNNER", new Vector2(0, 38), new Vector2(660, 40), 24, Color.white, TextAlignmentOptions.Center, false);
        s1Title.fontStyle = FontStyles.Bold;
        CreateText(row1.transform, "Course Objective: Collect Coins, avoid debuffs, reach 10,000 points!", new Vector2(0, 6), new Vector2(660, 30), 16, new Color(0.65f, 0.75f, 0.90f), TextAlignmentOptions.Center, false);
        CreateButton(row1.transform, "PLAY STAGE 1", new Vector2(0, -36), new Vector2(360, 48), ctrl, "Stage1", false, "UI_Button_Green");

        // Stage 2 Container Row Card
        GameObject row2 = new GameObject("Row_Stage2");
        row2.transform.SetParent(cardGo.transform, false);
        RectTransform row2Rt = row2.AddComponent<RectTransform>();
        row2Rt.anchoredPosition = new Vector2(0, -30);
        row2Rt.sizeDelta = new Vector2(720, 150);
        Image row2Img = row2.AddComponent<Image>();
        row2Img.sprite = GetUISprite("UI_RowCard");
        row2Img.type = Image.Type.Sliced;
        row2Img.color = Color.white;

        var s2Title = CreateText(row2.transform, "STAGE 2 : HUMANOID EXPLORER", new Vector2(0, 38), new Vector2(660, 40), 24, Color.white, TextAlignmentOptions.Center, false);
        s2Title.fontStyle = FontStyles.Bold;
        CreateText(row2.transform, "Course Objective: Humanoid Character Controller & Item Interactions.", new Vector2(0, 6), new Vector2(660, 30), 16, new Color(0.65f, 0.75f, 0.90f), TextAlignmentOptions.Center, false);
        CreateButton(row2.transform, "PLAY STAGE 2", new Vector2(0, -36), new Vector2(360, 48), ctrl, "Stage2");

        // Divider Bottom
        GameObject divBottom = new GameObject("Divider_Bottom");
        divBottom.transform.SetParent(cardGo.transform, false);
        RectTransform divBottomRt = divBottom.AddComponent<RectTransform>();
        divBottomRt.anchoredPosition = new Vector2(0, -170);
        divBottomRt.sizeDelta = new Vector2(720, 3);
        Image divBottomImg = divBottom.AddComponent<Image>();
        divBottomImg.sprite = GetUISprite("UI_Divider");
        divBottomImg.color = Color.white;
        divBottomImg.raycastTarget = false;

        // Back Button
        CreateButton(cardGo.transform, "BACK TO MAIN MENU", new Vector2(0, -250), new Vector2(380, 68), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/StageSelection.unity");
    }

    private static void SetupOptions()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase(""); // Empty title so base doesn't create raw plain text

        // Sleek modern dark obsidian background
        cam.GetComponent<Camera>().backgroundColor = new Color(0.06f, 0.08f, 0.11f);

        // Audio Manager in Options scene (loads exam sounds from PracticalExamResources)
        GameObject audioGo = new GameObject("AudioManager");
        AudioManager audioMgr = audioGo.AddComponent<AudioManager>();
        audioMgr.bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/BGM/Tiny_Blocks.wav") 
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/BGM_Default.wav");
        audioMgr.itemSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/SFX/Coin Pickup 4.wav") 
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/SFX_Item.wav");
        audioMgr.obstacleSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/SFX/Debuff Downgrade 1.wav") 
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/SFX_Obstacle.wav");

        // Backdrop subtle dark glow overlay
        GameObject backdropGo = new GameObject("UI_Backdrop");
        backdropGo.transform.SetParent(canvas.transform, false);
        RectTransform bdRt = backdropGo.AddComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero;
        bdRt.anchorMax = Vector2.one;
        bdRt.sizeDelta = Vector2.zero;
        Image bdImg = backdropGo.AddComponent<Image>();
        bdImg.color = new Color(0.04f, 0.06f, 0.09f, 0.65f);
        bdImg.raycastTarget = false;

        // Central Glass Card Panel (820 x 860)
        GameObject cardGo = new GameObject("Panel_OptionsCard");
        cardGo.transform.SetParent(canvas.transform, false);
        RectTransform cardRt = cardGo.AddComponent<RectTransform>();
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(820, 860);

        Image cardImg = cardGo.AddComponent<Image>();
        Sprite glassSprite = GetUISprite("UI_GlassCard");
        if (glassSprite != null)
        {
            cardImg.sprite = glassSprite;
            cardImg.type = Image.Type.Sliced;
        }
        cardImg.color = Color.white;

        // Header Tag: [ SYSTEM PREFERENCES ]
        var tagTmp = CreateText(cardGo.transform, "SYSTEM PREFERENCES", new Vector2(0, 360), new Vector2(500, 30), 16, new Color(0.22f, 0.74f, 0.97f), TextAlignmentOptions.Center, false);
        tagTmp.fontStyle = FontStyles.Bold;
        tagTmp.characterSpacing = 8f;

        // Header Title: AUDIO SETTINGS
        var titleTmp = CreateText(cardGo.transform, "AUDIO SETTINGS", new Vector2(0, 312), new Vector2(600, 60), 40, Color.white, TextAlignmentOptions.Center, false);
        titleTmp.fontStyle = FontStyles.Bold;

        // Top Divider Line
        GameObject divTop = new GameObject("Divider_Top");
        divTop.transform.SetParent(cardGo.transform, false);
        RectTransform divTopRt = divTop.AddComponent<RectTransform>();
        divTopRt.anchoredPosition = new Vector2(0, 265);
        divTopRt.sizeDelta = new Vector2(720, 3);
        Image divTopImg = divTop.AddComponent<Image>();
        divTopImg.sprite = GetUISprite("UI_Divider");
        divTopImg.color = Color.white;
        divTopImg.raycastTarget = false;

        // Row 1: Master Volume
        var (masterSliderGo, masterBadge) = CreateVolumeRow(cardGo.transform, "MASTER VOLUME", "", "Slider_Master", new Vector2(0, 165), new Vector2(720, 115));

        // Row 2: BGM Volume
        var (bgmSliderGo, bgmBadge) = CreateVolumeRow(cardGo.transform, "BACKGROUND MUSIC (BGM)", "", "Slider_BGM", new Vector2(0, 35), new Vector2(720, 115));

        // Row 3: SFX Volume
        var (sfxSliderGo, sfxBadge) = CreateVolumeRow(cardGo.transform, "SOUND EFFECTS (SFX)", "", "Slider_SFX", new Vector2(0, -95), new Vector2(720, 115));

        // Bottom Divider Line
        GameObject divBottom = new GameObject("Divider_Bottom");
        divBottom.transform.SetParent(cardGo.transform, false);
        RectTransform divBottomRt = divBottom.AddComponent<RectTransform>();
        divBottomRt.anchoredPosition = new Vector2(0, -195);
        divBottomRt.sizeDelta = new Vector2(720, 3);
        Image divBottomImg = divBottom.AddComponent<Image>();
        divBottomImg.sprite = GetUISprite("UI_Divider");
        divBottomImg.color = Color.white;
        divBottomImg.raycastTarget = false;

        // Back Button
        CreateButton(cardGo.transform, "BACK TO MAIN MENU", new Vector2(0, -280), new Vector2(380, 68), ctrl, "MainMenu");

        // Controller setup
        OptionsController optCtrl = canvas.gameObject.AddComponent<OptionsController>();
        Slider mSlider = masterSliderGo.GetComponent<Slider>();
        Slider bSlider = bgmSliderGo.GetComponent<Slider>();
        Slider sSlider = sfxSliderGo.GetComponent<Slider>();

        optCtrl.masterSlider = mSlider;
        optCtrl.bgmSlider = bSlider;
        optCtrl.sfxSlider = sSlider;

        mSlider.value = 1.0f;
        bSlider.value = 0.8f;
        sSlider.value = 0.8f;

        masterBadge.text = "100%";
        bgmBadge.text = "80%";
        sfxBadge.text = "80%";

        optCtrl.masterValueText = masterBadge;
        optCtrl.bgmValueText = bgmBadge;
        optCtrl.sfxValueText = sfxBadge;

        UnityAction<float> masterAction = new UnityAction<float>(optCtrl.OnMasterVolumeChanged);
        UnityEventTools.AddPersistentListener(mSlider.onValueChanged, masterAction);

        UnityAction<float> bgmAction = new UnityAction<float>(optCtrl.OnBGMVolumeChanged);
        UnityEventTools.AddPersistentListener(bSlider.onValueChanged, bgmAction);

        UnityAction<float> sfxAction = new UnityAction<float>(optCtrl.OnSFXVolumeChanged);
        UnityEventTools.AddPersistentListener(sSlider.onValueChanged, sfxAction);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Options.unity");
    }

    private static void SetupCredits()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase("");

        cam.GetComponent<Camera>().backgroundColor = new Color(0.06f, 0.08f, 0.11f);

        // Backdrop
        GameObject backdropGo = new GameObject("UI_Backdrop");
        backdropGo.transform.SetParent(canvas.transform, false);
        RectTransform bdRt = backdropGo.AddComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero;
        bdRt.anchorMax = Vector2.one;
        bdRt.sizeDelta = Vector2.zero;
        Image bdImg = backdropGo.AddComponent<Image>();
        bdImg.color = new Color(0.04f, 0.06f, 0.09f, 0.65f);
        bdImg.raycastTarget = false;

        // Card Panel (820 x 860)
        GameObject cardGo = new GameObject("Panel_CreditsCard");
        cardGo.transform.SetParent(canvas.transform, false);
        RectTransform cardRt = cardGo.AddComponent<RectTransform>();
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(820, 860);

        Image cardImg = cardGo.AddComponent<Image>();
        Sprite glassSprite = GetUISprite("UI_GlassCard");
        if (glassSprite != null)
        {
            cardImg.sprite = glassSprite;
            cardImg.type = Image.Type.Sliced;
        }
        cardImg.color = Color.white;

        // Tag: [ PROJECT INFORMATION ]
        var tagTmp = CreateText(cardGo.transform, "PROJECT INFORMATION", new Vector2(0, 360), new Vector2(500, 30), 16, new Color(0.22f, 0.74f, 0.97f), TextAlignmentOptions.Center, false);
        tagTmp.fontStyle = FontStyles.Bold;
        tagTmp.characterSpacing = 8f;

        // Title
        var titleTmp = CreateText(cardGo.transform, "CREDITS", new Vector2(0, 312), new Vector2(600, 60), 40, Color.white, TextAlignmentOptions.Center, false);
        titleTmp.fontStyle = FontStyles.Bold;

        // Divider Top
        GameObject divTop = new GameObject("Divider_Top");
        divTop.transform.SetParent(cardGo.transform, false);
        RectTransform divTopRt = divTop.AddComponent<RectTransform>();
        divTopRt.anchoredPosition = new Vector2(0, 265);
        divTopRt.sizeDelta = new Vector2(720, 3);
        Image divTopImg = divTop.AddComponent<Image>();
        divTopImg.sprite = GetUISprite("UI_Divider");
        divTopImg.color = Color.white;
        divTopImg.raycastTarget = false;

        // Info Row 1: Institution
        GameObject row1 = new GameObject("Row_Institution");
        row1.transform.SetParent(cardGo.transform, false);
        RectTransform row1Rt = row1.AddComponent<RectTransform>();
        row1Rt.anchoredPosition = new Vector2(0, 160);
        row1Rt.sizeDelta = new Vector2(720, 110);
        Image row1Img = row1.AddComponent<Image>();
        row1Img.sprite = GetUISprite("UI_RowCard");
        row1Img.type = Image.Type.Sliced;
        row1Img.color = Color.white;

        var instTitle = CreateText(row1.transform, "DEPARTMENT OF DIGITAL GAME", new Vector2(0, 18), new Vector2(660, 34), 22, Color.white, TextAlignmentOptions.Center, false);
        instTitle.fontStyle = FontStyles.Bold;
        CreateText(row1.transform, "College of Arts, Media and Technology\nChiang Mai University", new Vector2(0, -20), new Vector2(660, 48), 16, new Color(0.65f, 0.75f, 0.90f), TextAlignmentOptions.Center, false);

        // Info Row 2: Developer
        GameObject row2 = new GameObject("Row_Developer");
        row2.transform.SetParent(cardGo.transform, false);
        RectTransform row2Rt = row2.AddComponent<RectTransform>();
        row2Rt.anchoredPosition = new Vector2(0, 25);
        row2Rt.sizeDelta = new Vector2(720, 110);
        Image row2Img = row2.AddComponent<Image>();
        row2Img.sprite = GetUISprite("UI_RowCard");
        row2Img.type = Image.Type.Sliced;
        row2Img.color = Color.white;

        var devTitle = CreateText(row2.transform, "COURSE 958321 GAME DEVELOPMENT 3", new Vector2(0, 18), new Vector2(660, 34), 22, Color.white, TextAlignmentOptions.Center, false);
        devTitle.fontStyle = FontStyles.Bold;
        var devName = CreateText(row2.transform, "Student Name: Pasit  |  ID: 672110109", new Vector2(0, -18), new Vector2(660, 34), 20, new Color(0.2f, 0.85f, 1f), TextAlignmentOptions.Center, false);
        devName.fontStyle = FontStyles.Bold;

        // Info Row 3: Repository
        GameObject row3 = new GameObject("Row_Repo");
        row3.transform.SetParent(cardGo.transform, false);
        RectTransform row3Rt = row3.AddComponent<RectTransform>();
        row3Rt.anchoredPosition = new Vector2(0, -100);
        row3Rt.sizeDelta = new Vector2(720, 95);
        Image row3Img = row3.AddComponent<Image>();
        row3Img.sprite = GetUISprite("UI_RowCard");
        row3Img.type = Image.Type.Sliced;
        row3Img.color = Color.white;

        var repoTitle = CreateText(row3.transform, "GITHUB REPOSITORY", new Vector2(0, 14), new Vector2(660, 32), 20, Color.white, TextAlignmentOptions.Center, false);
        repoTitle.fontStyle = FontStyles.Bold;
        CreateText(row3.transform, "github.com/pasit789/GameDev_Exam", new Vector2(0, -16), new Vector2(660, 30), 17, new Color(0.38f, 0.74f, 0.98f), TextAlignmentOptions.Center, false);

        // Divider Bottom
        GameObject divBottom = new GameObject("Divider_Bottom");
        divBottom.transform.SetParent(cardGo.transform, false);
        RectTransform divBottomRt = divBottom.AddComponent<RectTransform>();
        divBottomRt.anchoredPosition = new Vector2(0, -185);
        divBottomRt.sizeDelta = new Vector2(720, 3);
        Image divBottomImg = divBottom.AddComponent<Image>();
        divBottomImg.sprite = GetUISprite("UI_Divider");
        divBottomImg.color = Color.white;
        divBottomImg.raycastTarget = false;

        // Back Button
        CreateButton(cardGo.transform, "BACK TO MAIN MENU", new Vector2(0, -260), new Vector2(380, 68), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Credits.unity");
    }

    private static void SetupWin()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase("");

        cam.GetComponent<Camera>().backgroundColor = new Color(0.04f, 0.08f, 0.05f);

        // Backdrop
        GameObject backdropGo = new GameObject("UI_Backdrop");
        backdropGo.transform.SetParent(canvas.transform, false);
        RectTransform bdRt = backdropGo.AddComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero;
        bdRt.anchorMax = Vector2.one;
        bdRt.sizeDelta = Vector2.zero;
        Image bdImg = backdropGo.AddComponent<Image>();
        bdImg.color = new Color(0.02f, 0.06f, 0.03f, 0.65f);
        bdImg.raycastTarget = false;

        // Card Panel (820 x 860) with emerald theme
        GameObject cardGo = new GameObject("Panel_WinCard");
        cardGo.transform.SetParent(canvas.transform, false);
        RectTransform cardRt = cardGo.AddComponent<RectTransform>();
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(820, 860);

        Image cardImg = cardGo.AddComponent<Image>();
        Sprite glassSprite = GetUISprite("UI_GlassCard_Green");
        if (glassSprite != null)
        {
            cardImg.sprite = glassSprite;
            cardImg.type = Image.Type.Sliced;
        }
        cardImg.color = Color.white;

        // Tag: [ MISSION ACCOMPLISHED ]
        var tagTmp = CreateText(cardGo.transform, "MISSION ACCOMPLISHED", new Vector2(0, 360), new Vector2(500, 30), 16, new Color(0.2f, 0.85f, 0.45f), TextAlignmentOptions.Center, false);
        tagTmp.fontStyle = FontStyles.Bold;
        tagTmp.characterSpacing = 8f;

        // Title
        var titleTmp = CreateText(cardGo.transform, "VICTORY!", new Vector2(0, 305), new Vector2(600, 65), 52, new Color(0.34f, 0.95f, 0.60f), TextAlignmentOptions.Center, false);
        titleTmp.fontStyle = FontStyles.Bold;

        // Divider Top
        GameObject divTop = new GameObject("Divider_Top");
        divTop.transform.SetParent(cardGo.transform, false);
        RectTransform divTopRt = divTop.AddComponent<RectTransform>();
        divTopRt.anchoredPosition = new Vector2(0, 255);
        divTopRt.sizeDelta = new Vector2(720, 3);
        Image divTopImg = divTop.AddComponent<Image>();
        divTopImg.sprite = GetUISprite("UI_Divider_Green");
        divTopImg.color = Color.white;
        divTopImg.raycastTarget = false;

        // Score Card Row
        GameObject scoreCard = new GameObject("Card_Score");
        scoreCard.transform.SetParent(cardGo.transform, false);
        RectTransform scoreCardRt = scoreCard.AddComponent<RectTransform>();
        scoreCardRt.anchoredPosition = new Vector2(0, 150);
        scoreCardRt.sizeDelta = new Vector2(680, 130);
        Image scoreCardImg = scoreCard.AddComponent<Image>();
        scoreCardImg.sprite = GetUISprite("UI_RowCard");
        scoreCardImg.type = Image.Type.Sliced;
        scoreCardImg.color = Color.white;

        var resultLabel = CreateText(scoreCard.transform, "FINAL SCORE ACHIEVED", new Vector2(0, 30), new Vector2(600, 30), 18, new Color(0.7f, 0.95f, 0.8f), TextAlignmentOptions.Center, false);
        resultLabel.fontStyle = FontStyles.Bold;

        var scoreTxt = CreateText(scoreCard.transform, "Score : 10000", new Vector2(0, -18), new Vector2(600, 60), 48, new Color(0.2f, 0.95f, 0.5f), TextAlignmentOptions.Center);
        scoreTxt.fontStyle = FontStyles.Bold;
        scoreTxt.gameObject.AddComponent<ScoreDisplay>();

        // Divider Middle
        GameObject divMid = new GameObject("Divider_Mid");
        divMid.transform.SetParent(cardGo.transform, false);
        RectTransform divMidRt = divMid.AddComponent<RectTransform>();
        divMidRt.anchoredPosition = new Vector2(0, 50);
        divMidRt.sizeDelta = new Vector2(720, 3);
        Image divMidImg = divMid.AddComponent<Image>();
        divMidImg.sprite = GetUISprite("UI_Divider_Green");
        divMidImg.color = Color.white;
        divMidImg.raycastTarget = false;

        // Action Buttons
        CreateButton(cardGo.transform, "PLAY AGAIN (STAGE 1)", new Vector2(0, -30), new Vector2(440, 70), ctrl, "Stage1", false, "UI_Button_Green");
        CreateButton(cardGo.transform, "STAGE SELECTION", new Vector2(0, -120), new Vector2(440, 70), ctrl, "StageSelection");
        CreateButton(cardGo.transform, "MAIN MENU", new Vector2(0, -210), new Vector2(440, 70), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Win.unity");
    }

    private static void SetupGameOver()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase("");

        cam.GetComponent<Camera>().backgroundColor = new Color(0.09f, 0.04f, 0.04f);

        // Backdrop
        GameObject backdropGo = new GameObject("UI_Backdrop");
        backdropGo.transform.SetParent(canvas.transform, false);
        RectTransform bdRt = backdropGo.AddComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero;
        bdRt.anchorMax = Vector2.one;
        bdRt.sizeDelta = Vector2.zero;
        Image bdImg = backdropGo.AddComponent<Image>();
        bdImg.color = new Color(0.06f, 0.02f, 0.02f, 0.65f);
        bdImg.raycastTarget = false;

        // Card Panel (820 x 860) with crimson theme
        GameObject cardGo = new GameObject("Panel_GameOverCard");
        cardGo.transform.SetParent(canvas.transform, false);
        RectTransform cardRt = cardGo.AddComponent<RectTransform>();
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(820, 860);

        Image cardImg = cardGo.AddComponent<Image>();
        Sprite glassSprite = GetUISprite("UI_GlassCard_Red");
        if (glassSprite != null)
        {
            cardImg.sprite = glassSprite;
            cardImg.type = Image.Type.Sliced;
        }
        cardImg.color = Color.white;

        // Tag: [ MISSION FAILED ]
        var tagTmp = CreateText(cardGo.transform, "MISSION FAILED", new Vector2(0, 360), new Vector2(500, 30), 16, new Color(0.95f, 0.35f, 0.35f), TextAlignmentOptions.Center, false);
        tagTmp.fontStyle = FontStyles.Bold;
        tagTmp.characterSpacing = 8f;

        // Title
        var titleTmp = CreateText(cardGo.transform, "GAME OVER", new Vector2(0, 305), new Vector2(600, 65), 52, new Color(0.95f, 0.25f, 0.25f), TextAlignmentOptions.Center, false);
        titleTmp.fontStyle = FontStyles.Bold;

        // Divider Top
        GameObject divTop = new GameObject("Divider_Top");
        divTop.transform.SetParent(cardGo.transform, false);
        RectTransform divTopRt = divTop.AddComponent<RectTransform>();
        divTopRt.anchoredPosition = new Vector2(0, 255);
        divTopRt.sizeDelta = new Vector2(720, 3);
        Image divTopImg = divTop.AddComponent<Image>();
        divTopImg.sprite = GetUISprite("UI_Divider_Red");
        divTopImg.color = Color.white;
        divTopImg.raycastTarget = false;

        // Score Card Row
        GameObject scoreCard = new GameObject("Card_Score");
        scoreCard.transform.SetParent(cardGo.transform, false);
        RectTransform scoreCardRt = scoreCard.AddComponent<RectTransform>();
        scoreCardRt.anchoredPosition = new Vector2(0, 150);
        scoreCardRt.sizeDelta = new Vector2(680, 130);
        Image scoreCardImg = scoreCard.AddComponent<Image>();
        scoreCardImg.sprite = GetUISprite("UI_RowCard");
        scoreCardImg.type = Image.Type.Sliced;
        scoreCardImg.color = Color.white;

        var resultLabel = CreateText(scoreCard.transform, "FINAL SCORE ACHIEVED", new Vector2(0, 30), new Vector2(600, 30), 18, new Color(0.95f, 0.7f, 0.7f), TextAlignmentOptions.Center, false);
        resultLabel.fontStyle = FontStyles.Bold;

        var scoreTxt = CreateText(scoreCard.transform, "Score : 0", new Vector2(0, -18), new Vector2(600, 60), 48, new Color(0.95f, 0.3f, 0.3f), TextAlignmentOptions.Center);
        scoreTxt.fontStyle = FontStyles.Bold;
        scoreTxt.gameObject.AddComponent<ScoreDisplay>();

        // Divider Middle
        GameObject divMid = new GameObject("Divider_Mid");
        divMid.transform.SetParent(cardGo.transform, false);
        RectTransform divMidRt = divMid.AddComponent<RectTransform>();
        divMidRt.anchoredPosition = new Vector2(0, 50);
        divMidRt.sizeDelta = new Vector2(720, 3);
        Image divMidImg = divMid.AddComponent<Image>();
        divMidImg.sprite = GetUISprite("UI_Divider_Red");
        divMidImg.color = Color.white;
        divMidImg.raycastTarget = false;

        // Action Buttons
        CreateButton(cardGo.transform, "TRY AGAIN (STAGE 1)", new Vector2(0, -30), new Vector2(440, 70), ctrl, "Stage1", false, "UI_Button_Red");
        CreateButton(cardGo.transform, "STAGE SELECTION", new Vector2(0, -120), new Vector2(440, 70), ctrl, "StageSelection");
        CreateButton(cardGo.transform, "MAIN MENU", new Vector2(0, -210), new Vector2(440, 70), ctrl, "MainMenu");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameOver.unity");
    }

    private static void SetupStage2()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (cam, _, canvas, ctrl) = CreateSceneBase("");

        cam.GetComponent<Camera>().backgroundColor = new Color(0.06f, 0.08f, 0.11f);

        // Backdrop
        GameObject backdropGo = new GameObject("UI_Backdrop");
        backdropGo.transform.SetParent(canvas.transform, false);
        RectTransform bdRt = backdropGo.AddComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero;
        bdRt.anchorMax = Vector2.one;
        bdRt.sizeDelta = Vector2.zero;
        Image bdImg = backdropGo.AddComponent<Image>();
        bdImg.color = new Color(0.04f, 0.06f, 0.09f, 0.65f);
        bdImg.raycastTarget = false;

        // Card Panel (820 x 680)
        GameObject cardGo = new GameObject("Panel_Stage2Card");
        cardGo.transform.SetParent(canvas.transform, false);
        RectTransform cardRt = cardGo.AddComponent<RectTransform>();
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(820, 680);

        Image cardImg = cardGo.AddComponent<Image>();
        Sprite glassSprite = GetUISprite("UI_GlassCard");
        if (glassSprite != null)
        {
            cardImg.sprite = glassSprite;
            cardImg.type = Image.Type.Sliced;
        }
        cardImg.color = Color.white;

        // Tag: [ LEVEL 2 ]
        var tagTmp = CreateText(cardGo.transform, "OBJECTIVE SYSTEM", new Vector2(0, 270), new Vector2(500, 30), 16, new Color(0.22f, 0.74f, 0.97f), TextAlignmentOptions.Center, false);
        tagTmp.fontStyle = FontStyles.Bold;
        tagTmp.characterSpacing = 8f;

        // Title
        var titleTmp = CreateText(cardGo.transform, "STAGE 2 : HUMANOID EXPLORER", new Vector2(0, 215), new Vector2(700, 60), 36, Color.white, TextAlignmentOptions.Center, false);
        titleTmp.fontStyle = FontStyles.Bold;

        // Divider
        GameObject divTop = new GameObject("Divider_Top");
        divTop.transform.SetParent(cardGo.transform, false);
        RectTransform divTopRt = divTop.AddComponent<RectTransform>();
        divTopRt.anchoredPosition = new Vector2(0, 165);
        divTopRt.sizeDelta = new Vector2(720, 3);
        Image divTopImg = divTop.AddComponent<Image>();
        divTopImg.sprite = GetUISprite("UI_Divider");
        divTopImg.color = Color.white;
        divTopImg.raycastTarget = false;

        // Info Row
        GameObject row = new GameObject("Row_Info");
        row.transform.SetParent(cardGo.transform, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.anchoredPosition = new Vector2(0, 50);
        rowRt.sizeDelta = new Vector2(720, 140);
        Image rowImg = row.AddComponent<Image>();
        rowImg.sprite = GetUISprite("UI_RowCard");
        rowImg.type = Image.Type.Sliced;
        rowImg.color = Color.white;

        var hText = CreateText(row.transform, "STAGE 2 LEVEL SETUP READY", new Vector2(0, 25), new Vector2(660, 34), 22, new Color(0.2f, 0.85f, 1f), TextAlignmentOptions.Center, false);
        hText.fontStyle = FontStyles.Bold;
        CreateText(row.transform, "Humanoid Character Model & Interactive Objectives configured.\nReady for Gameplay Testing.", new Vector2(0, -20), new Vector2(660, 48), 17, new Color(0.7f, 0.8f, 0.9f), TextAlignmentOptions.Center, false);

        // Divider Bottom
        GameObject divBottom = new GameObject("Divider_Bottom");
        divBottom.transform.SetParent(cardGo.transform, false);
        RectTransform divBottomRt = divBottom.AddComponent<RectTransform>();
        divBottomRt.anchoredPosition = new Vector2(0, -75);
        divBottomRt.sizeDelta = new Vector2(720, 3);
        Image divBottomImg = divBottom.AddComponent<Image>();
        divBottomImg.sprite = GetUISprite("UI_Divider");
        divBottomImg.color = Color.white;
        divBottomImg.raycastTarget = false;

        // Button
        CreateButton(cardGo.transform, "BACK TO STAGE SELECTION", new Vector2(0, -165), new Vector2(440, 68), ctrl, "StageSelection");

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
        eventSystem.AddComponent<InputSystemUIInputModule>();

        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject ctrlGo = new GameObject("SceneController");
        SceneController sceneCtrl = ctrlGo.AddComponent<SceneController>();

        // HUD: Top Center Stage Banner
        GameObject bannerGo = new GameObject("HUD_StageBanner");
        bannerGo.transform.SetParent(canvas.transform, false);
        RectTransform rtBanner = bannerGo.AddComponent<RectTransform>();
        rtBanner.anchorMin = new Vector2(0.5f, 1);
        rtBanner.anchorMax = new Vector2(0.5f, 1);
        rtBanner.pivot = new Vector2(0.5f, 1);
        rtBanner.anchoredPosition = new Vector2(0, -25);
        rtBanner.sizeDelta = new Vector2(520, 48);
        Image bannerImg = bannerGo.AddComponent<Image>();
        bannerImg.sprite = GetUISprite("UI_Badge");
        bannerImg.type = Image.Type.Sliced;
        bannerImg.color = Color.white;
        var bannerTxt = CreateText(bannerGo.transform, "STAGE 1 : CAPSULE RUNNER", Vector2.zero, new Vector2(500, 40), 20, new Color(0.85f, 0.92f, 1f), TextAlignmentOptions.Center, false);
        bannerTxt.fontStyle = FontStyles.Bold;

        // Top Left Time & Score Panel (Modern HUD Glass Panel)
        GameObject hudPanel = new GameObject("HUD_Panel");
        hudPanel.transform.SetParent(canvas.transform, false);
        RectTransform rtHud = hudPanel.AddComponent<RectTransform>();
        rtHud.anchorMin = new Vector2(0, 1);
        rtHud.anchorMax = new Vector2(0, 1);
        rtHud.pivot = new Vector2(0, 1);
        rtHud.anchoredPosition = new Vector2(40, -30);
        rtHud.sizeDelta = new Vector2(400, 140);

        Image bgHud = hudPanel.AddComponent<Image>();
        bgHud.sprite = GetUISprite("UI_HUD_Panel");
        bgHud.type = Image.Type.Sliced;
        bgHud.color = Color.white;

        var timeTxt = CreateText(hudPanel.transform, "Time: 30", new Vector2(25, 25), new Vector2(350, 45), 28, new Color(0.98f, 0.75f, 0.15f), TextAlignmentOptions.Left, false);
        timeTxt.fontStyle = FontStyles.Bold;

        var scoreTxt = CreateText(hudPanel.transform, "Score: 0", new Vector2(25, -25), new Vector2(350, 45), 28, new Color(0.22f, 0.85f, 1.0f), TextAlignmentOptions.Left, false);
        scoreTxt.fontStyle = FontStyles.Bold;

        // Top Right Menu Button
        GameObject menuBtnGo = CreateButton(canvas.transform, "MENU", Vector2.zero, new Vector2(180, 56), sceneCtrl, "MainMenu").gameObject;
        RectTransform rtMenuBtn = menuBtnGo.GetComponent<RectTransform>();
        rtMenuBtn.anchorMin = new Vector2(1, 1);
        rtMenuBtn.anchorMax = new Vector2(1, 1);
        rtMenuBtn.pivot = new Vector2(1, 1);
        rtMenuBtn.anchoredPosition = new Vector2(-40, -30);

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
