using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Unity.Cinemachine;
using Pasit;

namespace Pasit.Editor
{
    public class Stage2SetupAutomation
    {
        [MenuItem("Tools/Pasit/Setup Stage 2 (Humanoid & Interaction)")]
        public static void SetupStage2Full()
        {
            Debug.Log("==== STARTING STAGE 2 SETUP AUTOMATION ====");

            CreateStage2ItemPrefabs();
            GameObject playerPrefab = CreateHumanoidPlayerPrefab();
            BuildStage2Scene(playerPrefab);

            Debug.Log("==== STAGE 2 SETUP SUCCESSFULLY COMPLETED! ====");
        }

        private static void CreateStage2ItemPrefabs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

            CreateStage2ItemPrefab("Item_Stage2_AddTime", PrimitiveType.Sphere, "Mat_AddTime", Item.ItemType.AddTime, new Vector3(0.8f, 0.8f, 0.8f));
            CreateStage2ItemPrefab("Item_Stage2_ReduceTime", PrimitiveType.Sphere, "Mat_ReduceTime", Item.ItemType.ReduceTime, new Vector3(0.8f, 0.8f, 0.8f));
            CreateStage2ItemPrefab("Item_Stage2_AddScore", PrimitiveType.Cube, "Mat_AddScore", Item.ItemType.AddScore, new Vector3(0.7f, 0.7f, 0.7f));
            CreateStage2ItemPrefab("Item_Stage2_ReduceScore", PrimitiveType.Cube, "Mat_ReduceScore", Item.ItemType.ReduceScore, new Vector3(0.7f, 0.7f, 0.7f));

            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 2 Item Prefabs with Interaction System enabled.");
        }

        private static void CreateStage2ItemPrefab(string name, PrimitiveType shape, string matName, Item.ItemType type, Vector3 scale)
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
            itemComp.requireInteraction = true; // Q.8: Interaction system instead of collision!

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static GameObject CreateHumanoidPlayerPrefab()
        {
            string prefabPath = "Assets/Prefabs/HumanoidPlayer.prefab";

            // Find Ch02_nonPBR avatar
            string fbxPath = "Assets/PracticalExamResources/PracticalExamResources/Character Models/Ch02_nonPBR.fbx";
            Avatar humanoidAvatar = null;
            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (var asset in allSubAssets)
            {
                if (asset is Avatar a)
                {
                    humanoidAvatar = a;
                    break;
                }
            }

            // Load StarterAssets PlayerArmature prefab as base
            string starterArmaturePath = "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/PlayerArmature.prefab";
            GameObject starterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(starterArmaturePath);

            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(starterPrefab);
            playerInstance.name = "HumanoidPlayer";
            playerInstance.tag = "Player";
            PrefabUtility.UnpackPrefabInstance(playerInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // Load and instantiate the Humanoid model mesh (Ch02_nonPBR)
            GameObject ch02Fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (ch02Fbx != null)
            {
                // Disable default starter robot geometry
                Transform geom = playerInstance.transform.Find("Geometry");
                if (geom != null)
                {
                    geom.gameObject.SetActive(false);
                }

                // Instantiate Ch02 character model inside player
                GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(ch02Fbx, playerInstance.transform);
                modelInstance.name = "Ch02_Visual";
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                modelInstance.transform.localScale = Vector3.one;
                PrefabUtility.UnpackPrefabInstance(modelInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                Material matPlayer = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Player.mat");
                var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    if (matPlayer != null) r.sharedMaterial = matPlayer;
                }

                // Configure Animator with Ch02 Avatar
                Animator animator = playerInstance.GetComponent<Animator>();
                if (animator != null && humanoidAvatar != null)
                {
                    animator.avatar = humanoidAvatar;
                }
            }

            // Configure CharacterController
            CharacterController cc = playerInstance.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.center = new Vector3(0, 0.9f, 0);
                cc.radius = 0.35f;
                cc.height = 1.8f;
            }

            // Ensure PlayerCameraRoot exists
            Transform cameraRoot = playerInstance.transform.Find("PlayerCameraRoot");
            if (cameraRoot == null)
            {
                GameObject rootGo = new GameObject("PlayerCameraRoot");
                rootGo.transform.SetParent(playerInstance.transform);
                rootGo.transform.localPosition = new Vector3(0, 1.4f, 0);
                rootGo.tag = "CinemachineTarget";
                cameraRoot = rootGo.transform;
            }

            // Ensure ThirdPersonController has camera target
            var tpc = playerInstance.GetComponent<StarterAssets.ThirdPersonController>();
            if (tpc != null)
            {
                tpc.CinemachineCameraTarget = cameraRoot.gameObject;
            }

            // Attach PlayerInteraction component (Q.6 & Q.8)
            var pi = playerInstance.GetComponent<PlayerInteraction>();
            if (pi == null)
            {
                pi = playerInstance.AddComponent<PlayerInteraction>();
                pi.interactRange = 3.0f;
            }

            // Save as HumanoidPlayer prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(playerInstance, prefabPath);
            UnityEngine.Object.DestroyImmediate(playerInstance);

            Debug.Log("Created HumanoidPlayer prefab with Starter Asset Third Person Controller and Ch02 Humanoid Avatar.");
            return savedPrefab;
        }

        private static void BuildStage2Scene(GameObject playerPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting
            GameObject lightGo = new GameObject("Directional Light");
            Light lightComp = lightGo.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.intensity = 1.25f;
            lightGo.transform.rotation = Quaternion.Euler(45, -35, 0);

            // Level Root
            GameObject levelRoot = new GameObject("Level_ProBuilder");

            Material matPlatform = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Platform.mat");
            Material matBridge = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Bridge.mat");

            // PROBUILDER LEVEL DESIGN (Q.7 - Size > 50 sq meters)
            // 1. Central Courtyard: 16m x 16m = 256 m²
            CreateProBuilderPlatform(levelRoot.transform, "PB_CentralCourtyard", new Vector3(0, 0, 0), new Vector3(16, 1, 16), matPlatform);

            // 2. North Elevated Terrace: 14m x 12m = 168 m² at Y = 2.5m
            CreateProBuilderPlatform(levelRoot.transform, "PB_NorthTerrace", new Vector3(0, 2.5f, 20), new Vector3(14, 1, 12), matPlatform);

            // 3. ProBuilder Staircase connecting Courtyard to North Terrace:
            CreateProBuilderStair(levelRoot.transform, "PB_GrandStairs", new Vector3(0, 0, 10.5f), new Vector3(4, 2.5f, 5), matBridge);

            // 4. East Walkway Bridge: 3.5m x 14m = 49 m²
            CreateProBuilderPlatform(levelRoot.transform, "PB_EastBridge", new Vector3(15f, 0, 0), new Vector3(14, 0.8f, 3.5f), matBridge);

            // 5. East Observation Platform: 10m x 10m = 100 m²
            CreateProBuilderPlatform(levelRoot.transform, "PB_EastPlatform", new Vector3(27f, 0, 0), new Vector3(10, 1, 10), matPlatform);

            // 6. West Walkway Bridge: 3.5m x 14m = 49 m²
            CreateProBuilderPlatform(levelRoot.transform, "PB_WestBridge", new Vector3(-15f, 0, 0), new Vector3(14, 0.8f, 3.5f), matBridge);

            // 7. West Observation Platform: 10m x 10m = 100 m²
            CreateProBuilderPlatform(levelRoot.transform, "PB_WestPlatform", new Vector3(-27f, 0, 0), new Vector3(10, 1, 10), matPlatform);

            // Total walkable ProBuilder area: 256 + 168 + 20 + 49 + 100 + 49 + 100 = 742 m² (> 50 m² required!)

            // SPAWN ITEMS (Q.8 - 4 distinct items with interaction system)
            GameObject itemsRoot = new GameObject("Items_Interactable");
            GameObject pAddScore = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_Stage2_AddScore.prefab");
            GameObject pAddTime = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_Stage2_AddTime.prefab");
            GameObject pReduceTime = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_Stage2_ReduceTime.prefab");
            GameObject pReduceScore = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_Stage2_ReduceScore.prefab");

            // Total 25 AddScore items (25 * 500 = 12,500 score -> exceeds 10,000 winning condition!)
            // Central Courtyard (8 score items)
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-5f, 1.2f, -5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(5f, 1.2f, -5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-5f, 1.2f, 5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(5f, 1.2f, 5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 1.2f, -6f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-6f, 1.2f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(6f, 1.2f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 1.2f, 4f));

            // Stairs & North Terrace (7 score items)
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 2.8f, 11f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-4f, 3.8f, 17f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(4f, 3.8f, 17f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 3.8f, 20f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-4f, 3.8f, 23f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(4f, 3.8f, 23f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 3.8f, 24f));

            // East Bridge & Platform (5 score items)
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(13f, 1.2f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(18f, 1.2f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(25f, 1.2f, -3f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(25f, 1.2f, 3f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(29f, 1.2f, 0));

            // West Bridge & Platform (5 score items)
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-13f, 1.2f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-18f, 1.2f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-25f, 1.2f, -3f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-25f, 1.2f, 3f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-29f, 1.2f, 0));

            // AddTime Items (+3s time bonus - 6 items)
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(0, 1.2f, 0));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(0, 3.8f, 16f));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(27f, 1.2f, 0));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(-27f, 1.2f, 0));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(-3f, 1.2f, -3f));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(3f, 1.2f, -3f));

            // ReduceTime Items (-5s time hazard - 4 items)
            SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(15f, 1.2f, 1.2f));
            SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(-15f, 1.2f, -1.2f));
            SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(-2f, 3.8f, 19f));
            SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(2f, 3.8f, 19f));

            // ReduceScore Items (-500 score hazard - 4 items)
            SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(15f, 1.2f, -1.2f));
            SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(-15f, 1.2f, 1.2f));
            SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(-27f, 1.2f, -2.5f));
            SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(27f, 1.2f, 2.5f));

            // INSTANTIATE PLAYER
            GameObject playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerGo.name = "HumanoidPlayer";
            playerGo.transform.position = new Vector3(0, 1.2f, -2f);
            playerGo.transform.rotation = Quaternion.identity;

            // SETUP CINEMACHINE CAMERAS
            SetupCinemachineCameras(playerGo);

            // SETUP CANVAS & HUD
            var (timeTxt, scoreTxt) = SetupStage2HUD(playerGo);

            // GAMEMANAGER & AUDIOMANAGER
            GameObject gmGo = new GameObject("GameManager");
            GameManager gm = gmGo.AddComponent<GameManager>();
            gm.timeRemaining = 30f;
            gm.currentScore = 0;
            gm.timeText = timeTxt;
            gm.scoreText = scoreTxt;

            GameObject sceneCtrlGo = new GameObject("SceneController");
            sceneCtrlGo.AddComponent<SceneController>();

            // Ensure Audio Manager has clips assigned
            if (AudioManager.Instance != null)
            {
                if (AudioManager.Instance.bgmClip == null)
                    AudioManager.Instance.bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/BGM/Tiny_Blocks.wav");
                if (AudioManager.Instance.itemSfxClip == null)
                    AudioManager.Instance.itemSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/SFX/Coin Pickup 4.wav");
                if (AudioManager.Instance.obstacleSfxClip == null)
                    AudioManager.Instance.obstacleSfxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/PracticalExamResources/Sounds/SFX/Debuff Downgrade 1.wav");
            }

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Stage2.unity");
            Debug.Log("Successfully built and saved Assets/Scenes/Stage2.unity!");
        }

        private static void CreateProBuilderPlatform(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var pbMesh = ShapeGenerator.CreateShape(ShapeType.Cube);
            pbMesh.gameObject.name = name;
            pbMesh.transform.SetParent(parent, false);
            pbMesh.transform.position = pos;
            pbMesh.transform.localScale = scale;

            if (mat != null)
            {
                pbMesh.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            // Collider
            var collider = pbMesh.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = Vector3.one;
        }

        private static void CreateProBuilderStair(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var pbMesh = ShapeGenerator.CreateShape(ShapeType.Stair);
            pbMesh.gameObject.name = name;
            pbMesh.transform.SetParent(parent, false);
            pbMesh.transform.position = pos;
            pbMesh.transform.localScale = scale;

            if (mat != null)
            {
                pbMesh.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            var collider = pbMesh.gameObject.AddComponent<MeshCollider>();
        }

        private static void SpawnItem(GameObject prefab, Transform parent, Vector3 pos)
        {
            if (prefab == null) return;
            GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            item.transform.SetParent(parent, false);
            item.transform.position = pos;
        }

        private static void SetupCinemachineCameras(GameObject playerGo)
        {
            // Main Camera with CinemachineBrain
            GameObject mainCamGo = new GameObject("Main Camera");
            mainCamGo.tag = "MainCamera";
            Camera cam = mainCamGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            mainCamGo.AddComponent<AudioListener>();
            mainCamGo.AddComponent<CinemachineBrain>();

            // Virtual Camera (CinemachineCamera)
            string followCamPrefabPath = "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/PlayerFollowCamera.prefab";
            GameObject followCamPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(followCamPrefabPath);

            GameObject vcamGo = null;
            if (followCamPrefab != null)
            {
                vcamGo = (GameObject)PrefabUtility.InstantiatePrefab(followCamPrefab);
                vcamGo.name = "PlayerFollowCamera";
            }
            else
            {
                vcamGo = new GameObject("PlayerFollowCamera");
                vcamGo.AddComponent<CinemachineCamera>();
            }

            Transform cameraRoot = playerGo.transform.Find("PlayerCameraRoot");
            if (cameraRoot != null)
            {
                var cmCam = vcamGo.GetComponent<CinemachineCamera>();
                if (cmCam != null)
                {
                    cmCam.Target.TrackingTarget = cameraRoot;
                }
            }
        }

        private static (TextMeshProUGUI timeTxt, TextMeshProUGUI scoreTxt) SetupStage2HUD(GameObject playerGo)
        {
            // EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            // Canvas
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Top Stage Banner
            GameObject bannerGo = new GameObject("HUD_StageBanner");
            bannerGo.transform.SetParent(canvas.transform, false);
            RectTransform bannerRt = bannerGo.AddComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 1f);
            bannerRt.anchorMax = new Vector2(0.5f, 1f);
            bannerRt.pivot = new Vector2(0.5f, 1f);
            bannerRt.anchoredPosition = new Vector2(0, -20);
            bannerRt.sizeDelta = new Vector2(620, 52);

            Image bannerImg = bannerGo.AddComponent<Image>();
            bannerImg.sprite = GetUISprite("UI_Badge");
            bannerImg.type = Image.Type.Sliced;
            bannerImg.color = Color.white;

            var bannerText = CreateText(bannerGo.transform, "STAGE 2 : HUMANOID EXPLORER", Vector2.zero, new Vector2(600, 40), 20, new Color(0.25f, 0.85f, 1f), TextAlignmentOptions.Center, false);
            bannerText.fontStyle = FontStyles.Bold;

            // HUD Stats Panel (Top-Left)
            GameObject panelGo = new GameObject("HUD_Panel");
            panelGo.transform.SetParent(canvas.transform, false);
            RectTransform panelRt = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0, 1);
            panelRt.anchorMax = new Vector2(0, 1);
            panelRt.pivot = new Vector2(0, 1);
            panelRt.anchoredPosition = new Vector2(30, -30);
            panelRt.sizeDelta = new Vector2(340, 170);

            Image panelImg = panelGo.AddComponent<Image>();
            panelImg.sprite = GetUISprite("UI_GlassCard");
            panelImg.type = Image.Type.Sliced;
            panelImg.color = Color.white;

            var timeTxt = CreateText(panelGo.transform, "Time: 30", new Vector2(25, -35), new Vector2(290, 40), 28, Color.white, TextAlignmentOptions.Left);
            timeTxt.name = "Text_Time";
            timeTxt.fontStyle = FontStyles.Bold;

            var scoreTxt = CreateText(panelGo.transform, "Score: 0", new Vector2(25, -80), new Vector2(290, 40), 28, new Color(1f, 0.88f, 0.2f), TextAlignmentOptions.Left);
            scoreTxt.name = "Text_Score";
            scoreTxt.fontStyle = FontStyles.Bold;

            var targetTxt = CreateText(panelGo.transform, "GOAL: > 10,000 PTS TO WIN", new Vector2(25, -125), new Vector2(290, 30), 16, new Color(0.35f, 0.95f, 0.6f), TextAlignmentOptions.Left, false);
            targetTxt.fontStyle = FontStyles.Bold;

            // Interaction Prompt Badge (Center-Bottom)
            GameObject promptPillGo = new GameObject("HUD_InteractionPrompt");
            promptPillGo.transform.SetParent(canvas.transform, false);
            RectTransform promptRt = promptPillGo.AddComponent<RectTransform>();
            promptRt.anchorMin = new Vector2(0.5f, 0.25f);
            promptRt.anchorMax = new Vector2(0.5f, 0.25f);
            promptRt.pivot = new Vector2(0.5f, 0.5f);
            promptRt.anchoredPosition = Vector2.zero;
            promptRt.sizeDelta = new Vector2(500, 60);

            Image promptImg = promptPillGo.AddComponent<Image>();
            promptImg.sprite = GetUISprite("UI_Button_Green");
            promptImg.type = Image.Type.Sliced;
            promptImg.color = new Color(1f, 1f, 1f, 0.92f);

            var promptText = CreateText(promptPillGo.transform, "[E] INTERACT", Vector2.zero, new Vector2(460, 48), 24, Color.white, TextAlignmentOptions.Center, false);
            promptText.name = "Text_Prompt";
            promptText.fontStyle = FontStyles.Bold;

            // Link Prompt to PlayerInteraction component
            var pi = playerGo.GetComponent<PlayerInteraction>();
            if (pi != null)
            {
                pi.promptText = promptText;
                pi.promptUI = promptPillGo;
            }

            // Top-Right: Return to Menu Button
            GameObject btnMenuGo = new GameObject("Btn_MENU");
            btnMenuGo.transform.SetParent(canvas.transform, false);
            RectTransform btnMenuRt = btnMenuGo.AddComponent<RectTransform>();
            btnMenuRt.anchorMin = new Vector2(1, 1);
            btnMenuRt.anchorMax = new Vector2(1, 1);
            btnMenuRt.pivot = new Vector2(1, 1);
            btnMenuRt.anchoredPosition = new Vector2(-30, -30);
            btnMenuRt.sizeDelta = new Vector2(170, 50);

            Image btnMenuImg = btnMenuGo.AddComponent<Image>();
            btnMenuImg.sprite = GetUISprite("UI_Button_Default");
            btnMenuImg.type = Image.Type.Sliced;
            btnMenuImg.color = Color.white;

            Button btnMenu = btnMenuGo.AddComponent<Button>();
            btnMenu.targetGraphic = btnMenuImg;
            ColorBlock cb = btnMenu.colors;
            cb.highlightedColor = new Color(0.3f, 0.7f, 1f);
            cb.pressedColor = new Color(0.15f, 0.45f, 0.8f);
            btnMenu.colors = cb;

            var sceneCtrl = canvasGo.AddComponent<SceneController>();
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btnMenu.onClick, sceneCtrl.LoadScene, "StageSelection");

            var btnText = CreateText(btnMenuGo.transform, "MENU", Vector2.zero, new Vector2(150, 40), 20, Color.white, TextAlignmentOptions.Center, false);
            btnText.fontStyle = FontStyles.Bold;

            return (timeTxt, scoreTxt);
        }

        private static Sprite GetUISprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/" + name + ".png");
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
    }
}
