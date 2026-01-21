using UnityEngine;
using UnityEditor;
using ET.Client;

namespace ET.Editor
{
    /// <summary>
    /// 战斗站位配置Inspector编辑器
    /// </summary>
    [CustomEditor(typeof(BattleFormationConfig))]
    public class BattleFormationConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BattleFormationConfig config = (BattleFormationConfig)target;

            EditorGUILayout.Space(5);

            // 工具按钮行
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("打开站位编辑器"))
            {
                BattleFormationEditor.ShowWindow();
            }

            if (GUILayout.Button("重置为默认"))
            {
                if (EditorUtility.DisplayDialog("确认重置", "确定要重置所有站位到默认位置吗？", "确定", "取消"))
                {
                    Undo.RecordObject(config, "Reset Formation Config");
                    config.ResetToDefault();
                    EditorUtility.SetDirty(config);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 统计信息
            EditorGUILayout.HelpBox($"玩家方站位: {config.PlayerSlotCount}\n敌方1人站位: {config.EnemyFormation1.SlotCount}\n敌方2人站位: {config.EnemyFormation2.SlotCount}\n敌方3人站位: {config.EnemyFormation3.SlotCount}\n敌方4人站位: {config.EnemyFormation4.SlotCount}", MessageType.Info);

            EditorGUILayout.Space(10);

            // 绘制默认Inspector
            DrawDefaultInspector();
        }
    }
}
