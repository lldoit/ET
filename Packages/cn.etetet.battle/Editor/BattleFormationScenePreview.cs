using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ET.Client;
using System.Collections.Generic;

namespace ET.Editor
{
    /// <summary>
    /// 战斗站位场景预览器
    /// 在专用预览Scene中预览实际的UI和角色站位
    /// </summary>
    public class BattleFormationScenePreview : EditorWindow
    {
        private BattleFormationConfig config;
        private GameObject battlePanelInstance;
        private GameObject heroPrefab;
        private GameObject previewRoot;
        private List<GameObject> heroInstances = new List<GameObject>();

        // 预览设置
        private int previewEnemyCount = 4;
        private int previewPlayerCount = 4;

        // 资源路径
        private const string BATTLE_PANEL_PATH = "Packages/cn.etetet.battle/Assets/GameRes/YIUI/Battle/Prefabs/BattlePanel.prefab";
        private const string HERO_PREFAB_PATH = "Packages/cn.etetet.battle/Assets/GameRes/Prefab/Hero/gong.prefab";
        private const string PREVIEW_SCENE_PATH = "Packages/cn.etetet.battle/Editor/Scenes/FormationPreviewScene.unity";

        // 之前的场景
        private string previousScenePath;
        private bool isInPreviewScene = false;

        [MenuItem("Tools/Battle/站位场景预览")]
        public static void ShowWindow()
        {
            var window = GetWindow<BattleFormationScenePreview>("站位场景预览");
            window.minSize = new Vector2(400, 350);
        }

        private void OnEnable()
        {
            LoadDefaultConfig();
            LoadPrefabs();
        }

        private void OnDisable()
        {
            // 窗口关闭时清理并返回之前的场景
            if (isInPreviewScene)
            {
                CleanupAndReturnToPreviousScene();
            }
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

        private void LoadPrefabs()
        {
            heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HERO_PREFAB_PATH);
            if (heroPrefab == null)
            {
                Debug.LogWarning($"[站位预览] 未找到角色预制体: {HERO_PREFAB_PATH}");
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("站位场景预览", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // 配置文件选择
            EditorGUI.BeginChangeCheck();
            config = (BattleFormationConfig)EditorGUILayout.ObjectField("站位配置", config, typeof(BattleFormationConfig), false);
            if (EditorGUI.EndChangeCheck() && isInPreviewScene)
            {
                RefreshPreview();
            }

            // 角色预制体选择
            EditorGUI.BeginChangeCheck();
            heroPrefab = (GameObject)EditorGUILayout.ObjectField("角色预制体", heroPrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck() && isInPreviewScene)
            {
                RefreshPreview();
            }

            EditorGUILayout.Space(10);

            // 预览设置
            EditorGUILayout.LabelField("预览设置", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            previewPlayerCount = EditorGUILayout.IntSlider("玩家方人数", previewPlayerCount, 1, 4);
            previewEnemyCount = EditorGUILayout.IntSlider("敌方人数", previewEnemyCount, 1, 4);
            if (EditorGUI.EndChangeCheck() && isInPreviewScene)
            {
                RefreshPreview();
            }

            EditorGUILayout.Space(10);

            // 操作按钮
            if (!isInPreviewScene)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("进入预览场景", GUILayout.Height(40)))
                {
                    EnterPreviewScene();
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("点击「进入预览场景」将打开专用预览场景，并在其中显示站位预览。\n\n注意：进入预览前请保存当前场景！", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("刷新预览"))
                {
                    RefreshPreview();
                }

                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("退出预览场景"))
                {
                    CleanupAndReturnToPreviousScene();
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                // 状态显示
                EditorGUILayout.HelpBox($"预览中\n玩家方: {previewPlayerCount} 人\n敌方: {previewEnemyCount} 人\n角色总数: {heroInstances.Count}", MessageType.Info);

                EditorGUILayout.Space(5);

                // 快速定位按钮
                if (battlePanelInstance != null && GUILayout.Button("聚焦到预览"))
                {
                    Selection.activeGameObject = battlePanelInstance;
                    SceneView.FrameLastActiveSceneView();
                }

                // 切换到Game视图
                if (GUILayout.Button("切换到Game视图"))
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Game");
                }
            }
        }

        private void EnterPreviewScene()
        {
            if (config == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择站位配置文件！", "确定");
                return;
            }

            // 保存当前场景路径
            var currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.isDirty)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return; // 用户取消了操作
                }
            }
            previousScenePath = currentScene.path;

            // 创建或打开预览场景
            CreateOrOpenPreviewScene();

            isInPreviewScene = true;

            // 创建预览内容
            CreatePreviewContent();
        }

        private void CreateOrOpenPreviewScene()
        {
            if (System.IO.File.Exists(PREVIEW_SCENE_PATH))
            {
                EditorSceneManager.OpenScene(PREVIEW_SCENE_PATH);
                
                previewRoot = GameObject.Find("[Preview] Canvas");
            }
            else
            {
                // 确保目录存在
                string sceneDir = System.IO.Path.GetDirectoryName(PREVIEW_SCENE_PATH);
                if (!System.IO.Directory.Exists(sceneDir))
                {
                    System.IO.Directory.CreateDirectory(sceneDir);
                }

                // 创建新的空场景
                var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

                // 配置Camera
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    mainCamera.orthographic = true;
                    mainCamera.orthographicSize = 5;
                    mainCamera.transform.position = new Vector3(0, 0, -10);
                    mainCamera.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
                }

                // 创建UI Canvas
                CreateUICanvas();
            }

            Debug.Log("[站位预览] 已进入预览场景");
        }

        private void CreateUICanvas()
        {
            // 创建Canvas
            GameObject canvasGO = new GameObject("[Preview] Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = -10;
            canvas.planeDistance = 100;

            // 添加CanvasScaler
            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // 添加GraphicRaycaster
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            previewRoot = canvasGO;
        }

        private void CreatePreviewContent()
        {
            if (previewRoot == null)
            {
                Debug.LogError("[站位预览] 预览根节点不存在");
                return;
            }

            // 加载并实例化BattlePanel
            GameObject battlePanelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BATTLE_PANEL_PATH);
            if (battlePanelPrefab != null)
            {
                battlePanelInstance = (GameObject)PrefabUtility.InstantiatePrefab(battlePanelPrefab, previewRoot.transform);
                battlePanelInstance.name = "[Preview] BattlePanel";

                // 确保RectTransform正确设置
                RectTransform rt = battlePanelInstance.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                Debug.LogWarning($"[站位预览] 未找到BattlePanel预制体: {BATTLE_PANEL_PATH}");
                // 创建一个空的容器
                battlePanelInstance = new GameObject("[Preview] BattlePanel");
                battlePanelInstance.transform.SetParent(previewRoot.transform, false);
                var rt = battlePanelInstance.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // 创建角色预览
            CreateHeroPreview();

            Debug.Log($"[站位预览] 预览已创建，玩家方 {previewPlayerCount} 人，敌方 {previewEnemyCount} 人");
        }

        private void CreateHeroPreview()
        {
            if (heroPrefab == null)
            {
                Debug.LogWarning("[站位预览] 未设置角色预制体，将使用占位符");
            }

            // 查找角色容器（Unit节点）
            Transform heroContainer = battlePanelInstance.transform;

            // 创建玩家方角色
            for (int i = 0; i < previewPlayerCount; i++)
            {
                var slot = config.GetPlayerSlot(i);
                CreateHeroAtPosition(heroContainer, slot.Position, slot.FacingLeft, $"[Preview] Player_{i + 1}", Color.blue);
            }

            // 创建敌方角色
            var enemyFormation = config.GetEnemyFormation(previewEnemyCount);
            for (int i = 0; i < previewEnemyCount && i < enemyFormation.SlotCount; i++)
            {
                var slot = enemyFormation.GetSlot(i);
                CreateHeroAtPosition(heroContainer, slot.Position, slot.FacingLeft, $"[Preview] Enemy_{i + 1}", Color.red);
            }
        }

        private void CreateHeroAtPosition(Transform parent, Vector2 position, bool facingLeft, string name, Color labelColor)
        {
            GameObject heroInstance;

            if (heroPrefab != null)
            {
                heroInstance = (GameObject)PrefabUtility.InstantiatePrefab(heroPrefab, parent);
            }
            else
            {
                // 创建一个UI占位符
                heroInstance = new GameObject(name);
                heroInstance.transform.SetParent(parent, false);

                var image = heroInstance.AddComponent<UnityEngine.UI.Image>();
                image.color = new Color(labelColor.r, labelColor.g, labelColor.b, 0.5f);

                var rt = heroInstance.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 150);
            }

            heroInstance.name = name;
            heroInstance.transform.localPosition = new Vector3(position.x, position.y, 0);

            // 设置朝向
            Vector3 scale = heroInstance.transform.localScale;
            scale.x = facingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            heroInstance.transform.localScale = scale;

            heroInstances.Add(heroInstance);
        }

        private void RefreshPreview()
        {
            if (!isInPreviewScene || battlePanelInstance == null) return;

            // 清除旧的角色
            foreach (var hero in heroInstances)
            {
                if (hero != null)
                {
                    DestroyImmediate(hero);
                }
            }
            heroInstances.Clear();

            // 重新创建角色
            if (config != null)
            {
                CreateHeroPreview();
            }
        }

        private void CleanupAndReturnToPreviousScene()
        {
            // 清除角色实例
            foreach (var hero in heroInstances)
            {
                if (hero != null)
                {
                    DestroyImmediate(hero);
                }
            }
            heroInstances.Clear();

            battlePanelInstance = null;
            previewRoot = null;

            isInPreviewScene = false;

            // 返回之前的场景
            if (!string.IsNullOrEmpty(previousScenePath) && System.IO.File.Exists(previousScenePath))
            {
                EditorSceneManager.OpenScene(previousScenePath);
                Debug.Log($"[站位预览] 已返回场景: {previousScenePath}");
            }
            else
            {
                // 创建一个新的空场景
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                Debug.Log("[站位预览] 已创建新场景");
            }
        }
    }
}
