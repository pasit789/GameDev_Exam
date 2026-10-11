using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pasit.Editor
{
    [InitializeOnLoad]
    public static class SetupCharacterMaterials
    {
        [MenuItem("Tools/Pasit/Setup Character Materials")]
        public static void ApplyCharacterMaterials()
        {
            Debug.Log("[SetupCharacterMaterials] Creating and applying character materials...");

            string folder = "Assets/Materials/Character";
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Character");
            }

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                litShader = Shader.Find("Standard");
            }

            // 1. Create / Get Materials
            Material matBody = GetOrCreateMaterial($"{folder}/Mat_Ch02_Body.mat", litShader, new Color(0.92f, 0.76f, 0.68f, 1f), 0.35f, 0.0f);
            Material matCloth = GetOrCreateMaterial($"{folder}/Mat_Ch02_Cloth.mat", litShader, new Color(0.12f, 0.45f, 0.88f, 1f), 0.25f, 0.05f); // Vibrant royal blue sportswear
            Material matHair = GetOrCreateMaterial($"{folder}/Mat_Ch02_Hair.mat", litShader, new Color(0.24f, 0.15f, 0.12f, 1f), 0.45f, 0.1f); // Dark chestnut hair
            Material matSneakers = GetOrCreateMaterial($"{folder}/Mat_Ch02_Sneakers.mat", litShader, new Color(0.92f, 0.94f, 0.96f, 1f), 0.55f, 0.1f); // Clean light silver/white sneakers
            Material matSocks = GetOrCreateMaterial($"{folder}/Mat_Ch02_Socks.mat", litShader, new Color(1.0f, 1.0f, 1.0f, 1f), 0.15f, 0.0f); // White socks
            Material matEyelashes = GetOrCreateMaterial($"{folder}/Mat_Ch02_Eyelashes.mat", litShader, new Color(0.1f, 0.1f, 0.1f, 1f), 0.2f, 0.0f); // Charcoal black eyelashes

            AssetDatabase.SaveAssets();

            Dictionary<string, Material> matMap = new Dictionary<string, Material>
            {
                { "body", matBody },
                { "cloth", matCloth },
                { "hair", matHair },
                { "sneakers", matSneakers },
                { "socks", matSocks },
                { "eyelashes", matEyelashes }
            };

            // 2. Apply to HumanoidPlayer Prefab
            string prefabPath = "Assets/Prefabs/HumanoidPlayer.prefab";
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot != null)
            {
                ApplyMaterialsToHierarchy(prefabRoot, matMap);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                Debug.Log("[SetupCharacterMaterials] Successfully updated materials on HumanoidPlayer.prefab!");
            }

            // 3. Apply to Active Scene or Stage2.unity
            string stage2ScenePath = "Assets/Scenes/Stage2.unity";
            var currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.path != stage2ScenePath)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(stage2ScenePath, OpenSceneMode.Single);
                }
            }

            GameObject scenePlayer = GameObject.Find("HumanoidPlayer");
            if (scenePlayer != null)
            {
                ApplyMaterialsToHierarchy(scenePlayer, matMap);
                EditorUtility.SetDirty(scenePlayer);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("[SetupCharacterMaterials] Successfully applied materials to HumanoidPlayer in Stage2 scene!");
            }

            AssetDatabase.Refresh();
            Debug.Log("[SetupCharacterMaterials] Character materials setup complete!");
        }

        private static void ApplyMaterialsToHierarchy(GameObject root, Dictionary<string, Material> matMap)
        {
            var smrs = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                string lowerName = smr.gameObject.name.ToLower();
                Material targetMat = null;

                if (lowerName.Contains("body")) targetMat = matMap["body"];
                else if (lowerName.Contains("cloth")) targetMat = matMap["cloth"];
                else if (lowerName.Contains("hair")) targetMat = matMap["hair"];
                else if (lowerName.Contains("sneaker")) targetMat = matMap["sneakers"];
                else if (lowerName.Contains("sock")) targetMat = matMap["socks"];
                else if (lowerName.Contains("eyelash")) targetMat = matMap["eyelashes"];

                if (targetMat != null)
                {
                    smr.sharedMaterial = targetMat;
                    smr.sharedMaterials = new Material[] { targetMat };
                    EditorUtility.SetDirty(smr);
                    Debug.Log($"[SetupCharacterMaterials] Assigned {targetMat.name} to renderer '{smr.gameObject.name}'");
                }
            }
        }

        private static Material GetOrCreateMaterial(string path, Shader shader, Color color, float smoothness, float metallic)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
