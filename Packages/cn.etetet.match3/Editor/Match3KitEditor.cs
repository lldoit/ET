using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET.Match3.Editor
{
    /// <summary>
    /// Match3 Kit编辑器主窗口
    /// </summary>
    public class Match3KitEditor : EditorWindow
    {
        private readonly List<Match3EditorTab> tabs = new List<Match3EditorTab>();

        private int selectedTabIndex = -1;
        private int prevSelectedTabIndex = -1;

        /// <summary>
        /// 打开编辑器窗口
        /// </summary>
        [MenuItem("Tools/Match3 Kit/Editor", false, 0)]
        private static void Init()
        {
            var window = GetWindow(typeof(Match3KitEditor));
            window.titleContent = new GUIContent("Match3 Kit Editor");
            window.minSize = new Vector2(800, 600);
        }

        /// <summary>
        /// Unity OnEnable回调
        /// </summary>
        private void OnEnable()
        {
            tabs.Clear();
            tabs.Add(new Match3LevelEditorTab(this));
            tabs.Add(new Match3AboutTab(this));
            selectedTabIndex = 0;
        }

        /// <summary>
        /// Unity OnGUI回调
        /// </summary>
        private void OnGUI()
        {
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, new[] { "关卡编辑器", "关于" });
            if (selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
            {
                var selectedEditor = tabs[selectedTabIndex];
                if (selectedTabIndex != prevSelectedTabIndex)
                {
                    selectedEditor.OnTabSelected();
                    GUI.FocusControl(null);
                }
                selectedEditor.Draw();
                prevSelectedTabIndex = selectedTabIndex;
            }
        }
    }
}
