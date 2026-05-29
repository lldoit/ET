using UnityEditor;
using UnityEngine;

namespace ET
{
    public class UGFAppConfigsWindow : EditorWindow
    {
        private const string GlobalConfigPath = "Packages/com.etetet.init/Resources/GlobalConfig.asset";
        private const string YooConfigPath = "Packages/cn.etetet.yooassets/Resources/YooConfig.asset";
        private const string YooAssetSettingsPath = "Packages/cn.etetet.yooassets/Resources/YooAssetSettings.asset";

        public static void Open()
        {
            UGFAppConfigsWindow window = GetWindow<UGFAppConfigsWindow>("App Configs");
            window.minSize = new Vector2(360, 160);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("UGF App Configs", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            DrawConfigButton("GlobalConfig", GlobalConfigPath);
            DrawConfigButton("YooConfig", YooConfigPath);
            DrawConfigButton("YooAssetSettings", YooAssetSettingsPath);
        }

        private static void DrawConfigButton(string label, string assetPath)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(assetPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (GUILayout.Button(label, GUILayout.Width(150)))
            {
                SelectConfig(assetPath);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void SelectConfig(string assetPath)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                EditorUtility.DisplayDialog("配置不存在", assetPath, "确定");
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
