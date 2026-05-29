using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    internal class UGFStripLinkConfigEditor : UGFHybridCLRConfigEditor
    {
        [MenuItem("ET/Loader/Tools/打包/代码裁剪配置", false, ETMenuItemPriority.BuildTool + 10)]
        public static void Open()
        {
            Open<UGFStripLinkConfigEditor>("代码裁剪配置", UGFHybridCLRConfigMode.StripLink);
        }
    }

    internal class UGFAotDllsConfigEditor : UGFHybridCLRConfigEditor
    {
        [MenuItem("ET/Loader/Tools/热更/AOT泛型补充配置", false, ETMenuItemPriority.BuildTool + 11)]
        public static void Open()
        {
            Open<UGFAotDllsConfigEditor>("AOT泛型补充配置", UGFHybridCLRConfigMode.AotMetadata);
        }
    }

    internal abstract class UGFHybridCLRConfigEditor : EditorWindow
    {
        private readonly List<ItemData> items = new();
        private UGFHybridCLRConfigMode mode;
        private Vector2 scrollPosition;

        protected static void Open<TWindow>(string title, UGFHybridCLRConfigMode mode)
            where TWindow : UGFHybridCLRConfigEditor
        {
            TWindow window = GetWindow<TWindow>(title, true);
            window.minSize = new Vector2(560f, 640f);
            window.mode = mode;
            window.RefreshListData();
            window.Show();
        }

        private void OnEnable()
        {
            RefreshListData();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawList();
            DrawActions();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField(titleContent.text, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(GetHelpText(), MessageType.Info);

            string dir = UGFHybridCLRConfigTool.GetStrippedAotDllDirectory();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Build Target", EditorUserBuildSettings.activeBuildTarget.ToString());
            EditorGUILayout.LabelField("AOT DLL Directory", dir);
            if (!Directory.Exists(dir))
            {
                EditorGUILayout.HelpBox("未找到裁剪后的 AOT DLL，请先执行 HybridCLR AOT 生成流程。", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawList()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (ItemData item in items)
            {
                item.IsOn = EditorGUILayout.ToggleLeft(item.Name, item.IsOn);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选", GUILayout.Height(30f)))
            {
                SetAll(true);
            }
            if (GUILayout.Button("全不选", GUILayout.Height(30f)))
            {
                SetAll(false);
            }
            if (GUILayout.Button("刷新列表", GUILayout.Height(30f)))
            {
                RefreshListData();
            }
            if (GUILayout.Button("保存", GUILayout.Height(30f)))
            {
                Save();
            }
            EditorGUILayout.EndHorizontal();
        }

        private string GetHelpText()
        {
            if (mode == UGFHybridCLRConfigMode.AotMetadata)
            {
                return "勾选需要作为 HybridCLR AOT 泛型补充元数据的 DLL，保存后写入 HybridCLRSettings.patchAOTAssemblies，并复制到 Bundles/AotDlls。";
            }

            return "勾选需要写入 Assets/link.xml 的程序集，保存后添加 preserve=\"all\"，用于降低 IL2CPP/linker 裁剪风险。";
        }

        private void RefreshListData()
        {
            items.Clear();
            string[] selected = GetSelected();
            foreach (string name in UGFHybridCLRConfigTool.GetProjectAssemblyNames(mode))
            {
                items.Add(new ItemData(name, selected.Contains(name)));
            }
        }

        private string[] GetSelected()
        {
            return mode == UGFHybridCLRConfigMode.AotMetadata
                ? UGFHybridCLRConfigTool.GetSelectedAotDlls()
                : UGFHybridCLRConfigTool.GetSelectedLinkAssemblies();
        }

        private void SetAll(bool selected)
        {
            foreach (ItemData item in items)
            {
                item.IsOn = selected;
            }
        }

        private void Save()
        {
            string[] selected = items.Where(item => item.IsOn).Select(item => item.Name).ToArray();
            bool success = mode == UGFHybridCLRConfigMode.AotMetadata
                ? UGFHybridCLRConfigTool.SaveAotDlls(selected)
                : UGFHybridCLRConfigTool.SaveLinkAssemblies(selected);

            if (success)
            {
                EditorUtility.DisplayDialog(titleContent.text, "保存成功。", "OK");
            }
        }

        private sealed class ItemData
        {
            public ItemData(string name, bool isOn)
            {
                Name = name;
                IsOn = isOn;
            }

            public string Name { get; }
            public bool IsOn { get; set; }
        }
    }
}
