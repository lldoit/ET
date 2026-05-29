using UnityEditor;
using UnityEngine;

namespace ET
{
    public class UGFBuildHotfixWindow : EditorWindow
    {
        public static void Open()
        {
            UGFAppBuildEditor.Open();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("UGF Build / Hotfix", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            if (GUILayout.Button("Compile Hotfix", GUILayout.Height(28)))
            {
                AssemblyTool.DoCompile();
            }

            if (GUILayout.Button("Open YooAsset Builder", GUILayout.Height(28)))
            {
                EditorApplication.ExecuteMenuItem("YooAsset/AssetBundle Builder");
            }

            if (GUILayout.Button("Open Build Tool", GUILayout.Height(28)))
            {
                UGFAppBuildEditor.Open();
            }
        }
    }
}
