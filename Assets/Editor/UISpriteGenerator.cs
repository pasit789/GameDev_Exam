using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pasit.Editor
{
    public static class UISpriteGenerator
    {
        private const string FOLDER_PATH = "Assets/UI/Sprites";

        [MenuItem("Tools/Pasit/Generate Modern UI Sprites")]
        public static void GenerateSprites()
        {
            if (!AssetDatabase.IsValidFolder("Assets/UI"))
            {
                AssetDatabase.CreateFolder("Assets", "UI");
            }
            if (!AssetDatabase.IsValidFolder(FOLDER_PATH))
            {
                AssetDatabase.CreateFolder("Assets/UI", "Sprites");
            }

            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_GlassCard.png", 128, 128, 28, new Color(0.06f, 0.08f, 0.12f, 0.90f), new Color(0.25f, 0.40f, 0.65f, 0.60f), 2.5f, 32);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_GlassCard_Green.png", 128, 128, 28, new Color(0.04f, 0.10f, 0.07f, 0.90f), new Color(0.15f, 0.70f, 0.40f, 0.65f), 2.5f, 32);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_GlassCard_Red.png", 128, 128, 28, new Color(0.10f, 0.05f, 0.05f, 0.90f), new Color(0.80f, 0.25f, 0.25f, 0.65f), 2.5f, 32);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_RowCard.png", 128, 128, 20, new Color(0.10f, 0.13f, 0.18f, 0.75f), new Color(0.20f, 0.28f, 0.40f, 0.40f), 1.5f, 24);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_Button.png", 128, 128, 22, new Color(0.12f, 0.20f, 0.35f, 0.95f), new Color(0.30f, 0.55f, 0.95f, 0.80f), 2.0f, 26);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_Button_Hover.png", 128, 128, 22, new Color(0.16f, 0.30f, 0.55f, 0.98f), new Color(0.40f, 0.75f, 1.0f, 1.0f), 2.5f, 26);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_Button_Back.png", 128, 128, 22, new Color(0.09f, 0.13f, 0.20f, 0.95f), new Color(0.25f, 0.35f, 0.50f, 0.70f), 2.0f, 26);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_Button_Green.png", 128, 128, 22, new Color(0.08f, 0.35f, 0.18f, 0.95f), new Color(0.20f, 0.85f, 0.45f, 0.80f), 2.0f, 26);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_Button_Red.png", 128, 128, 22, new Color(0.35f, 0.10f, 0.10f, 0.95f), new Color(0.90f, 0.30f, 0.30f, 0.80f), 2.0f, 26);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_HUD_Panel.png", 128, 128, 18, new Color(0.06f, 0.08f, 0.12f, 0.85f), new Color(0.25f, 0.45f, 0.75f, 0.50f), 1.5f, 24);
            CreateRoundedCardSprite($"{FOLDER_PATH}/UI_Badge.png", 64, 32, 14, new Color(0.08f, 0.14f, 0.24f, 0.95f), new Color(0.0f, 0.75f, 1.0f, 0.50f), 1.5f, 16);
            CreateSliderTrackSprite($"{FOLDER_PATH}/UI_SliderTrack.png", 64, 32, 14, new Color(0.05f, 0.07f, 0.10f, 0.95f), new Color(0.18f, 0.24f, 0.35f, 0.80f), 1.5f, 16);
            CreateSliderFillSprite($"{FOLDER_PATH}/UI_SliderFill.png", 64, 32, 14, new Color(0.0f, 0.75f, 1.0f, 1.0f), new Color(0.0f, 0.45f, 0.90f, 1.0f), 16);
            CreateCircleKnobSprite($"{FOLDER_PATH}/UI_SliderKnob.png", 64, 64, 26, Color.white, new Color(0.0f, 0.80f, 1.0f, 1.0f), 3.5f);
            CreateGlowDividerSprite($"{FOLDER_PATH}/UI_Divider.png", 256, 8, new Color(0.0f, 0.75f, 1.0f, 0.85f));
            CreateGlowDividerSprite($"{FOLDER_PATH}/UI_Divider_Green.png", 256, 8, new Color(0.15f, 0.85f, 0.45f, 0.85f));
            CreateGlowDividerSprite($"{FOLDER_PATH}/UI_Divider_Red.png", 256, 8, new Color(0.95f, 0.30f, 0.30f, 0.85f));

            AssetDatabase.Refresh();
            Debug.Log("[UISpriteGenerator] All Modern UI Sprites generated successfully in " + FOLDER_PATH);
        }

        private static void CreateRoundedCardSprite(string path, int width, int height, float radius, Color fillColor, Color borderColor, float borderWidth, int borderPadding)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float cx = Mathf.Clamp(x, radius, width - 1 - radius);
                    float cy = Mathf.Clamp(y, radius, height - 1 - radius);
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));

                    if (dist > radius)
                    {
                        float alpha = Mathf.Clamp01(1.0f - (dist - radius));
                        tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                    }
                    else
                    {
                        float edgeDist = radius - dist;
                        if (edgeDist < borderWidth)
                        {
                            float t = edgeDist / borderWidth;
                            Color c = Color.Lerp(borderColor, fillColor, t);
                            tex.SetPixel(x, y, c);
                        }
                        else
                        {
                            // subtle vertical gradient
                            float vGrad = (float)y / height * 0.10f;
                            Color c = new Color(
                                Mathf.Clamp01(fillColor.r + vGrad),
                                Mathf.Clamp01(fillColor.g + vGrad),
                                Mathf.Clamp01(fillColor.b + vGrad),
                                fillColor.a
                            );
                            tex.SetPixel(x, y, c);
                        }
                    }
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);

            SetupImporter(path, new Vector4(borderPadding, borderPadding, borderPadding, borderPadding));
        }

        private static void CreateSliderTrackSprite(string path, int width, int height, float radius, Color fillColor, Color borderColor, float borderWidth, int borderPadding)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float cx = Mathf.Clamp(x, radius, width - 1 - radius);
                    float cy = Mathf.Clamp(y, radius, height - 1 - radius);
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));

                    if (dist > radius)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float edgeDist = radius - dist;
                        if (edgeDist < borderWidth)
                        {
                            tex.SetPixel(x, y, borderColor);
                        }
                        else
                        {
                            tex.SetPixel(x, y, fillColor);
                        }
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            SetupImporter(path, new Vector4(borderPadding, borderPadding, borderPadding, borderPadding));
        }

        private static void CreateSliderFillSprite(string path, int width, int height, float radius, Color startColor, Color endColor, int borderPadding)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float cx = Mathf.Clamp(x, radius, width - 1 - radius);
                    float cy = Mathf.Clamp(y, radius, height - 1 - radius);
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));

                    if (dist > radius)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float t = (float)x / width;
                        Color c = Color.Lerp(startColor, endColor, t);
                        // Subtle inner gloss highlight at top
                        if (y > height * 0.65f)
                        {
                            c = Color.Lerp(c, Color.white, 0.25f);
                        }
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            SetupImporter(path, new Vector4(borderPadding, borderPadding, borderPadding, borderPadding));
        }

        private static void CreateCircleKnobSprite(string path, int width, int height, float radius, Color coreColor, Color glowColor, float glowWidth)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2((width - 1) / 2f, (height - 1) / 2f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist > radius)
                    {
                        float alpha = Mathf.Clamp01(1.0f - (dist - radius));
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float edgeDist = radius - dist;
                        if (edgeDist < glowWidth)
                        {
                            float t = edgeDist / glowWidth;
                            tex.SetPixel(x, y, Color.Lerp(glowColor, coreColor, t));
                        }
                        else
                        {
                            // Inner circle
                            tex.SetPixel(x, y, coreColor);
                        }
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            SetupImporter(path, Vector4.zero);
        }

        private static void CreateGlowDividerSprite(string path, int width, int height, Color glowColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float midX = (width - 1) / 2f;
            float midY = (height - 1) / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 1.0f - Mathf.Abs(x - midX) / midX;
                    float dy = 1.0f - Mathf.Abs(y - midY) / midY;
                    dx = Mathf.Pow(Mathf.Clamp01(dx), 1.5f);
                    dy = Mathf.Clamp01(dy);

                    Color c = glowColor;
                    c.a *= (dx * dy);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            SetupImporter(path, Vector4.zero);
        }

        private static void SetupImporter(string path, Vector4 border)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = border;
                importer.filterMode = FilterMode.Bilinear;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }
}
