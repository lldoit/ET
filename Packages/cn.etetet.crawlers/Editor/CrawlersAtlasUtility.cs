using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using YIUIFramework;

namespace ET.Editor
{
    public static class CrawlersAtlasUtility
    {
        private const string AtlasDirectory = "Packages/cn.etetet.crawlers/Assets/GameRes/YIUI/Crawlers/Atlas";
        private const string AtlasPath = AtlasDirectory + "/Atlas_Crawlers_Atlas1.spriteatlasv2";
        private const string Atlas1Directory = "Packages/cn.etetet.crawlers/Assets/GameRes/YIUI/Crawlers/Sprites/Atlas1";
        private const string AtlasDataPath = "Assets/GameRes/YIUI/YIUISettings/YIUIAtlasData.asset";

        [MenuItem("ET/Crawlers/Refresh Crawlers Atlas")]
        public static void RefreshCrawlersAtlas()
        {
            Directory.CreateDirectory(AtlasDirectory);
            NormalizeAtlasSpriteImporters();
            if (!RecreateAtlasAsset())
            {
                return;
            }

            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceUpdate);
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);
            atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            RefreshAtlasData(atlas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static bool RecreateAtlasAsset()
        {
            if (File.Exists(AtlasPath))
            {
                AssetDatabase.DeleteAsset(AtlasPath);
                AssetDatabase.Refresh();
            }

            UnityEngine.Object atlas1 = AssetDatabase.LoadMainAssetAtPath(Atlas1Directory);
            if (atlas1 == null)
            {
                Debug.LogError($"Crawlers atlas missing packable: {Atlas1Directory}");
                return false;
            }

            var atlasAsset = new SpriteAtlasAsset();
            atlasAsset.Add(new[] { atlas1 });
            SpriteAtlasAsset.Save(atlasAsset, AtlasPath);
            AssetDatabase.Refresh();
            return SpriteAtlasAsset.Load(AtlasPath) != null;
        }

        private static void NormalizeAtlasSpriteImporters()
        {
            string[] spritePaths = Directory.GetFiles(Atlas1Directory, "*.png", SearchOption.AllDirectories)
                .Select(path => path.Replace("\\", "/"))
                .ToArray();

            foreach (string spritePath in spritePaths)
            {
                if (AssetImporter.GetAtPath(spritePath) is not TextureImporter importer)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
                defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(defaultSettings);
                AssetDatabase.WriteImportSettingsIfDirty(spritePath);
            }
        }

        private static void RefreshAtlasData(SpriteAtlas crawlersAtlas)
        {
            if (crawlersAtlas == null)
            {
                Debug.LogError($"Crawlers atlas missing after import: {AtlasPath}");
                return;
            }

            YIUIAtlasData atlasData = AssetDatabase.LoadAssetAtPath<YIUIAtlasData>(AtlasDataPath);
            if (atlasData == null)
            {
                atlasData = ScriptableObject.CreateInstance<YIUIAtlasData>();
                AssetDatabase.CreateAsset(atlasData, AtlasDataPath);
            }

            List<YIUIAtlasInfo> infos = atlasData.Infos != null
                ? atlasData.Infos.Where(info => info != null && info.AtlasName != crawlersAtlas.name).ToList()
                : new List<YIUIAtlasInfo>();

            string[] spriteNames = GetSpriteNames(crawlersAtlas);
            infos.Add(new YIUIAtlasInfo
            {
                AtlasName = crawlersAtlas.name,
                SpriteNames = spriteNames,
            });

            atlasData.Infos = infos.OrderBy(info => info.AtlasName).ToArray();
            EditorUtility.SetDirty(atlasData);
        }

        private static string[] GetSpriteNames(SpriteAtlas atlas)
        {
            Sprite[] sprites = new Sprite[atlas.spriteCount];
            int count = atlas.GetSprites(sprites);
            return sprites
                .Take(count)
                .Where(sprite => sprite != null)
                .Select(sprite => sprite.name.Replace("(Clone)", string.Empty))
                .Distinct()
                .OrderBy(name => name)
                .ToArray();
        }
    }
}
