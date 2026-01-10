namespace ET.Client
{
    /// <summary>
    /// 战斗场景切换助手
    /// 提供进入战斗和退出战斗的接口
    /// </summary>
    [FriendOf(typeof(BattlePreviousSceneComponent))]
    [FriendOf(typeof(TilePoolComponent))]
    public static class BattleSceneHelper
    {
        /// <summary>
        /// 进入战斗场景
        /// </summary>
        /// <param name="root">根场景</param>
        /// <param name="levelId">关卡ID</param>
        public static async ETTask EnterBattleAsync(Scene root, int levelId)
        {
            // 获取当前场景信息，用于战斗结束后返回
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene previousScene = currentScenesComponent.Scene;
            
            long previousSceneId = previousScene?.Id ?? 0;
            string previousSceneName = previousScene?.Name ?? "";
            int previousSceneType = previousScene?.SceneType ?? 0;
            
            // 发布战斗场景开始事件（可显示 Loading）
            EventSystem.Instance.Publish(root, new BattleSceneChangeStart());
            
            // 创建战斗场景
            Scene battleScene = BattleSceneFactory.Create(
                IdGenerater.Instance.GenerateId(), 
                "Battle", 
                currentScenesComponent,
                previousSceneId,
                previousSceneName,
                previousSceneType);
            
            // 添加战斗组件
            BattleSceneComponent battle = battleScene.AddComponent<BattleSceneComponent>();
            
            // 初始化三消棋盘
            await InitializeMatch3BoardAsync(battleScene, levelId);
            
            // 开始战斗
            await battle.StartBattle(levelId);
            
            // 发布战斗场景完成事件（可隐藏 Loading）
            EventSystem.Instance.Publish(battleScene, new BattleSceneChangeFinish());
        }

        /// <summary>
        /// 初始化三消棋盘
        /// 参考 Match3BoardViewInitSystem.InitializeBoardViewAsync
        /// </summary>
        /// <param name="scene">战斗场景</param>
        /// <param name="levelId">关卡ID</param>
        private static async ETTask InitializeMatch3BoardAsync(Scene scene, int levelId)
        {
            Log.Info($"[BattleSceneHelper] 开始初始化三消棋盘，关卡ID: {levelId}");
            
            // 1. 添加关卡加载器组件
            LevelLoaderComponent levelLoader = scene.AddComponent<LevelLoaderComponent>();
            
            // 2. 添加瓦片对象池组件
            TilePoolComponent tilePool = scene.AddComponent<TilePoolComponent>();
            
            // 3. 添加三消棋盘组件
            Match3BoardComponent board = scene.AddComponent<Match3BoardComponent>();
            
            // 4. 添加输入处理组件（作为棋盘的子组件）
            board.AddComponent<Match3InputComponent>();
            
            // 5. 添加道具管理器组件
            BoosterManagerComponent boosterManager = scene.AddComponent<BoosterManagerComponent>();
            
            // 6. 加载关卡数据
            Level level = await levelLoader.LoadLevelAsync(levelId);
            if (level.Id == 0 && level.Width == 0)
            {
                Log.Error($"[BattleSceneHelper] 加载关卡失败，关卡ID: {levelId}");
                return;
            }
            
            // 7. 设置关卡数据到棋盘
            board.LoadLevel(level);
            
            // 8. 初始化棋盘视图（创建瓦片等）
            await board.InitializeBoardViewAsync(level);
            
            // 9. 初始化输入组件参数
            var inputComponent = board.GetComponent<Match3InputComponent>();
            if (inputComponent != null)
            {
                // 获取瓦片大小（与视图初始化一致）
                float tileSize = 1.0f;
                if (tilePool.LightBgTilePrefab != null)
                {
                    var spriteRenderer = tilePool.LightBgTilePrefab.GetComponent<UnityEngine.SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        tileSize = spriteRenderer.bounds.size.x;
                    }
                }
                
                // 初始化输入组件（坐标转换现在由WorldToBoardPosition根据棋盘尺寸自动计算）
                inputComponent.Initialize(
                    UnityEngine.Camera.main,
                    null,  // 棋盘Transform暂时不需要
                    tileSize,
                    UnityEngine.Vector2.zero  // BoardOffset不再使用，坐标转换自动计算
                );
                
                Log.Info($"[BattleSceneHelper] 输入组件初始化完成，瓦片大小: {tileSize}");
            }
            
            // 10. 初始化道具信息
            if (level.AvailableBoosters != null)
            {
                foreach (var kvp in level.AvailableBoosters)
                {
                    if (kvp.Value)
                    {
                        // 初始化可用道具（默认数量为3）
                        boosterManager.AddBooster(kvp.Key, 3);
                    }
                }
            }
            
            Log.Info($"[BattleSceneHelper] 三消棋盘初始化完成，宽度: {level.Width}，高度: {level.Height}");
        }

        /// <summary>
        /// 退出战斗，返回之前的场景
        /// </summary>
        /// <param name="root">根场景</param>
        public static async ETTask ExitBattleAsync(Scene root)
        {
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene battleScene = currentScenesComponent.Scene;
            
            if (battleScene == null)
            {
                Log.Error("当前没有战斗场景");
                return;
            }
            
            // 获取之前场景的信息
            BattlePreviousSceneComponent previousSceneInfo = battleScene.GetComponent<BattlePreviousSceneComponent>();
            if (previousSceneInfo == null)
            {
                Log.Error("找不到之前场景的信息，释放战斗场景");
            
                // 发布退出战斗场景开始事件（可关闭战斗界面）
                await EventSystem.Instance.PublishAsync(battleScene, new BattleSceneExitStart());
                
                battleScene.Dispose();
                return;
            }
            
            long previousSceneId = previousSceneInfo.PreviousSceneId;
            string previousSceneName = previousSceneInfo.PreviousSceneName;
            int previousSceneType = previousSceneInfo.PreviousSceneType;
            
            // 重新创建之前的场景
            Scene previousScene = EntitySceneFactory.CreateScene(
                currentScenesComponent,
                previousSceneId,
                IdGenerater.Instance.GenerateInstanceId(),
                previousSceneType,
                previousSceneName);
            currentScenesComponent.Scene = previousScene;
            
            // 发布场景切换完成事件
            EventSystem.Instance.Publish(previousScene, new SceneChangeFinish());
            
            // 发布退出战斗场景开始事件（可关闭战斗界面）
            await EventSystem.Instance.PublishAsync(battleScene, new BattleSceneExitStart());
            
            // 释放战斗场景
            battleScene.Dispose();
            
            await ETTask.CompletedTask;
        }
    }
}
