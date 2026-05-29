using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace ET
{
    public class UGFAppBuildEditor : EditorWindow
    {
        private const string GlobalConfigPath = "Packages/com.etetet.init/Resources/GlobalConfig.asset";

        private Vector2 scrollPosition;

        [MenuItem("ET/Loader/UGF App Builder", false, ETMenuItemPriority.BuildTool)]
        public static void Open()
        {
            UGFAppBuildEditor window = GetWindow<UGFAppBuildEditor>("UGF App Builder", true);
            window.minSize = new Vector2(720f, 640f);
            window.Show();
        }

        private void OnGUI()
        {
            UGFAppBuildSettings settings = UGFAppBuildSettings.instance;
            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawEnvironment();
            DrawYooAssetSettings(settings);
            DrawStepSettings(settings);
            DrawPlayerSettings(settings);
            DrawActions(settings);

            EditorGUILayout.EndScrollView();
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawEnvironment()
        {
            EditorGUILayout.LabelField("Environment", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Unity Version", Application.unityVersion);
            EditorGUILayout.LabelField("Active Build Target", EditorUserBuildSettings.activeBuildTarget.ToString());
            EditorGUILayout.LabelField("Selected Build Target Group", EditorUserBuildSettings.selectedBuildTargetGroup.ToString());

            GlobalConfig globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>(GlobalConfigPath);
            EditorGUILayout.LabelField("Code Mode", globalConfig == null ? "Missing GlobalConfig" : globalConfig.CodeMode.ToString());
            EditorGUILayout.EndVertical();
        }

        private static void DrawYooAssetSettings(UGFAppBuildSettings settings)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("YooAsset", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            DrawPackagePopup(settings);
            settings.PackageVersion = EditorGUILayout.TextField("Package Version", settings.PackageVersion);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);
            if (GUILayout.Button("Use Current Time", GUILayout.Width(140f)))
            {
                settings.PackageVersion = DateTime.Now.ToString("yyyy-MM-dd-HHmm");
            }
            EditorGUILayout.LabelField($"Actual: {UGFAppBuildPipeline.GetPackageVersion(settings)}");
            EditorGUILayout.EndHorizontal();

            settings.BuildPipeline = DrawSupportedPipelinePopup(settings.BuildPipeline);
            settings.CompressOption = (ECompressOption)EditorGUILayout.EnumPopup("Compression", settings.CompressOption);
            settings.FileNameStyle = (EFileNameStyle)EditorGUILayout.EnumPopup("File Name Style", settings.FileNameStyle);
            settings.BuildinFileCopyOption = (EBuildinFileCopyOption)EditorGUILayout.EnumPopup("Buildin Copy", settings.BuildinFileCopyOption);
            settings.ClearBuildCacheFiles = EditorGUILayout.Toggle("Clear Build Cache", settings.ClearBuildCacheFiles);
            settings.UseAssetDependencyDB = EditorGUILayout.Toggle("Use Asset Dependency DB", settings.UseAssetDependencyDB);

            EditorGUILayout.HelpBox("第一版只支持 BuiltinBuildPipeline 和 ScriptableBuildPipeline，并使用当前 active build target。", MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        private static void DrawStepSettings(UGFAppBuildSettings settings)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Build Steps", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            settings.BuildHotfixDll = EditorGUILayout.ToggleLeft("Build Hotfix DLL", settings.BuildHotfixDll);
            settings.CopyAotDlls = EditorGUILayout.ToggleLeft("Copy AOT DLLs", settings.CopyAotDlls);
            settings.BuildYooAssets = EditorGUILayout.ToggleLeft("Build YooAsset", settings.BuildYooAssets);
            settings.BuildPlayer = EditorGUILayout.ToggleLeft("Build Player", settings.BuildPlayer);
            settings.SwitchToClientCodeMode = EditorGUILayout.ToggleLeft("Switch GlobalConfig.CodeMode to Client before Player Build", settings.SwitchToClientCodeMode);
            settings.RevealOutput = EditorGUILayout.ToggleLeft("Reveal Player Output", settings.RevealOutput);
            EditorGUILayout.EndVertical();
        }

        private static void DrawPlayerSettings(UGFAppBuildSettings settings)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Player Build", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            PlayerSettings.bundleVersion = EditorGUILayout.TextField("Version", PlayerSettings.bundleVersion);
            settings.PlayerBuildDir = EditorGUILayout.TextField("Output Directory", settings.PlayerBuildDir);
            settings.PlayerBuildOptions = (BuildOptions)EditorGUILayout.EnumFlagsField("Build Options", settings.PlayerBuildOptions);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);
            if (GUILayout.Button("Select Output", GUILayout.Width(140f)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Player Build Output", settings.PlayerBuildDir, string.Empty);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    settings.PlayerBuildDir = ToProjectRelativePath(path);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private static void DrawActions(UGFAppBuildSettings settings)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Save", GUILayout.Height(34f)))
            {
                SaveSettings(settings);
            }
            if (GUILayout.Button("Build Hotfix DLL", GUILayout.Height(34f)))
            {
                RunStep("Build Hotfix DLL", UGFAppBuildPipeline.BuildHotfixDlls);
            }
            if (GUILayout.Button("Build YooAsset", GUILayout.Height(34f)))
            {
                RunStep("Build YooAsset", () => UGFAppBuildPipeline.BuildYooAssets(settings));
            }
            if (GUILayout.Button("Build Player", GUILayout.Height(34f)))
            {
                RunStep("Build Player", () => UGFAppBuildPipeline.BuildPlayer(settings));
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Build All", GUILayout.Height(42f)))
            {
                if (EditorUtility.DisplayDialog("UGF App Builder", "Start Build All with current settings?", "Build", "Cancel"))
                {
                    RunStep("Build All", () => UGFAppBuildPipeline.BuildAll(settings));
                }
            }
        }

        private static void DrawPackagePopup(UGFAppBuildSettings settings)
        {
            List<string> packageNames = AssetBundleCollectorSettingData.Setting.Packages
                .Select(package => package.PackageName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (packageNames.Count == 0)
            {
                settings.PackageName = EditorGUILayout.TextField("Package", settings.PackageName);
                EditorGUILayout.HelpBox("Not found any YooAsset package.", MessageType.Warning);
                return;
            }

            int selectedIndex = packageNames.IndexOf(settings.PackageName);
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
                settings.PackageName = packageNames[0];
            }

            selectedIndex = EditorGUILayout.Popup("Package", selectedIndex, packageNames.ToArray());
            settings.PackageName = packageNames[selectedIndex];
        }

        private static EBuildPipeline DrawSupportedPipelinePopup(EBuildPipeline current)
        {
            EBuildPipeline[] supported =
            {
                EBuildPipeline.BuiltinBuildPipeline,
                EBuildPipeline.ScriptableBuildPipeline
            };
            string[] labels = supported.Select(value => value.ToString()).ToArray();
            int selectedIndex = Array.IndexOf(supported, current);
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            selectedIndex = EditorGUILayout.Popup("Build Pipeline", selectedIndex, labels);
            return supported[selectedIndex];
        }

        private static void RunStep(string title, Action action)
        {
            try
            {
                SaveSettings(UGFAppBuildSettings.instance);
                EditorUtility.DisplayProgressBar("UGF App Builder", title, 0.1f);
                action.Invoke();
                EditorUtility.DisplayDialog("UGF App Builder", $"{title} success.", "OK");
            }
            catch (ExitGUIException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("UGF App Builder", $"{title} failed:\n{exception.Message}", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void SaveSettings(UGFAppBuildSettings settings)
        {
            settings.Save();
            AssetDatabase.SaveAssets();
            Debug.Log("[UGF App Builder] Settings saved.");
        }

        private static string ToProjectRelativePath(string fullPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string normalizedPath = fullPath.Replace('\\', '/');
            string normalizedRoot = projectRoot.Replace('\\', '/');
            if (normalizedPath.StartsWith(normalizedRoot, StringComparison.Ordinal))
            {
                return normalizedPath.Substring(normalizedRoot.Length).TrimStart('/');
            }

            return fullPath;
        }
    }
}
