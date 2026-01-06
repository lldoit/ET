using UnityEditor;
using UnityEngine;

namespace ET.Match3.Editor
{
    /// <summary>
    /// 关于页面Tab
    /// </summary>
    public class Match3AboutTab : Match3EditorTab
    {
        public Match3AboutTab(Match3KitEditor editor) : base(editor)
        {
        }

        public override void Draw()
        {
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label("Match3 Kit Editor", titleStyle);

            GUILayout.Space(10);

            var versionStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label("版本: 1.0.0", versionStyle);

            GUILayout.Space(20);

            var descStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            GUILayout.Label("基于ET框架的三消游戏关卡编辑器", descStyle);
            GUILayout.Label("参照CandyMatch3Kit实现", descStyle);

            GUILayout.Space(30);

            GUILayout.Label("功能说明:", EditorStyles.boldLabel);
            GUILayout.Label("• 可视化编辑关卡布局", descStyle);
            GUILayout.Label("• 支持多种笔刷类型和模式", descStyle);
            GUILayout.Label("• 配置关卡目标和可用道具", descStyle);
            GUILayout.Label("• 导入/导出JSON格式关卡文件", descStyle);

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
