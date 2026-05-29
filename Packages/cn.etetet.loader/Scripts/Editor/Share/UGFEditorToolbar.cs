#if UNITY_2021_1_OR_NEWER
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace ET
{
    [InitializeOnLoad]
    public static class UGFEditorToolbar
    {
        private const string StateSyncInitScene = "Packages/cn.etetet.statesync/Scenes/Init.unity";
        private const string LeftContainerName = "UGFEditorToolbarLeft";
        private const string RightContainerName = "UGFEditorToolbarRight";
        private const float LeftContainerWidth = 285f;
        private const float RightContainerWidth = 330f;

        private static GUIContent switchSceneContent;
        private static GUIContent buildContent;
        private static GUIContent appConfigsContent;
        private static GUIContent toolsContent;
        private static GUIContent openCSharpProjectContent;

        private static bool injected;

        static UGFEditorToolbar()
        {
            CreateContents();
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.update -= InstallToolbarOverlay;
            EditorApplication.update += InstallToolbarOverlay;
        }

        private static void CreateContents()
        {
            GUIContent platformIcon = GetBuildTargetIcon();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            switchSceneContent = EditorGUIUtility.TrTextContentWithIcon(string.IsNullOrEmpty(sceneName) ? "Switch Scene" : sceneName, "切换场景", "UnityLogo");
            buildContent = EditorGUIUtility.TrTextContentWithIcon("Build App/Hotfix", "打新包/打热更", platformIcon?.image);
            appConfigsContent = EditorGUIUtility.TrTextContentWithIcon("App Configs", "打开 UGF 运行配置", "Settings");
            toolsContent = EditorGUIUtility.TrTextContentWithIcon("Tools", "工具箱", "CustomTool");
            openCSharpProjectContent = EditorGUIUtility.TrTextContentWithIcon("Open C# Project", "打开 C# 工程", "dll Script Icon");
        }

        private static GUIContent GetBuildTargetIcon()
        {
            MethodInfo getIconMethod = typeof(Editor).Assembly
                    .GetType("UnityEditor.Networking.PlayerConnection.ConnectionUIHelper")
                    ?.GetMethod("GetIcon", BindingFlags.Static | BindingFlags.Public);

            return getIconMethod?.Invoke(null, new object[] { EditorUserBuildSettings.activeBuildTarget.ToString() }) as GUIContent;
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            if (switchSceneContent != null)
            {
                switchSceneContent.text = string.IsNullOrEmpty(scene.name) ? "Switch Scene" : scene.name;
            }
        }

        private static void InstallToolbarOverlay()
        {
            if (injected)
            {
                EditorApplication.update -= InstallToolbarOverlay;
                return;
            }

            VisualElement root = GetToolbarRoot();
            if (root == null)
            {
                return;
            }

            VisualElement leftZone = root.Q("ToolbarZoneLeftAlign");
            VisualElement rightZone = root.Q("ToolbarZoneRightAlign");
            if (leftZone == null || rightZone == null)
            {
                return;
            }

            FindByName(leftZone, LeftContainerName)?.RemoveFromHierarchy();
            FindByName(rightZone, RightContainerName)?.RemoveFromHierarchy();

            leftZone.Add(CreateToolbarContainer(LeftContainerName, DrawLeftToolbarGUI, LeftContainerWidth));
            rightZone.Add(CreateToolbarContainer(RightContainerName, DrawRightToolbarGUI, RightContainerWidth));
            injected = true;
            EditorApplication.update -= InstallToolbarOverlay;
        }

        private static VisualElement GetToolbarRoot()
        {
            System.Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null)
            {
                return null;
            }

            foreach (UnityEngine.Object toolbar in Resources.FindObjectsOfTypeAll(toolbarType))
            {
                FieldInfo rootField = toolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rootField?.GetValue(toolbar) is VisualElement root)
                {
                    return root;
                }
            }

            return null;
        }

        private static VisualElement FindByName(VisualElement root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; ++i)
            {
                VisualElement result = FindByName(root[i], name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static VisualElement CreateToolbarContainer(string name, System.Action guiHandler, float width)
        {
            IMGUIContainer container = new IMGUIContainer(guiHandler) { name = name };
            container.pickingMode = PickingMode.Position;
            container.style.flexShrink = 0;
            container.style.width = width;
            container.style.height = 28;
            container.style.marginLeft = 4;
            container.style.marginRight = 4;
            return container;
        }

        private static void DrawLeftToolbarGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (EditorGUILayout.DropdownButton(switchSceneContent, FocusType.Passive, EditorStyles.toolbarPopup, GUILayout.MaxWidth(150)))
            {
                DrawSwitchSceneDropdownMenu();
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button(buildContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(125)))
            {
                UGFAppBuildEditor.Open();
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawRightToolbarGUI()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(appConfigsContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(100)))
            {
                UGFAppConfigsWindow.Open();
            }

            EditorGUILayout.Space(10);
            if (EditorGUILayout.DropdownButton(toolsContent, FocusType.Passive, EditorStyles.toolbarPopup, GUILayout.MaxWidth(90)))
            {
                ShowToolsMenu();
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button(openCSharpProjectContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(120)))
            {
                OpenCSharpProject();
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private static void DrawSwitchSceneDropdownMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.allowDuplicateNames = true;
            FillSwitchSceneMenu(menu);
            menu.ShowAsContext();
        }

        private static void ShowToolsMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("打包/代码裁剪配置"), false, UGFStripLinkConfigEditor.Open);
            menu.AddItem(new GUIContent("热更/AOT泛型补充配置"), false, UGFAotDllsConfigEditor.Open);
            menu.ShowAsContext();
        }

        private static void FillSwitchSceneMenu(GenericMenu menu)
        {
            List<string> scenes = FindSwitchableScenes();
            if (scenes.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No scenes found"));
                return;
            }

            foreach (string scenePath in scenes)
            {
                bool isCurrent = scenePath == EditorSceneManager.GetActiveScene().path;
                string displayName = GetSceneDisplayName(scenePath);
                menu.AddItem(new GUIContent(displayName), isCurrent, OpenScene, scenePath);
            }
        }

        private static List<string> FindSwitchableScenes()
        {
            List<string> scenes = new List<string>
            {
                StateSyncInitScene
            };

            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Packages" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    scenes.Add(path);
                }
            }

            scenes.RemoveAll(path => !File.Exists(path) || IsIgnoredScene(path));
            scenes.Sort((x, y) => string.Compare(GetSceneDisplayName(x), GetSceneDisplayName(y), System.StringComparison.OrdinalIgnoreCase));

            for (int i = scenes.Count - 1; i > 0; --i)
            {
                if (scenes.IndexOf(scenes[i]) != i)
                {
                    scenes.RemoveAt(i);
                }
            }

            return scenes;
        }

        private static bool IsIgnoredScene(string path)
        {
            return path.Contains("/Demo/") || path.Contains("/Tests/") || path.Contains("/Samples~/") || path.Contains("/Editor/");
        }

        private static string GetSceneDisplayName(string scenePath)
        {
            const string packagePrefix = "Packages/cn.etetet.";
            if (scenePath.StartsWith(packagePrefix))
            {
                string rest = scenePath.Substring(packagePrefix.Length);
                string packageName = rest.Split('/')[0];
                return $"{packageName}/{Path.GetFileNameWithoutExtension(scenePath)}";
            }

            return Path.GetFileNameWithoutExtension(scenePath);
        }

        private static void OpenScene(object userData)
        {
            string scenePath = userData as string;
            if (string.IsNullOrEmpty(scenePath))
            {
                return;
            }

            UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isDirty)
            {
                int option = EditorUtility.DisplayDialogComplex("警告", $"当前场景 {activeScene.name} 未保存，是否保存？", "保存", "取消", "不保存");
                if (option == 0 && !EditorSceneManager.SaveOpenScenes())
                {
                    return;
                }

                if (option == 1)
                {
                    return;
                }
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        private static void OpenCSharpProject()
        {
            AssetDatabase.Refresh();
            CodeEditor.CurrentEditor.SyncAll();
            CodeEditor.CurrentEditor.OpenProject();
        }

    }
}
#endif
