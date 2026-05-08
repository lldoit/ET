#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework.Editor
{
    public static partial class YiuiPrefabToolService
    {
        private const string DefaultYiuiRoot = "Packages/cn.etetet.yiuistatesync/Assets/GameRes/YIUI";
        private const string ButtonTemplatePath = "Packages/cn.etetet.yiuiframework/Editor/TemplatePrefabs/YIUI/YIUIButton.prefab";
        private const string PreviewFolder = "Temp/YIUIMcpPreviews";

        public static YiuiMcpResult CreateYIUIPanel(string packageName, string panelName)
        {
            if (!IsSafeName(packageName)) return YiuiMcpResult.Fail("Invalid packageName.");
            if (!IsSafeName(panelName)) return YiuiMcpResult.Fail("Invalid panelName.");
            if (!panelName.EndsWith("Panel", StringComparison.Ordinal))
                return YiuiMcpResult.Fail("panelName must end with Panel.");

            var packageRoot = ResolvePackageRoot(packageName);
            var prefabFolder = $"{packageRoot}/{YIUIConstHelper.Const.UIPrefabs}";
            EnsureFolder(prefabFolder);

            var prefabPath = $"{prefabFolder}/{panelName}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                return YiuiMcpResult.Fail($"Prefab already exists: {prefabPath}");

            var panel = MenuItemYIUIPanelSource.CreateYIUIPanel();
            panel.name = panelName;
            var cdeTable = panel.GetComponent<UIBindCDETable>();
            if (cdeTable == null)
            {
                UnityEngine.Object.DestroyImmediate(panel);
                return YiuiMcpResult.Fail("Created panel is missing UIBindCDETable.");
            }

            cdeTable.PkgName = packageName;
            cdeTable.ResName = panelName;
            cdeTable.UICodeType = EUICodeType.Panel;
            SetInternalField(cdeTable, "IsSplitData", false);
            EnsureEventTable(panel, cdeTable);
            PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);
            UnityEngine.Object.DestroyImmediate(panel);
            AssetDatabase.Refresh();

            return YiuiMcpResult.Ok("YIUI panel prefab created.", new { prefabPath, packageName, panelName });
        }

        public static YiuiMcpResult AddYIUIButton(string prefabPath, string path, string objectName, string eventName, string text)
        {
            if (!ValidatePrefabPath(prefabPath, out var error)) return YiuiMcpResult.Fail(error);
            if (!IsSafeName(objectName)) return YiuiMcpResult.Fail("Invalid objectName.");

            return EditPrefab(prefabPath, root =>
            {
                var parent = FindByPath(root.transform, path);
                if (parent == null) return YiuiMcpResult.Fail($"Parent path not found: {path}");
                if (FindDirectChild(parent, objectName) != null)
                    return YiuiMcpResult.Fail($"Object already exists under parent: {objectName}");

                var template = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonTemplatePath);
                if (template == null) return YiuiMcpResult.Fail($"Button template not found: {ButtonTemplatePath}");

                var created = PrefabUtility.InstantiatePrefab(template, parent) as GameObject;
                if (created == null) return YiuiMcpResult.Fail("Failed to instantiate YIUI button template.");

                created.name = objectName;
                ConfigureRect(created.GetComponent<RectTransform>());
                SetButtonText(created, text);

                var bindResult = BindEventOnLoadedRoot(root, created, eventName);
                if (!bindResult.Success) return bindResult;

                return YiuiMcpResult.Ok("YIUI button added.", new
                {
                    prefabPath,
                    objectName,
                    eventName = NormalizeEventName(eventName)
                });
            });
        }

        public static YiuiMcpResult BindYIUIEvent(string prefabPath, string objectName, string eventName)
        {
            if (!ValidatePrefabPath(prefabPath, out var error)) return YiuiMcpResult.Fail(error);
            if (!IsSafeName(objectName)) return YiuiMcpResult.Fail("Invalid objectName.");

            return EditPrefab(prefabPath, root =>
            {
                var target = FindUniqueByName(root.transform, objectName);
                if (target == null) return YiuiMcpResult.Fail($"Object not found or duplicated: {objectName}");

                var bindResult = BindEventOnLoadedRoot(root, target.gameObject, eventName);
                if (!bindResult.Success) return bindResult;

                return YiuiMcpResult.Ok("YIUI event bound.", new
                {
                    prefabPath,
                    objectName,
                    eventName = NormalizeEventName(eventName)
                });
            });
        }

        public static YiuiMcpResult UnbindYIUIEvent(string prefabPath, string objectName)
        {
            if (!ValidatePrefabPath(prefabPath, out var error)) return YiuiMcpResult.Fail(error);
            if (!IsSafeName(objectName)) return YiuiMcpResult.Fail("Invalid objectName.");

            return EditPrefab(prefabPath, root =>
            {
                var target = FindUniqueByName(root.transform, objectName);
                if (target == null) return YiuiMcpResult.Fail($"Object not found or duplicated: {objectName}");

                var removed = 0;
                foreach (var bind in target.GetComponents<UITaskEventBindClick>())
                {
                    UnityEngine.Object.DestroyImmediate(bind, true);
                    removed++;
                }

                EditorUtility.SetDirty(target);
                return YiuiMcpResult.Ok("YIUI event unbound.", new
                {
                    prefabPath,
                    objectName,
                    removed
                });
            });
        }

        public static YiuiMcpResult GenerateYIUICode(string prefabPath)
        {
            if (!ValidatePrefabPath(prefabPath, out var error)) return YiuiMcpResult.Fail(error);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var cdeTable = prefab == null ? null : prefab.GetComponent<UIBindCDETable>();
            if (cdeTable == null) return YiuiMcpResult.Fail("Prefab root is missing UIBindCDETable.");

            UICreateModule.CreatePackages(cdeTable, true, false);
            AssetDatabase.Refresh();
            return YiuiMcpResult.Ok("YIUI code generation requested.", new { prefabPath, cdeTable.PkgName, cdeTable.ResName });
        }

        public static YiuiMcpResult OpenPrefabAndCapturePreview(string prefabPath)
        {
            if (!ValidatePrefabPath(prefabPath, out var error)) return YiuiMcpResult.Fail(error);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return YiuiMcpResult.Fail($"Prefab not found: {prefabPath}");

            AssetDatabase.OpenAsset(prefab);
            var texture = WaitForPreview(prefab);
            if (texture == null) return YiuiMcpResult.Fail("Unity did not produce a prefab preview texture.");

            Directory.CreateDirectory(PreviewFolder);
            var previewPath = $"{PreviewFolder}/{Path.GetFileNameWithoutExtension(prefabPath)}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            File.WriteAllBytes(previewPath, texture.EncodeToPNG());
            return YiuiMcpResult.Ok("Prefab preview captured.", new { prefabPath, previewPath });
        }

        private static YiuiMcpResult EditPrefab(string prefabPath, Func<GameObject, YiuiMcpResult> edit)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(prefabPath);
                var result = edit(root);
                if (!result.Success) return result;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                AssetDatabase.Refresh();
                return result;
            }
            catch (Exception ex)
            {
                return YiuiMcpResult.Fail(ex.Message);
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static YiuiMcpResult BindEventOnLoadedRoot(GameObject root, GameObject target, string eventName)
        {
            var cdeTable = root.GetComponent<UIBindCDETable>();
            if (cdeTable == null) return YiuiMcpResult.Fail("Prefab root is missing UIBindCDETable.");

            var eventTable = EnsureEventTable(root, cdeTable);
            var normalized = NormalizeEventName(eventName);
            if (string.IsNullOrEmpty(normalized)) return YiuiMcpResult.Fail("Invalid eventName.");

            var uiEvent = eventTable.FindEvent(normalized) ??
                          eventTable.EditorAddEvent(UIBindEventTable.EUITaskEventType.Async, normalized);
            if (uiEvent == null) return YiuiMcpResult.Fail($"Failed to create event: {normalized}");

            var bind = target.GetComponent<UITaskEventBindClick>() ?? target.AddComponent<UITaskEventBindClick>();
            if (!bind.EditorAddBind(eventTable, uiEvent))
                return YiuiMcpResult.Fail($"Failed to bind event: {normalized}");

            EditorUtility.SetDirty(target);
            EditorUtility.SetDirty(eventTable);
            EditorUtility.SetDirty(cdeTable);
            return YiuiMcpResult.Ok("Event bound.");
        }

        private static UIBindEventTable EnsureEventTable(GameObject root, UIBindCDETable cdeTable)
        {
            var eventTable = cdeTable.EventTable ?? root.GetComponent<UIBindEventTable>() ?? root.AddComponent<UIBindEventTable>();
            cdeTable.EventTable = eventTable;
            eventTable.hideFlags = YIUIConstHelper.Const.DisplayOldCDEInspector ? HideFlags.None : HideFlags.HideInInspector;
            return eventTable;
        }

        private static string ResolvePackageRoot(string packageName)
        {
            var preferred = $"{DefaultYiuiRoot}/{packageName}";
            EnsureFolder(preferred);
            EnsureFolder($"{preferred}/{YIUIConstHelper.Const.UIPrefabs}");
            EnsureFolder($"{preferred}/{YIUIConstHelper.Const.UISource}");
            return preferred;
        }

        private static void EnsureFolder(string path)
        {
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        private static void ConfigureRect(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchoredPosition = Vector2.zero;
            if (rect.sizeDelta == Vector2.zero) rect.sizeDelta = new Vector2(300, 100);
        }

        private static void SetButtonText(GameObject button, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                tmp.text = text;
                EditorUtility.SetDirty(tmp);
                return;
            }

            var uiText = button.GetComponentInChildren<Text>(true);
            if (uiText == null) return;
            uiText.text = text;
            EditorUtility.SetDirty(uiText);
        }

        private static void SetInternalField(object target, string fieldName, object value)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var field = target.GetType().GetField(fieldName, flags);
            field?.SetValue(target, value);
        }
    }
}
#endif
