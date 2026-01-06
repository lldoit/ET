using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ET.Match3.Editor
{
    /// <summary>
    /// 笔刷类型枚举
    /// </summary>
    public enum BrushType
    {
        Candy,
        Element,
        SpecialCandy,
        SpecialBlock,
        Collectable,
        Empty,
        Hole
    }

    /// <summary>
    /// 笔刷模式枚举
    /// </summary>
    public enum BrushMode
    {
        Tile,
        Row,
        Column,
        Fill
    }

    /// <summary>
    /// 关卡编辑器Tab
    /// </summary>
    public class Match3LevelEditorTab : Match3EditorTab
    {
        // 关卡数据
        private Level currentLevel;
        private bool hasLevel = false;
        private int prevWidth = -1;
        private int prevHeight = -1;

        // 笔刷设置
        private BrushType currentBrushType = BrushType.Candy;
        private BrushMode currentBrushMode = BrushMode.Tile;
        private CandyType currentCandyType = CandyType.RandomCandy;
        private ElementType currentElementType = ElementType.None;
        private SpecialCandyType currentSpecialCandyType = SpecialCandyType.ColorBomb;
        private SpecialBlockType currentSpecialBlockType = SpecialBlockType.Unbreakable;
        private CollectableType currentCollectableType = CollectableType.Cherry;

        // 目标列表
        private ReorderableList goalList;
        private int currentGoalIndex = -1;

        // 可用颜色列表
        private ReorderableList colorList;
        private int currentColorIndex = -1;

        // 滚动位置
        private Vector2 scrollPos;

        // 瓦片纹理缓存
        private readonly Dictionary<string, Texture2D> tileTextures = new Dictionary<string, Texture2D>();

        // 关卡存储路径
        private const string LEVELS_PATH = "Packages/cn.etetet.match3/GameRes/Match3/Levels";

        public Match3LevelEditorTab(Match3KitEditor editor) : base(editor)
        {
            LoadTileTextures();
        }

        /// <summary>
        /// 加载瓦片纹理
        /// </summary>
        private void LoadTileTextures()
        {
            // 从本地Editor/Resources目录加载纹理
            string editorResourcesPath = "Packages/cn.etetet.match3/Editor/Resources";
            if (Directory.Exists(editorResourcesPath))
            {
                var files = Directory.GetFiles(editorResourcesPath, "*.png");
                foreach (var file in files)
                {
                    string filename = Path.GetFileNameWithoutExtension(file);
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                    if (texture != null)
                    {
                        tileTextures[filename] = texture;
                    }
                }
            }
        }

        public override void Draw()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.Space(15);
            DrawMenu();

            if (hasLevel)
            {
                GUILayout.Space(15);

                EditorGUILayout.BeginHorizontal();
                
                // 左侧：通用设置和道具设置
                EditorGUILayout.BeginVertical(GUILayout.Width(350));
                DrawGeneralSettings();
                GUILayout.Space(15);
                DrawBoosterSettings();
                EditorGUILayout.EndVertical();

                GUILayout.Space(50);

                // 右侧：目标设置和颜色设置
                EditorGUILayout.BeginVertical(GUILayout.Width(400));
                DrawGoalSettings();
                GUILayout.Space(15);
                DrawColorSettings();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(15);
                DrawLevelEditor();
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制菜单栏
        /// </summary>
        private void DrawMenu()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("新建", GUILayout.Width(100), GUILayout.Height(40)))
            {
                CreateNewLevel();
            }

            if (GUILayout.Button("打开", GUILayout.Width(100), GUILayout.Height(40)))
            {
                OpenLevel();
            }

            if (GUILayout.Button("保存", GUILayout.Width(100), GUILayout.Height(40)))
            {
                SaveLevel();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 创建新关卡
        /// </summary>
        private void CreateNewLevel()
        {
            currentLevel = new Level
            {
                Id = 1,
                Width = 9,
                Height = 9,
                LimitType = LimitType.Moves,
                Limit = 20,
                Score1 = 1000,
                Score2 = 2000,
                Score3 = 3000,
                AwardSpecialCandies = false,
                AwardedSpecialCandyType = AwardedSpecialCandyType.Striped,
                CollectableChance = 10,
                Tiles = new List<LevelTile>(),
                Goals = new List<Goal>(),
                AvailableColors = new List<CandyColor>(),
                AvailableBoosters = new Dictionary<BoosterType, bool>()
            };

            // 初始化默认颜色
            foreach (CandyColor color in Enum.GetValues(typeof(CandyColor)))
            {
                currentLevel.AvailableColors.Add(color);
            }

            // 初始化道具设置
            foreach (BoosterType booster in Enum.GetValues(typeof(BoosterType)))
            {
                currentLevel.AvailableBoosters[booster] = true;
            }

            // 初始化瓦片
            InitializeTiles();

            hasLevel = true;
            prevWidth = currentLevel.Width;
            prevHeight = currentLevel.Height;

            CreateGoalList();
            CreateColorList();
        }

        /// <summary>
        /// 初始化瓦片网格
        /// </summary>
        private void InitializeTiles()
        {
            currentLevel.Tiles = new List<LevelTile>();
            for (int i = 0; i < currentLevel.Width * currentLevel.Height; i++)
            {
                currentLevel.Tiles.Add(LevelTile.CreateCandy(CandyType.RandomCandy));
            }
        }

        /// <summary>
        /// 打开关卡
        /// </summary>
        private void OpenLevel()
        {
            string defaultPath = Path.GetFullPath(LEVELS_PATH);
            if (!Directory.Exists(defaultPath))
            {
                defaultPath = Application.dataPath;
            }

            string path = EditorUtility.OpenFilePanel("打开关卡", defaultPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                currentLevel = Match3LevelSerializer.LoadLevel(path);
                if (currentLevel.Tiles != null && currentLevel.Tiles.Count > 0)
                {
                    hasLevel = true;
                    prevWidth = currentLevel.Width;
                    prevHeight = currentLevel.Height;
                    CreateGoalList();
                    CreateColorList();
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "加载关卡失败", "确定");
                }
            }
        }

        /// <summary>
        /// 保存关卡
        /// </summary>
        private void SaveLevel()
        {
            if (!hasLevel)
            {
                EditorUtility.DisplayDialog("错误", "没有可保存的关卡", "确定");
                return;
            }

            // 确保目录存在
            string levelsPath = Path.GetFullPath(LEVELS_PATH);
            if (!Directory.Exists(levelsPath))
            {
                Directory.CreateDirectory(levelsPath);
            }

            string path = Path.Combine(levelsPath, $"{currentLevel.Id}.json");
            Match3LevelSerializer.SaveLevel(path, currentLevel);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("成功", $"关卡已保存到:\n{path}", "确定");
        }

        /// <summary>
        /// 绘制通用设置
        /// </summary>
        private void DrawGeneralSettings()
        {
            EditorGUILayout.LabelField("通用设置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("配置关卡的基本参数", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("关卡ID", GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.Id = EditorGUILayout.IntField(currentLevel.Id, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("限制类型", GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.LimitType = (LimitType)EditorGUILayout.EnumPopup(currentLevel.LimitType, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            string limitLabel = currentLevel.LimitType == LimitType.Moves ? "步数" : "时间(秒)";
            EditorGUILayout.LabelField(limitLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.Limit = EditorGUILayout.IntField(currentLevel.Limit, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("一星分数", GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.Score1 = EditorGUILayout.IntField(currentLevel.Score1, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("二星分数", GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.Score2 = EditorGUILayout.IntField(currentLevel.Score2, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("三星分数", GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.Score3 = EditorGUILayout.IntField(currentLevel.Score3, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("奖励特殊糖果", GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.AwardSpecialCandies = EditorGUILayout.Toggle(currentLevel.AwardSpecialCandies);
            EditorGUILayout.EndHorizontal();

            if (currentLevel.AwardSpecialCandies)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("奖励类型", GUILayout.Width(EditorGUIUtility.labelWidth));
                currentLevel.AwardedSpecialCandyType = (AwardedSpecialCandyType)EditorGUILayout.EnumPopup(
                    currentLevel.AwardedSpecialCandyType, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("收集物概率%", GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.CollectableChance = EditorGUILayout.IntField(currentLevel.CollectableChance, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制道具设置
        /// </summary>
        private void DrawBoosterSettings()
        {
            EditorGUILayout.LabelField("道具设置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("配置关卡中可使用的道具", MessageType.Info);

            if (currentLevel.AvailableBoosters == null)
            {
                currentLevel.AvailableBoosters = new Dictionary<BoosterType, bool>();
            }

            foreach (BoosterType booster in Enum.GetValues(typeof(BoosterType)))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(GetBoosterDisplayName(booster), GUILayout.Width(EditorGUIUtility.labelWidth));
                
                bool enabled = currentLevel.AvailableBoosters.GetValueOrDefault(booster);
                bool newEnabled = EditorGUILayout.Toggle(enabled);
                currentLevel.AvailableBoosters[booster] = newEnabled;
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private string GetBoosterDisplayName(BoosterType booster)
        {
            switch (booster)
            {
                case BoosterType.Lollipop: return "棒棒糖";
                case BoosterType.Bomb: return "炸弹";
                case BoosterType.Switch: return "交换";
                case BoosterType.ColorBomb: return "彩虹糖";
                default: return booster.ToString();
            }
        }

        /// <summary>
        /// 绘制目标设置
        /// </summary>
        private void DrawGoalSettings()
        {
            EditorGUILayout.LabelField("目标设置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("配置玩家需要完成的目标", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            if (goalList != null)
            {
                goalList.DoLayoutList();
            }
            EditorGUILayout.EndVertical();

            if (currentGoalIndex >= 0 && currentGoalIndex < currentLevel.Goals.Count)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(180));
                DrawGoalEditor(currentGoalIndex);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制目标编辑器
        /// </summary>
        private void DrawGoalEditor(int index)
        {
            var goal = currentLevel.Goals[index];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("类型", GUILayout.Width(60));
            var newGoalType = (GoalType)EditorGUILayout.EnumPopup(goal.GoalType, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            if (newGoalType != goal.GoalType)
            {
                goal.GoalType = newGoalType;
            }

            switch (goal.GoalType)
            {
                case GoalType.ReachScore:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("分数", GUILayout.Width(60));
                    goal.Amount = EditorGUILayout.IntField(goal.Amount, GUILayout.Width(70));
                    EditorGUILayout.EndHorizontal();
                    break;

                case GoalType.CollectCandy:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("颜色", GUILayout.Width(60));
                    goal.CandyColor = (CandyColor)EditorGUILayout.EnumPopup(goal.CandyColor, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("数量", GUILayout.Width(60));
                    goal.Amount = EditorGUILayout.IntField(goal.Amount, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    break;

                case GoalType.CollectElement:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("元素", GUILayout.Width(60));
                    goal.ElementType = (ElementType)EditorGUILayout.EnumPopup(goal.ElementType, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("数量", GUILayout.Width(60));
                    goal.Amount = EditorGUILayout.IntField(goal.Amount, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    break;

                case GoalType.CollectSpecialBlock:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("方块", GUILayout.Width(60));
                    goal.SpecialBlockType = (SpecialBlockType)EditorGUILayout.EnumPopup(goal.SpecialBlockType, GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("数量", GUILayout.Width(60));
                    goal.Amount = EditorGUILayout.IntField(goal.Amount, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    break;

                case GoalType.CollectCollectable:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("收集物", GUILayout.Width(60));
                    goal.CollectableType = (CollectableType)EditorGUILayout.EnumPopup(goal.CollectableType, GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("数量", GUILayout.Width(60));
                    goal.Amount = EditorGUILayout.IntField(goal.Amount, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    break;

                case GoalType.DestroyAllChocolate:
                    EditorGUILayout.LabelField("摧毁所有巧克力");
                    break;
            }

            currentLevel.Goals[index] = goal;
        }

        /// <summary>
        /// 创建目标列表
        /// </summary>
        private void CreateGoalList()
        {
            if (currentLevel.Goals == null)
            {
                currentLevel.Goals = new List<Goal>();
            }

            goalList = new ReorderableList(currentLevel.Goals, typeof(Goal), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "目标列表"),
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index >= 0 && index < currentLevel.Goals.Count)
                    {
                        var goal = currentLevel.Goals[index];
                        EditorGUI.LabelField(rect, GetGoalDisplayName(goal));
                    }
                },
                onSelectCallback = list =>
                {
                    currentGoalIndex = list.index;
                },
                onAddDropdownCallback = (rect, list) =>
                {
                    var menu = new GenericMenu();
                    foreach (GoalType goalType in Enum.GetValues(typeof(GoalType)))
                    {
                        menu.AddItem(new GUIContent(GetGoalTypeDisplayName(goalType)), false, () =>
                        {
                            var newGoal = new Goal { GoalType = goalType, Amount = 10 };
                            currentLevel.Goals.Add(newGoal);
                            currentGoalIndex = currentLevel.Goals.Count - 1;
                        });
                    }
                    menu.ShowAsContext();
                },
                onRemoveCallback = list =>
                {
                    if (list.index >= 0 && list.index < currentLevel.Goals.Count)
                    {
                        currentLevel.Goals.RemoveAt(list.index);
                        currentGoalIndex = -1;
                    }
                }
            };
        }

        private string GetGoalDisplayName(Goal goal)
        {
            switch (goal.GoalType)
            {
                case GoalType.ReachScore:
                    return $"达到 {goal.Amount} 分";
                case GoalType.CollectCandy:
                    return $"收集 {goal.Amount} 个 {goal.CandyColor}";
                case GoalType.CollectElement:
                    return $"收集 {goal.Amount} 个 {goal.ElementType}";
                case GoalType.CollectSpecialBlock:
                    return $"收集 {goal.Amount} 个 {goal.SpecialBlockType}";
                case GoalType.CollectCollectable:
                    return $"收集 {goal.Amount} 个 {goal.CollectableType}";
                case GoalType.DestroyAllChocolate:
                    return "摧毁所有巧克力";
                default:
                    return goal.GoalType.ToString();
            }
        }

        private string GetGoalTypeDisplayName(GoalType goalType)
        {
            switch (goalType)
            {
                case GoalType.ReachScore: return "达到分数";
                case GoalType.CollectCandy: return "收集糖果";
                case GoalType.CollectElement: return "收集元素";
                case GoalType.CollectSpecialBlock: return "收集特殊方块";
                case GoalType.CollectCollectable: return "收集收集物";
                case GoalType.DestroyAllChocolate: return "摧毁所有巧克力";
                default: return goalType.ToString();
            }
        }

        /// <summary>
        /// 绘制颜色设置
        /// </summary>
        private void DrawColorSettings()
        {
            EditorGUILayout.LabelField("可用颜色", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("配置随机糖果的颜色范围", MessageType.Info);

            if (colorList != null)
            {
                colorList.DoLayoutList();
            }
        }

        /// <summary>
        /// 创建颜色列表
        /// </summary>
        private void CreateColorList()
        {
            if (currentLevel.AvailableColors == null)
            {
                currentLevel.AvailableColors = new List<CandyColor>();
            }

            colorList = new ReorderableList(currentLevel.AvailableColors, typeof(CandyColor), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "颜色列表"),
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index >= 0 && index < currentLevel.AvailableColors.Count)
                    {
                        var color = currentLevel.AvailableColors[index];
                        EditorGUI.LabelField(rect, GetColorDisplayName(color));
                    }
                },
                onSelectCallback = list =>
                {
                    currentColorIndex = list.index;
                },
                onAddDropdownCallback = (rect, list) =>
                {
                    var menu = new GenericMenu();
                    foreach (CandyColor color in Enum.GetValues(typeof(CandyColor)))
                    {
                        if (!currentLevel.AvailableColors.Contains(color))
                        {
                            menu.AddItem(new GUIContent(GetColorDisplayName(color)), false, () =>
                            {
                                currentLevel.AvailableColors.Add(color);
                            });
                        }
                    }
                    menu.ShowAsContext();
                },
                onRemoveCallback = list =>
                {
                    if (currentLevel.AvailableColors.Count <= 1)
                    {
                        EditorUtility.DisplayDialog("警告", "至少需要保留一种颜色", "确定");
                        return;
                    }
                    if (list.index >= 0 && list.index < currentLevel.AvailableColors.Count)
                    {
                        currentLevel.AvailableColors.RemoveAt(list.index);
                        currentColorIndex = -1;
                    }
                }
            };
        }

        private string GetColorDisplayName(CandyColor color)
        {
            switch (color)
            {
                case CandyColor.Blue: return "蓝色";
                case CandyColor.Green: return "绿色";
                case CandyColor.Orange: return "橙色";
                case CandyColor.Purple: return "紫色";
                case CandyColor.Red: return "红色";
                case CandyColor.Yellow: return "黄色";
                default: return color.ToString();
            }
        }

        /// <summary>
        /// 绘制关卡编辑器
        /// </summary>
        private void DrawLevelEditor()
        {
            EditorGUILayout.LabelField("关卡布局", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("使用笔刷编辑关卡布局", MessageType.Info);

            // 宽度高度设置
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("宽度", GUILayout.Width(50));
            int newWidth = EditorGUILayout.IntField(currentLevel.Width, GUILayout.Width(40));
            GUILayout.Space(20);
            EditorGUILayout.LabelField("高度", GUILayout.Width(50));
            int newHeight = EditorGUILayout.IntField(currentLevel.Height, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            // 检查尺寸变化
            if (newWidth != currentLevel.Width || newHeight != currentLevel.Height)
            {
                if (newWidth > 0 && newWidth <= 15 && newHeight > 0 && newHeight <= 15)
                {
                    currentLevel.Width = newWidth;
                    currentLevel.Height = newHeight;
                    InitializeTiles();
                    prevWidth = newWidth;
                    prevHeight = newHeight;
                }
            }

            GUILayout.Space(10);

            // 笔刷设置
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("笔刷类型", GUILayout.Width(70));
            currentBrushType = (BrushType)EditorGUILayout.EnumPopup(currentBrushType, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            // 根据笔刷类型显示子选项
            switch (currentBrushType)
            {
                case BrushType.Candy:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("糖果类型", GUILayout.Width(70));
                    currentCandyType = (CandyType)EditorGUILayout.EnumPopup(currentCandyType, GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();
                    break;

                case BrushType.Element:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("元素类型", GUILayout.Width(70));
                    currentElementType = (ElementType)EditorGUILayout.EnumPopup(currentElementType, GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();
                    break;

                case BrushType.SpecialCandy:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("特殊糖果", GUILayout.Width(70));
                    currentSpecialCandyType = (SpecialCandyType)EditorGUILayout.EnumPopup(currentSpecialCandyType, GUILayout.Width(180));
                    EditorGUILayout.EndHorizontal();
                    break;

                case BrushType.SpecialBlock:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("特殊方块", GUILayout.Width(70));
                    currentSpecialBlockType = (SpecialBlockType)EditorGUILayout.EnumPopup(currentSpecialBlockType, GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();
                    break;

                case BrushType.Collectable:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("收集物", GUILayout.Width(70));
                    currentCollectableType = (CollectableType)EditorGUILayout.EnumPopup(currentCollectableType, GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("笔刷模式", GUILayout.Width(70));
            currentBrushMode = (BrushMode)EditorGUILayout.EnumPopup(currentBrushMode, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            // 绘制网格
            DrawGrid();
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawGrid()
        {
            for (int y = 0; y < currentLevel.Height; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < currentLevel.Width; x++)
                {
                    int index = x + y * currentLevel.Width;
                    DrawTileButton(index, x, y);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制瓦片按钮
        /// </summary>
        private void DrawTileButton(int index, int x, int y)
        {
            if (index >= currentLevel.Tiles.Count)
                return;

            var tile = currentLevel.Tiles[index];
            string textureName = GetTileTextureName(tile);
            
            GUIContent content;
            if (tileTextures.TryGetValue(textureName, out var texture))
            {
                content = new GUIContent(texture);
            }
            else
            {
                content = new GUIContent(GetTileLabel(tile));
            }

            if (GUILayout.Button(content, GUILayout.Width(50), GUILayout.Height(50)))
            {
                ApplyBrush(x, y);
            }
        }

        /// <summary>
        /// 获取瓦片纹理名称
        /// </summary>
        private string GetTileTextureName(LevelTile tile)
        {
            string baseName = "";
            switch (tile.TileType)
            {
                case LevelTileType.Candy:
                    baseName = tile.CandyType.ToString();
                    break;
                case LevelTileType.SpecialCandy:
                    baseName = tile.SpecialCandyType.ToString();
                    break;
                case LevelTileType.SpecialBlock:
                    baseName = tile.SpecialBlockType.ToString();
                    break;
                case LevelTileType.Collectable:
                    baseName = tile.CollectableType.ToString();
                    break;
                case LevelTileType.Hole:
                    baseName = "Hole";
                    break;
                case LevelTileType.Empty:
                    return "";
            }

            // 添加元素后缀
            if (tile.ElementType != ElementType.None)
            {
                baseName += "_" + tile.ElementType.ToString();
            }

            return baseName;
        }

        /// <summary>
        /// 获取瓦片标签文本（当没有纹理时使用）
        /// </summary>
        private string GetTileLabel(LevelTile tile)
        {
            switch (tile.TileType)
            {
                case LevelTileType.Candy:
                    return GetCandyShortName(tile.CandyType);
                case LevelTileType.SpecialCandy:
                    return "SC";
                case LevelTileType.SpecialBlock:
                    return GetBlockShortName(tile.SpecialBlockType);
                case LevelTileType.Collectable:
                    return "C";
                case LevelTileType.Hole:
                    return "H";
                case LevelTileType.Empty:
                    return "";
                default:
                    return "?";
            }
        }

        private string GetCandyShortName(CandyType type)
        {
            switch (type)
            {
                case CandyType.BlueCandy: return "B";
                case CandyType.GreenCandy: return "G";
                case CandyType.OrangeCandy: return "O";
                case CandyType.PurpleCandy: return "P";
                case CandyType.RedCandy: return "R";
                case CandyType.YellowCandy: return "Y";
                case CandyType.RandomCandy: return "?";
                default: return "?";
            }
        }

        private string GetBlockShortName(SpecialBlockType type)
        {
            switch (type)
            {
                case SpecialBlockType.Marshmallow: return "M";
                case SpecialBlockType.Chocolate: return "Ch";
                case SpecialBlockType.Unbreakable: return "U";
                default: return "?";
            }
        }

        /// <summary>
        /// 应用笔刷
        /// </summary>
        private void ApplyBrush(int x, int y)
        {
            switch (currentBrushMode)
            {
                case BrushMode.Tile:
                    ApplyBrushToTile(x, y);
                    break;

                case BrushMode.Row:
                    for (int i = 0; i < currentLevel.Width; i++)
                    {
                        ApplyBrushToTile(i, y);
                    }
                    break;

                case BrushMode.Column:
                    for (int j = 0; j < currentLevel.Height; j++)
                    {
                        ApplyBrushToTile(x, j);
                    }
                    break;

                case BrushMode.Fill:
                    for (int j = 0; j < currentLevel.Height; j++)
                    {
                        for (int i = 0; i < currentLevel.Width; i++)
                        {
                            ApplyBrushToTile(i, j);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 应用笔刷到单个瓦片
        /// </summary>
        private void ApplyBrushToTile(int x, int y)
        {
            int index = x + y * currentLevel.Width;
            if (index < 0 || index >= currentLevel.Tiles.Count)
                return;

            var tile = currentLevel.Tiles[index];

            switch (currentBrushType)
            {
                case BrushType.Candy:
                    currentLevel.Tiles[index] = LevelTile.CreateCandy(currentCandyType, tile.ElementType);
                    break;

                case BrushType.Element:
                    tile.ElementType = currentElementType;
                    currentLevel.Tiles[index] = tile;
                    break;

                case BrushType.SpecialCandy:
                    currentLevel.Tiles[index] = LevelTile.CreateSpecialCandy(currentSpecialCandyType, tile.ElementType);
                    break;

                case BrushType.SpecialBlock:
                    currentLevel.Tiles[index] = LevelTile.CreateSpecialBlock(currentSpecialBlockType, tile.ElementType);
                    break;

                case BrushType.Collectable:
                    currentLevel.Tiles[index] = LevelTile.CreateCollectable(currentCollectableType, tile.ElementType);
                    break;

                case BrushType.Empty:
                    currentLevel.Tiles[index] = LevelTile.CreateEmpty();
                    break;

                case BrushType.Hole:
                    currentLevel.Tiles[index] = LevelTile.CreateHole();
                    break;
            }
        }
    }
}
