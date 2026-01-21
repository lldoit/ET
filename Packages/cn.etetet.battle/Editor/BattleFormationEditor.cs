using UnityEngine;
using UnityEditor;
using ET.Client;

namespace ET.Editor
{
    /// <summary>
    /// 战斗站位编辑器窗口
    /// 可视化编辑战斗界面中角色的站位位置
    /// </summary>
    public class BattleFormationEditor : EditorWindow
    {
        private BattleFormationConfig config;
        private Vector2 scrollPosition;
        private bool showPlayerSlots = true;
        private bool showEnemy1Slots = true;
        private bool showEnemy2Slots = true;
        private bool showEnemy3Slots = true;
        private bool showEnemy4Slots = true;

        // 预览区域参数
        private Rect previewRect;
        private float previewScale = 0.3f;
        private Vector2 previewCenter = new Vector2(400, 300);

        // 预览的敌方配置（1-4）
        private int previewEnemyCount = 4;

        // 拖拽状态
        private int dragSide = -1; // 0=玩家方, 1-4=敌方
        private int dragIndex = -1;
        private bool isDragging = false;

        [MenuItem("Tools/Battle/站位编辑器")]
        public static void ShowWindow()
        {
            var window = GetWindow<BattleFormationEditor>("战斗站位编辑器");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            LoadDefaultConfig();
        }

        private void LoadDefaultConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:BattleFormationConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                config = AssetDatabase.LoadAssetAtPath<BattleFormationConfig>(path);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // 左侧面板 - 配置编辑
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            DrawConfigPanel();
            EditorGUILayout.EndVertical();

            // 右侧面板 - 可视化预览
            EditorGUILayout.BeginVertical();
            DrawPreviewPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            HandleDragEvents();
        }

        private void DrawConfigPanel()
        {
            EditorGUILayout.LabelField("站位配置", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            config = (BattleFormationConfig)EditorGUILayout.ObjectField("配置文件", config, typeof(BattleFormationConfig), false);
            if (EditorGUI.EndChangeCheck() && config != null)
            {
                Repaint();
            }

            EditorGUILayout.Space(10);

            if (config == null)
            {
                EditorGUILayout.HelpBox("请选择或创建一个站位配置文件", MessageType.Info);

                if (GUILayout.Button("创建新配置"))
                {
                    CreateNewConfig();
                }
                return;
            }

            // 工具按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重置为默认"))
            {
                if (EditorUtility.DisplayDialog("确认重置", "确定要重置所有站位到默认位置吗？", "确定", "取消"))
                {
                    Undo.RecordObject(config, "Reset Formation Config");
                    config.ResetToDefault();
                    EditorUtility.SetDirty(config);
                }
            }
            if (GUILayout.Button("保存"))
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                Debug.Log("[站位编辑器] 配置已保存");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 玩家方站位
            showPlayerSlots = EditorGUILayout.Foldout(showPlayerSlots, $"玩家方站位 ({config.PlayerSlotCount})", true);
            if (showPlayerSlots)
            {
                EditorGUI.indentLevel++;
                DrawFormationSide(ref config.PlayerFormation, "玩家", 0);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("敌方站位配置", EditorStyles.boldLabel);

            // 敌方1人站位
            showEnemy1Slots = EditorGUILayout.Foldout(showEnemy1Slots, $"敌方1人站位 ({config.EnemyFormation1.SlotCount})", true);
            if (showEnemy1Slots)
            {
                EditorGUI.indentLevel++;
                DrawFormationSide(ref config.EnemyFormation1, "敌方1人", 1);
                EditorGUI.indentLevel--;
            }

            // 敌方2人站位
            showEnemy2Slots = EditorGUILayout.Foldout(showEnemy2Slots, $"敌方2人站位 ({config.EnemyFormation2.SlotCount})", true);
            if (showEnemy2Slots)
            {
                EditorGUI.indentLevel++;
                DrawFormationSide(ref config.EnemyFormation2, "敌方2人", 2);
                EditorGUI.indentLevel--;
            }

            // 敌方3人站位
            showEnemy3Slots = EditorGUILayout.Foldout(showEnemy3Slots, $"敌方3人站位 ({config.EnemyFormation3.SlotCount})", true);
            if (showEnemy3Slots)
            {
                EditorGUI.indentLevel++;
                DrawFormationSide(ref config.EnemyFormation3, "敌方3人", 3);
                EditorGUI.indentLevel--;
            }

            // 敌方4人站位
            showEnemy4Slots = EditorGUILayout.Foldout(showEnemy4Slots, $"敌方4人站位 ({config.EnemyFormation4.SlotCount})", true);
            if (showEnemy4Slots)
            {
                EditorGUI.indentLevel++;
                DrawFormationSide(ref config.EnemyFormation4, "敌方4人", 4);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFormationSide(ref FormationSide side, string label, int sideIndex)
        {
            if (side.Slots == null) return;

            for (int i = 0; i < side.Slots.Length; i++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"站位 {i + 1}", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                Vector2 newPos = EditorGUILayout.Vector2Field("位置", side.Slots[i].Position);
                bool newFacing = EditorGUILayout.Toggle("面向左侧", side.Slots[i].FacingLeft);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(config, "Edit Formation Slot");
                    side.Slots[i].Position = newPos;
                    side.Slots[i].FacingLeft = newFacing;
                    EditorUtility.SetDirty(config);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.LabelField("可视化预览", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 预览缩放
            previewScale = EditorGUILayout.Slider("缩放", previewScale, 0.1f, 1f);

            // 预览敌方配置选择
            previewEnemyCount = EditorGUILayout.IntSlider("预览敌方人数", previewEnemyCount, 1, 4);

            EditorGUILayout.Space(5);

            // 获取预览区域
            previewRect = GUILayoutUtility.GetRect(400, 500, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            previewCenter = previewRect.center;

            // 绘制背景
            EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f));

            // 绘制中心线
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Handles.DrawLine(
                new Vector3(previewCenter.x, previewRect.yMin),
                new Vector3(previewCenter.x, previewRect.yMax)
            );
            Handles.DrawLine(
                new Vector3(previewRect.xMin, previewCenter.y),
                new Vector3(previewRect.xMax, previewCenter.y)
            );

            if (config == null) return;

            // 绘制站位
            DrawSlots(config.PlayerFormation, Color.blue, 0);
            DrawSlots(config.GetEnemyFormation(previewEnemyCount), Color.red, previewEnemyCount);

            // 绘制图例
            DrawLegend();
        }

        private void DrawSlots(FormationSide side, Color color, int sideIndex)
        {
            if (side.Slots == null) return;

            for (int i = 0; i < side.Slots.Length; i++)
            {
                var slot = side.Slots[i];
                Vector2 screenPos = WorldToScreen(slot.Position);

                // 绘制站位圆点
                float radius = 15f;
                Rect slotRect = new Rect(screenPos.x - radius, screenPos.y - radius, radius * 2, radius * 2);

                // 高亮当前拖拽的站位
                Color drawColor = color;
                if (isDragging && dragSide == sideIndex && dragIndex == i)
                {
                    drawColor = Color.yellow;
                }

                EditorGUI.DrawRect(slotRect, drawColor);

                // 绘制朝向箭头
                Vector2 arrowDir = slot.FacingLeft ? Vector2.left : Vector2.right;
                Vector2 arrowEnd = screenPos + arrowDir * 20f;
                Handles.color = Color.white;
                Handles.DrawLine(screenPos, arrowEnd);

                // 绘制索引标签
                GUI.Label(new Rect(screenPos.x - 10, screenPos.y - 25, 20, 20), (i + 1).ToString(), EditorStyles.whiteLabel);
            }
        }

        private void DrawLegend()
        {
            Rect legendRect = new Rect(previewRect.xMin + 10, previewRect.yMin + 10, 120, 50);
            EditorGUI.DrawRect(legendRect, new Color(0, 0, 0, 0.5f));

            GUI.Label(new Rect(legendRect.x + 5, legendRect.y + 5, 110, 20), "■ 玩家方", EditorStyles.whiteLabel);
            GUI.Label(new Rect(legendRect.x + 5, legendRect.y + 25, 110, 20), $"■ 敌方({previewEnemyCount}人)", EditorStyles.whiteLabel);
        }

        private Vector2 WorldToScreen(Vector2 worldPos)
        {
            // 翻转Y轴：UI坐标系Y向上，编辑器GUI坐标系Y向下
            return previewCenter + new Vector2(worldPos.x, -worldPos.y) * previewScale;
        }

        private Vector2 ScreenToWorld(Vector2 screenPos)
        {
            // 翻转Y轴：编辑器GUI坐标系Y向下，转换为UI坐标系Y向上
            Vector2 offset = (screenPos - previewCenter) / previewScale;
            return new Vector2(offset.x, -offset.y);
        }

        private void HandleDragEvents()
        {
            if (config == null) return;

            Event e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 && previewRect.Contains(e.mousePosition))
                    {
                        if (TryGetSlotAtPosition(e.mousePosition, out int side, out int index))
                        {
                            isDragging = true;
                            dragSide = side;
                            dragIndex = index;
                            e.Use();
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (isDragging && e.button == 0)
                    {
                        Vector2 worldPos = ScreenToWorld(e.mousePosition);
                        Undo.RecordObject(config, "Drag Formation Slot");

                        FormationSide targetSide = GetFormationSide(dragSide);
                        if (targetSide.Slots != null && dragIndex < targetSide.Slots.Length)
                        {
                            SetFormationSlotPosition(dragSide, dragIndex, worldPos);
                        }

                        EditorUtility.SetDirty(config);
                        Repaint();
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (isDragging)
                    {
                        isDragging = false;
                        dragSide = -1;
                        dragIndex = -1;
                        e.Use();
                    }
                    break;
            }
        }

        private FormationSide GetFormationSide(int sideIndex)
        {
            return sideIndex switch
            {
                0 => config.PlayerFormation,
                1 => config.EnemyFormation1,
                2 => config.EnemyFormation2,
                3 => config.EnemyFormation3,
                4 => config.EnemyFormation4,
                _ => config.PlayerFormation
            };
        }

        private void SetFormationSlotPosition(int sideIndex, int slotIndex, Vector2 position)
        {
            switch (sideIndex)
            {
                case 0:
                    config.PlayerFormation.Slots[slotIndex].Position = position;
                    break;
                case 1:
                    config.EnemyFormation1.Slots[slotIndex].Position = position;
                    break;
                case 2:
                    config.EnemyFormation2.Slots[slotIndex].Position = position;
                    break;
                case 3:
                    config.EnemyFormation3.Slots[slotIndex].Position = position;
                    break;
                case 4:
                    config.EnemyFormation4.Slots[slotIndex].Position = position;
                    break;
            }
        }

        private bool TryGetSlotAtPosition(Vector2 mousePos, out int side, out int index)
        {
            side = -1;
            index = -1;
            float radius = 15f;

            // 检查玩家方站位
            if (config.PlayerFormation.Slots != null)
            {
                for (int i = 0; i < config.PlayerFormation.Slots.Length; i++)
                {
                    Vector2 screenPos = WorldToScreen(config.PlayerFormation.Slots[i].Position);
                    if (Vector2.Distance(mousePos, screenPos) <= radius)
                    {
                        side = 0;
                        index = i;
                        return true;
                    }
                }
            }

            // 检查当前预览的敌方站位
            var enemyFormation = config.GetEnemyFormation(previewEnemyCount);
            if (enemyFormation.Slots != null)
            {
                for (int i = 0; i < enemyFormation.Slots.Length; i++)
                {
                    Vector2 screenPos = WorldToScreen(enemyFormation.Slots[i].Position);
                    if (Vector2.Distance(mousePos, screenPos) <= radius)
                    {
                        side = previewEnemyCount;
                        index = i;
                        return true;
                    }
                }
            }

            return false;
        }

        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "创建站位配置",
                "BattleFormationConfig",
                "asset",
                "选择保存位置"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var newConfig = CreateInstance<BattleFormationConfig>();
                newConfig.ResetToDefault();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                config = newConfig;
                Debug.Log($"[站位编辑器] 创建新配置: {path}");
            }
        }
    }
}
