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
            Debug.Log("==== STARTING STAGE 2 SETUP AUTOMATION (FBX ASSETS & VIBRANT UI) ====");

            EnsureStage2Materials();
            UISpriteGenerator.GenerateSprites();
            CreateStage2ItemPrefabs();
            GameObject playerPrefab = CreateHumanoidPlayerPrefab();
            BuildStage2Scene(playerPrefab);

            Debug.Log("==== STAGE 2 SETUP SUCCESSFULLY COMPLETED! ====");
        }

        private static void EnsureStage2Materials()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            // 1. Palette Material for all imported FBX models
            string matPalettePath = "Assets/Materials/Mat_Palette.mat";
            Material matPalette = AssetDatabase.LoadAssetAtPath<Material>(matPalettePath);
            if (matPalette == null)
            {
                matPalette = new Material(litShader);
                matPalette.name = "Mat_Palette";
                Texture2D paletteTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MyPalette.png");
                matPalette.SetTexture("_BaseMap", paletteTex);
                matPalette.SetFloat("_Smoothness", 0.1f);
                matPalette.SetFloat("_Metallic", 0.0f);
                AssetDatabase.CreateAsset(matPalette, matPalettePath);
            }
            else
            {
                Texture2D paletteTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MyPalette.png");
                matPalette.SetTexture("_BaseMap", paletteTex);
            }

            // 2. Stage 2 Platform Grass Material (Lush vibrant low-poly green)
            string matGrassPath = "Assets/Materials/Mat_Stage2_Grass.mat";
            Material matGrass = AssetDatabase.LoadAssetAtPath<Material>(matGrassPath);
            if (matGrass == null)
            {
                matGrass = new Material(litShader);
                matGrass.name = "Mat_Stage2_Grass";
                matGrass.SetColor("_BaseColor", new Color(0.40f, 0.76f, 0.22f, 1f));
                matGrass.SetFloat("_Smoothness", 0.1f);
                AssetDatabase.CreateAsset(matGrass, matGrassPath);
            }

            // 3. Stage 2 Platform Wood Material (Warm stylized cedar wood)
            string matWoodPath = "Assets/Materials/Mat_Stage2_Wood.mat";
            Material matWood = AssetDatabase.LoadAssetAtPath<Material>(matWoodPath);
            if (matWood == null)
            {
                matWood = new Material(litShader);
                matWood.name = "Mat_Stage2_Wood";
                matWood.SetColor("_BaseColor", new Color(0.56f, 0.36f, 0.20f, 1f));
                matWood.SetFloat("_Smoothness", 0.15f);
                AssetDatabase.CreateAsset(matWood, matWoodPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Stage2SetupAutomation] Materials verified.");
        }

        private static void CreateStage2ItemPrefabs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

            // 1. AddScore Item: Pickup.fbx (Golden rotating pickup coin)
            CreateFBXItemPrefab(
                "Item_Stage2_AddScore",
                "Pickup",
                Item.ItemType.AddScore,
                new Vector3(0.65f, 0.65f, 0.65f),
                new Vector3(2.2f, 2.2f, 1.2f),
                new Vector3(0f, 1.0f, 0f)
            );

            // 2. AddTime Item: flower_Yellow.fbx (Bright sunny flower bonus)
            CreateFBXItemPrefab(
                "Item_Stage2_AddTime",
                "flower_Yellow",
                Item.ItemType.AddTime,
                new Vector3(0.85f, 0.85f, 0.85f),
                new Vector3(1.6f, 1.8f, 1.6f),
                new Vector3(0f, 0.7f, 0f)
            );

            // 3. ReduceScore Item: Spikes.fbx (Hazard spike obstacle)
            CreateFBXItemPrefab(
                "Item_Stage2_ReduceScore",
                "Spikes",
                Item.ItemType.ReduceScore,
                new Vector3(0.6f, 0.6f, 0.6f),
                new Vector3(1.8f, 3.5f, 2.8f),
                new Vector3(0f, 1.7f, -1.2f)
            );

            // 4. ReduceTime Item: Spikes2.fbx (Deadly spike trap obstacle)
            CreateFBXItemPrefab(
                "Item_Stage2_ReduceTime",
                "Spikes2",
                Item.ItemType.ReduceTime,
                new Vector3(0.55f, 0.55f, 0.55f),
                new Vector3(3.2f, 3.0f, 3.5f),
                new Vector3(0f, 1.5f, 0f)
            );

            AssetDatabase.SaveAssets();
            Debug.Log("[Stage2SetupAutomation] Created Stage 2 Item Prefabs with FBX assets and Interaction System.");
        }

        private static void CreateFBXItemPrefab(string prefabName, string fbxName, Item.ItemType type, Vector3 rootScale, Vector3 colliderSize, Vector3 colliderCenter)
        {
            string prefabPath = "Assets/Prefabs/" + prefabName + ".prefab";
            string fbxPath = "Assets/FBX/" + fbxName + ".fbx";
            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogError("FBX Asset not found: " + fbxPath);
                return;
            }

            GameObject go = new GameObject(prefabName);
            go.transform.localScale = rootScale;

            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, go.transform);
            visual.name = "Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            Material matPalette = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Palette.mat");
            if (matPalette != null)
            {
                foreach (var r in visual.GetComponentsInChildren<Renderer>(true))
                {
                    r.sharedMaterial = matPalette;
                }
            }

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            go.layer = ignoreRaycast;
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = ignoreRaycast;
            }

            BoxCollider col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = colliderSize;
            col.center = colliderCenter;

            Item itemComp = go.AddComponent<Item>();
            itemComp.type = type;
            itemComp.requireInteraction = true; // Q.8: Interaction system enabled!

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
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
                Transform geom = playerInstance.transform.Find("Geometry");
                if (geom != null)
                {
                    geom.gameObject.SetActive(false);
                }

                GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(ch02Fbx, playerInstance.transform);
                modelInstance.name = "Ch02_Visual";
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                modelInstance.transform.localScale = Vector3.one;
                PrefabUtility.UnpackPrefabInstance(modelInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

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

            // Attach PlayerInteraction component
            var pi = playerInstance.GetComponent<PlayerInteraction>();
            if (pi == null)
            {
                pi = playerInstance.AddComponent<PlayerInteraction>();
                pi.interactRange = 3.0f;
            }

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(playerInstance, prefabPath);
            UnityEngine.Object.DestroyImmediate(playerInstance);

            // Apply character materials to prefab
            SetupCharacterMaterials.ApplyCharacterMaterials();

            Debug.Log("[Stage2SetupAutomation] Created HumanoidPlayer prefab with Starter Asset Third Person Controller.");
            return savedPrefab;
        }

        private static void BuildStage2Scene(GameObject playerPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Lighting (Warm sunny golden atmosphere matching the cartoon aesthetic)
            GameObject lightGo = new GameObject("Directional Light");
            Light lightComp = lightGo.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(1.0f, 0.97f, 0.88f);
            lightComp.intensity = 1.35f;
            lightComp.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48, -32, 0);

            // 2. ProBuilder Platforms (Q.7 - Size > 50 sq meters)
            GameObject levelRoot = new GameObject("Level_ProBuilder");
            Material matGrass = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Stage2_Grass.mat");
            Material matWood = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Stage2_Wood.mat");
            Material matPalette = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Palette.mat");

            // Central Courtyard: 16m x 16m = 256 m² (Surface at Y = 0.5)
            CreateProBuilderPlatform(levelRoot.transform, "PB_CentralCourtyard", new Vector3(0, 0, 0), new Vector3(16, 1, 16), matGrass);

            // North Elevated Terrace: 14m x 12m = 168 m² at Y = 2.5 (Surface at Y = 3.0)
            CreateProBuilderPlatform(levelRoot.transform, "PB_NorthTerrace", new Vector3(0, 2.5f, 20), new Vector3(14, 1, 12), matGrass);

            // ProBuilder Grand Stairs connecting Courtyard to North Terrace
            CreateProBuilderStair(levelRoot.transform, "PB_GrandStairs", new Vector3(0, 0, 10.5f), new Vector3(4, 2.5f, 5), matWood);

            // East Walkway Bridge: 3.5m x 14m = 49 m² (Surface at Y = 0.4)
            CreateProBuilderPlatform(levelRoot.transform, "PB_EastBridge", new Vector3(15f, 0, 0), new Vector3(14, 0.8f, 3.5f), matWood);

            // East Observation Platform: 10m x 10m = 100 m² (Surface at Y = 0.5)
            CreateProBuilderPlatform(levelRoot.transform, "PB_EastPlatform", new Vector3(27f, 0, 0), new Vector3(10, 1, 10), matGrass);

            // West Walkway Bridge: 3.5m x 14m = 49 m² (Surface at Y = 0.4)
            CreateProBuilderPlatform(levelRoot.transform, "PB_WestBridge", new Vector3(-15f, 0, 0), new Vector3(14, 0.8f, 3.5f), matWood);

            // West Observation Platform: 10m x 10m = 100 m² (Surface at Y = 0.5)
            CreateProBuilderPlatform(levelRoot.transform, "PB_WestPlatform", new Vector3(-27f, 0, 0), new Vector3(10, 1, 10), matGrass);

            // Total ProBuilder walkable area = 742 m² (> 50 m² required by rubric)

            // 3. FBX Environment Scenery Decoration Root
            GameObject envRoot = new GameObject("Environment_FBX");

            // Decorative Trees around borders
            SpawnFBXDecoration("Tree1", envRoot.transform, new Vector3(-6.5f, 0.5f, -6.5f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0, 25, 0), matPalette);
            SpawnFBXDecoration("Tree1", envRoot.transform, new Vector3(6.5f, 0.5f, -6.5f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0, -45, 0), matPalette);
            SpawnFBXDecoration("Tree2", envRoot.transform, new Vector3(-6.5f, 0.5f, 6.5f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0, 80, 0), matPalette);
            SpawnFBXDecoration("Tree2", envRoot.transform, new Vector3(6.5f, 0.5f, 6.5f), new Vector3(0.9f, 0.9f, 0.9f), Quaternion.Euler(0, -90, 0), matPalette);

            SpawnFBXDecoration("Tree3", envRoot.transform, new Vector3(-5.5f, 3.0f, 24.5f), new Vector3(1f, 1f, 1f), Quaternion.identity, matPalette);
            SpawnFBXDecoration("Tree4", envRoot.transform, new Vector3(5.5f, 3.0f, 24.5f), new Vector3(1f, 1f, 1f), Quaternion.Euler(0, 120, 0), matPalette);

            SpawnFBXDecoration("Tree1", envRoot.transform, new Vector3(30f, 0.5f, -3.5f), new Vector3(0.85f, 0.85f, 0.85f), Quaternion.Euler(0, 60, 0), matPalette);
            SpawnFBXDecoration("Tree3", envRoot.transform, new Vector3(30f, 0.5f, 3.5f), new Vector3(0.85f, 0.85f, 0.85f), Quaternion.Euler(0, -30, 0), matPalette);
            SpawnFBXDecoration("Tree2", envRoot.transform, new Vector3(-30f, 0.5f, -3.5f), new Vector3(0.85f, 0.85f, 0.85f), Quaternion.Euler(0, 45, 0), matPalette);
            SpawnFBXDecoration("Tree4", envRoot.transform, new Vector3(-30f, 0.5f, 3.5f), new Vector3(0.85f, 0.85f, 0.85f), Quaternion.Euler(0, 140, 0), matPalette);

            // Low-poly Decorative Wooden Crates
            SpawnFBXDecoration("Crate_Big", envRoot.transform, new Vector3(-2.8f, 0.5f, 8.5f), new Vector3(0.6f, 0.6f, 0.6f), Quaternion.Euler(0, 15, 0), matPalette);
            SpawnFBXDecoration("Crate_Small", envRoot.transform, new Vector3(-2.3f, 0.5f, 9.2f), new Vector3(0.7f, 0.7f, 0.7f), Quaternion.Euler(0, -20, 0), matPalette);
            SpawnFBXDecoration("Crate_Big", envRoot.transform, new Vector3(2.8f, 0.5f, 8.5f), new Vector3(0.6f, 0.6f, 0.6f), Quaternion.Euler(0, -10, 0), matPalette);
            SpawnFBXDecoration("Crate_Small", envRoot.transform, new Vector3(2.4f, 0.5f, 9.2f), new Vector3(0.7f, 0.7f, 0.7f), Quaternion.Euler(0, 35, 0), matPalette);

            SpawnFBXDecoration("Crate_Big", envRoot.transform, new Vector3(9f, 0.5f, 2.5f), new Vector3(0.65f, 0.65f, 0.65f), Quaternion.Euler(0, 20, 0), matPalette);
            SpawnFBXDecoration("Crate_Big", envRoot.transform, new Vector3(-9f, 0.5f, 2.5f), new Vector3(0.65f, 0.65f, 0.65f), Quaternion.Euler(0, -20, 0), matPalette);

            // Flowers in Courtyard and Terraces
            SpawnFBXDecoration("flower_RedBlue", envRoot.transform, new Vector3(-5.2f, 0.5f, -6f), Vector3.one, Quaternion.Euler(0, 30, 0), matPalette);
            SpawnFBXDecoration("flower_Yellow", envRoot.transform, new Vector3(5.2f, 0.5f, -6f), Vector3.one, Quaternion.Euler(0, -50, 0), matPalette);
            SpawnFBXDecoration("flower_YellowPurple", envRoot.transform, new Vector3(-6f, 0.5f, 5.2f), Vector3.one, Quaternion.Euler(0, 15, 0), matPalette);
            SpawnFBXDecoration("flower_RedBlue", envRoot.transform, new Vector3(6f, 0.5f, 5.2f), Vector3.one, Quaternion.Euler(0, -75, 0), matPalette);

            SpawnFBXDecoration("flower_YellowPurple", envRoot.transform, new Vector3(-4f, 3.0f, 25f), Vector3.one, Quaternion.identity, matPalette);
            SpawnFBXDecoration("flower_RedBlue", envRoot.transform, new Vector3(4f, 3.0f, 25f), Vector3.one, Quaternion.Euler(0, 45, 0), matPalette);

            SpawnFBXDecoration("flower_Yellow", envRoot.transform, new Vector3(25f, 0.5f, 4f), Vector3.one, Quaternion.identity, matPalette);
            SpawnFBXDecoration("flower_YellowPurple", envRoot.transform, new Vector3(-25f, 0.5f, 4f), Vector3.one, Quaternion.identity, matPalette);

            // Grass Clumps along Walkways
            SpawnFBXDecoration("Grass", envRoot.transform, new Vector3(-4.5f, 0.5f, -3f), Vector3.one, Quaternion.identity, matPalette);
            SpawnFBXDecoration("Grass.001", envRoot.transform, new Vector3(4.5f, 0.5f, -3f), Vector3.one, Quaternion.Euler(0, 90, 0), matPalette);
            SpawnFBXDecoration("Grass.002", envRoot.transform, new Vector3(-4.5f, 0.5f, 3f), Vector3.one, Quaternion.Euler(0, 180, 0), matPalette);
            SpawnFBXDecoration("Grass.003", envRoot.transform, new Vector3(4.5f, 0.5f, 3f), Vector3.one, Quaternion.Euler(0, -90, 0), matPalette);

            SpawnFBXDecoration("Grass.004", envRoot.transform, new Vector3(14f, 0.4f, 1.2f), Vector3.one, Quaternion.identity, matPalette);
            SpawnFBXDecoration("Grass.001", envRoot.transform, new Vector3(-14f, 0.4f, 1.2f), Vector3.one, Quaternion.identity, matPalette);

            // Landmark: Golden Portal / Victory Gate on North Terrace
            SpawnFBXDecoration("Door_Yellow", envRoot.transform, new Vector3(0f, 3.0f, 25.2f), new Vector3(1.15f, 1.15f, 1.15f), Quaternion.identity, matPalette);

            // Distant Low-poly Mountains & Hills in the northern background
            SpawnFBXDecoration("Mountain", envRoot.transform, new Vector3(0f, 0f, 50f), new Vector3(3f, 3f, 3f), Quaternion.identity, matPalette);
            SpawnFBXDecoration("Hill", envRoot.transform, new Vector3(-30f, 0f, 45f), new Vector3(2.5f, 2.5f, 2.5f), Quaternion.Euler(0, 30, 0), matPalette);
            SpawnFBXDecoration("Hill", envRoot.transform, new Vector3(30f, 0f, 45f), new Vector3(2.5f, 2.5f, 2.5f), Quaternion.Euler(0, -45, 0), matPalette);
            SpawnFBXDecoration("Rock", envRoot.transform, new Vector3(-16f, 0f, 38f), new Vector3(2f, 2f, 2f), Quaternion.Euler(0, 70, 0), matPalette);
            SpawnFBXDecoration("Rock", envRoot.transform, new Vector3(16f, 0f, 38f), new Vector3(2f, 2f, 2f), Quaternion.Euler(0, -50, 0), matPalette);

            // Terrace Balcony Boundary Walls
            SpawnFBXDecoration("Walls", envRoot.transform, new Vector3(-6.2f, 3.0f, 26f), new Vector3(1.5f, 1f, 1f), Quaternion.identity, matPalette);
            SpawnFBXDecoration("Walls", envRoot.transform, new Vector3(6.2f, 3.0f, 26f), new Vector3(1.5f, 1f, 1f), Quaternion.identity, matPalette);

            // 4. SPAWN ITEMS (FBX Assets, 4 Distinct Item Types, Interaction System Q.8)
            GameObject itemsRoot = new GameObject("Items_Interactable");
            GameObject pAddScore = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_Stage2_AddScore.prefab");
            GameObject pAddTime = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_Stage2_AddTime.prefab");
            GameObject pReduceScore = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_Stage2_ReduceScore.prefab");
            GameObject pReduceTime = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item_Stage2_ReduceTime.prefab");

            // Total 25 AddScore items (Pickup.fbx) (25 * 500 = 12,500 score -> exceeds 10,000 pts winning condition)
            // Central Courtyard (8 score items floating at Y = 1.0)
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-5f, 1.0f, -5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(5f, 1.0f, -5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-5f, 1.0f, 5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(5f, 1.0f, 5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-2.5f, 1.0f, -5.5f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-6f, 1.0f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(6f, 1.0f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 1.0f, 4f));

            // Stairs & North Terrace (7 score items floating at Y = 2.5 to 3.6)
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 2.5f, 11f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-4f, 3.6f, 17f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(4f, 3.6f, 17f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 3.6f, 20f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-4f, 3.6f, 23f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(4f, 3.6f, 23f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(0, 3.6f, 24f));

            // East Bridge & Platform (5 score items)
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(13f, 1.0f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(18f, 1.0f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(25f, 1.0f, -3f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(25f, 1.0f, 3f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(29f, 1.0f, 0));

            // West Bridge & Platform (5 score items)
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-13f, 1.0f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-18f, 1.0f, 0));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-25f, 1.0f, -3f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-25f, 1.0f, 3f));
            SpawnItem(pAddScore, itemsRoot.transform, new Vector3(-29f, 1.0f, 0));

            // AddTime Items (flower_Yellow.fbx) (+3s bonus - 6 items)
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(0, 0.9f, 0));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(0, 3.4f, 16f));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(27f, 0.9f, 0));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(-27f, 0.9f, 0));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(-3f, 0.9f, -3f));
            SpawnItem(pAddTime, itemsRoot.transform, new Vector3(3f, 0.9f, -3f));

            // ReduceTime Hazard Items (Spikes2.fbx) (-5s hazard - 4 items resting on ground)
            SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(15f, 0.4f, 1.2f));
            SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(-15f, 0.4f, -1.2f));
            SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(-2f, 3.0f, 19f));
            SpawnItem(pReduceTime, itemsRoot.transform, new Vector3(2f, 3.0f, 19f));

            // ReduceScore Hazard Items (Spikes.fbx) (-500 score hazard - 4 items resting on ground)
            SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(15f, 0.4f, -1.2f));
            SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(-15f, 0.4f, 1.2f));
            SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(-27f, 0.5f, -2.5f));
            SpawnItem(pReduceScore, itemsRoot.transform, new Vector3(27f, 0.5f, 2.5f));

            // 5. INSTANTIATE HUMANOID PLAYER
            GameObject playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerGo.name = "HumanoidPlayer";
            playerGo.transform.position = new Vector3(0, 0.6f, -3f);
            playerGo.transform.rotation = Quaternion.identity;

            // 6. SETUP CINEMACHINE CAMERAS
            Camera cam = SetupCinemachineCameras(playerGo);

            // 7. SETUP VIBRANT UI HUD (MATCHING ASSET PALETTE - NO TEXT STRING MODIFICATION!)
            var (timeTxt, scoreTxt) = SetupVibrantStage2HUD(playerGo, cam);

            // 8. GAMEMANAGER & AUDIOMANAGER
            GameObject gmGo = new GameObject("GameManager");
            GameManager gm = gmGo.AddComponent<GameManager>();
            gm.timeRemaining = 30f;
            gm.currentScore = 0;
            gm.timeText = timeTxt;
            gm.scoreText = scoreTxt;

            GameObject sceneCtrlGo = new GameObject("SceneController");
            sceneCtrlGo.AddComponent<SceneController>();

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
            Debug.Log("[Stage2SetupAutomation] Successfully built and saved Assets/Scenes/Stage2.unity!");
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

            pbMesh.gameObject.AddComponent<MeshCollider>();
        }

        private static void SpawnFBXDecoration(string fbxName, Transform parent, Vector3 pos, Vector3 scale, Quaternion rot, Material mat)
        {
            string path = "Assets/FBX/" + fbxName + ".fbx";
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (fbx == null) return;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx, parent);
            instance.name = fbxName + "_Decor";
            instance.transform.position = pos;
            instance.transform.rotation = rot;
            instance.transform.localScale = scale;

            if (mat != null)
            {
                foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
                {
                    r.sharedMaterial = mat;
                }
            }
        }

        private static void SpawnItem(GameObject prefab, Transform parent, Vector3 pos)
        {
            if (prefab == null) return;
            GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            item.transform.position = pos;
        }

        private static Camera SetupCinemachineCameras(GameObject playerGo)
        {
            GameObject mainCamGo = new GameObject("Main Camera");
            mainCamGo.tag = "MainCamera";
            mainCamGo.transform.position = new Vector3(0, 2.2f, -6.5f);
            mainCamGo.transform.rotation = Quaternion.Euler(14, 0, 0);

            Camera cam = mainCamGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            mainCamGo.AddComponent<AudioListener>();
            mainCamGo.AddComponent<CinemachineBrain>();

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

            return cam;
        }

        private static (TextMeshProUGUI timeTxt, TextMeshProUGUI scoreTxt) SetupVibrantStage2HUD(GameObject playerGo, Camera cam)
        {
            // EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            // Canvas
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.0f;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // 1. Top Stage Banner: STAGE 2 : HUMANOID EXPLORER (Exact text string preserved!)
            GameObject bannerGo = new GameObject("HUD_StageBanner");
            bannerGo.transform.SetParent(canvas.transform, false);
            RectTransform bannerRt = bannerGo.AddComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 1f);
            bannerRt.anchorMax = new Vector2(0.5f, 1f);
            bannerRt.pivot = new Vector2(0.5f, 1f);
            bannerRt.anchoredPosition = new Vector2(0, -18);
            bannerRt.sizeDelta = new Vector2(620, 52);

            Image bannerImg = bannerGo.AddComponent<Image>();
            bannerImg.sprite = GetUISprite("UI_Vibrant_Banner");
            bannerImg.type = Image.Type.Sliced;
            bannerImg.color = Color.white;

            var bannerText = CreateText(bannerGo.transform, "STAGE 2 : HUMANOID EXPLORER", Vector2.zero, new Vector2(600, 40), 20, new Color(0.15f, 0.95f, 1f), TextAlignmentOptions.Center, false);
            bannerText.fontStyle = FontStyles.Bold;

            // 2. HUD Stats Panel (Top-Left): Time, Score, Goal (Exact text strings preserved!)
            GameObject panelGo = new GameObject("HUD_Panel");
            panelGo.transform.SetParent(canvas.transform, false);
            RectTransform panelRt = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0, 1);
            panelRt.anchorMax = new Vector2(0, 1);
            panelRt.pivot = new Vector2(0, 1);
            panelRt.anchoredPosition = new Vector2(30, -30);
            panelRt.sizeDelta = new Vector2(340, 170);

            Image panelImg = panelGo.AddComponent<Image>();
            panelImg.sprite = GetUISprite("UI_Vibrant_Card");
            panelImg.type = Image.Type.Sliced;
            panelImg.color = Color.white;

            // Time text: Vibrant Lime Green
            var timeTxt = CreateText(panelGo.transform, "Time: 30", new Vector2(25, -35), new Vector2(290, 40), 28, new Color(0.35f, 1.0f, 0.55f), TextAlignmentOptions.Left);
            timeTxt.name = "Text_Time";
            timeTxt.fontStyle = FontStyles.Bold;

            // Score text: Radiant Sun Gold
            var scoreTxt = CreateText(panelGo.transform, "Score: 0", new Vector2(25, -80), new Vector2(290, 40), 28, new Color(1.0f, 0.85f, 0.15f), TextAlignmentOptions.Left);
            scoreTxt.name = "Text_Score";
            scoreTxt.fontStyle = FontStyles.Bold;

            // Target goal text: Vibrant Coral
            var targetTxt = CreateText(panelGo.transform, "GOAL: > 10,000 PTS TO WIN", new Vector2(25, -125), new Vector2(290, 30), 16, new Color(1.0f, 0.45f, 0.55f), TextAlignmentOptions.Left, false);
            targetTxt.fontStyle = FontStyles.Bold;

            // 3. Interaction Prompt Pill: [E] INTERACT (Exact text string preserved!)
            GameObject promptPillGo = new GameObject("HUD_InteractionPrompt");
            promptPillGo.transform.SetParent(canvas.transform, false);
            RectTransform promptRt = promptPillGo.AddComponent<RectTransform>();
            promptRt.anchorMin = new Vector2(0.5f, 0.22f);
            promptRt.anchorMax = new Vector2(0.5f, 0.22f);
            promptRt.pivot = new Vector2(0.5f, 0.5f);
            promptRt.anchoredPosition = Vector2.zero;
            promptRt.sizeDelta = new Vector2(500, 60);

            Image promptImg = promptPillGo.AddComponent<Image>();
            promptImg.sprite = GetUISprite("UI_Vibrant_Prompt");
            promptImg.type = Image.Type.Sliced;
            promptImg.color = Color.white;

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

            // 4. Return to Menu Button: MENU (Exact text string preserved!)
            GameObject btnMenuGo = new GameObject("Btn_MENU");
            btnMenuGo.transform.SetParent(canvas.transform, false);
            RectTransform btnMenuRt = btnMenuGo.AddComponent<RectTransform>();
            btnMenuRt.anchorMin = new Vector2(1, 1);
            btnMenuRt.anchorMax = new Vector2(1, 1);
            btnMenuRt.pivot = new Vector2(1, 1);
            btnMenuRt.anchoredPosition = new Vector2(-30, -30);
            btnMenuRt.sizeDelta = new Vector2(170, 50);

            Image btnMenuImg = btnMenuGo.AddComponent<Image>();
            btnMenuImg.sprite = GetUISprite("UI_Vibrant_Button");
            btnMenuImg.type = Image.Type.Sliced;
            btnMenuImg.color = Color.white;

            Button btnMenu = btnMenuGo.AddComponent<Button>();
            btnMenu.targetGraphic = btnMenuImg;
            ColorBlock cb = btnMenu.colors;
            cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btnMenu.colors = cb;

            var sceneCtrl = canvasGo.AddComponent<SceneController>();
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btnMenu.onClick, sceneCtrl.LoadScene, "StageSelection");

            var btnText = CreateText(btnMenuGo.transform, "MENU", Vector2.zero, new Vector2(150, 40), 20, Color.white, TextAlignmentOptions.Center, false);
            btnText.fontStyle = FontStyles.Bold;

            return (timeTxt, scoreTxt);
        }

        private static Sprite GetUISprite(string name)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/" + name + ".png");
            if (s == null) s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/UI_Badge.png");
            return s;
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
