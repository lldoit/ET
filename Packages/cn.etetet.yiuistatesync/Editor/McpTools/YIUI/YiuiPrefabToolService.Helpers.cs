#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    public static partial class YiuiPrefabToolService
    {
        private static bool ValidatePrefabPath(string prefabPath, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                error = "prefabPath is required.";
                return false;
            }

            if (!prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                error = "prefabPath must point to a .prefab asset.";
                return false;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return true;
            error = $"Prefab not found: {prefabPath}";
            return false;
        }

        private static string NormalizeEventName(string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName)) return null;
            var trimmed = eventName.Trim();
            return trimmed.StartsWith("u_Event", StringComparison.Ordinal) ? trimmed : $"u_Event{trimmed}";
        }

        private static bool IsSafeName(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.IndexOfAny(new[] { '/', '\\', '.', ':' }) < 0;
        }

        private static Transform FindByPath(Transform root, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return root;
            var current = root;
            foreach (var raw in path.Split('/'))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (raw == root.name && current == root) continue;
                current = FindDirectChild(current, raw);
                if (current == null) return null;
            }

            return current;
        }

        private static Transform FindDirectChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName) return child;
            }

            return null;
        }

        private static Transform FindUniqueByName(Transform root, string objectName)
        {
            var matches = new List<Transform>();
            CollectByName(root, objectName, matches);
            return matches.Count == 1 ? matches[0] : null;
        }

        private static void CollectByName(Transform root, string objectName, List<Transform> matches)
        {
            if (root.name == objectName) matches.Add(root);
            foreach (Transform child in root) CollectByName(child, objectName, matches);
        }

        private static Texture2D WaitForPreview(UnityEngine.Object asset)
        {
            for (var i = 0; i < 30; i++)
            {
                var texture = AssetPreview.GetAssetPreview(asset);
                if (texture != null) return texture;
                if (!AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID())) break;
                System.Threading.Thread.Sleep(50);
            }

            return AssetPreview.GetMiniThumbnail(asset) as Texture2D;
        }
    }
}
#endif
