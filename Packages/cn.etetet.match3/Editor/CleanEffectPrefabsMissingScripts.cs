using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class CleanEffectPrefabsMissingScripts
    {
        [MenuItem("ET/Match3/清理特效Prefab的Missing Scripts")]
        public static void CleanAllEffectPrefabs()
        {
            string effectPath = "Packages/cn.etetet.match3/GameRes/Match3/Effect";
            
            // 获取所有prefab的GUID
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { effectPath });
            int cleanedCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                
                if (prefab == null) continue;

                // 使用GameObjectUtility清除Missing Scripts
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
                
                if (count > 0)
                {
                    cleanedCount++;
                    Debug.Log($"从 {prefab.name} 移除了 {count} 个Missing Script(s)");
                    EditorUtility.SetDirty(prefab);
                }
            }

            if (cleanedCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"<color=green>清理完成！共清理了 {cleanedCount} 个Prefab</color>");
            }
            else
            {
                Debug.Log("没有发现需要清理的Missing Scripts");
            }
        }
    }
}
