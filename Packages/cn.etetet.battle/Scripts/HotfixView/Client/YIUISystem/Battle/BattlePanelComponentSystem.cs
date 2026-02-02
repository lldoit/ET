using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.1.8
    /// Desc    战斗界面面板系统
    /// UI空间渲染模式
    /// </summary>
    [FriendOf(typeof(BattlePanelComponent))]
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Match3InputComponent))]
    [FriendOf(typeof(TilePoolComponent))]
    [FriendOf(typeof(FxPoolComponent))]
    public static partial class BattlePanelComponentSystem
    {
        /// <summary>
        /// 退出战斗确认来源标识
        /// </summary>
        public const string ConfirmSource_ExitBattle = "ExitBattle";

        [EntitySystem]
        private static void YIUIInitialize(this BattlePanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this BattlePanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this BattlePanelComponent self)
        {
            var currentScenesComponent = self.Root().GetComponent<CurrentScenesComponent>();
            var battleScene = currentScenesComponent?.Scene;
            var board = battleScene?.GetComponent<Match3BoardComponent>();

            if (board == null)
            {
                Log.Warning("[BattlePanel] Match3BoardComponent 未找到");
                await ETTask.CompletedTask;
                return true;
            }
            
            // UI渲染模式初始化 - 分层初始化
            await self.InitializeUIRenderMode(battleScene, board);

            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 初始化UI渲染模式的三个主要区域
        /// </summary>
        private static async ETTask InitializeUIRenderMode(this BattlePanelComponent self, Scene battleScene, Match3BoardComponent board)
        {
            // 1. 初始化顶部战斗区 (Top - Battle Arena)
            await self.InitializeTopArea(battleScene);

            // 2. 初始化中部信息区 (Middle - Info Bar)
            await self.InitializeMiddleArea(battleScene);

            // 3. 初始化底部三消区 (Bottom - Match-3 Board)
            await self.InitializeBottomArea(battleScene, board);

            Log.Info("[BattlePanel] 全局UI渲染模式初始化完成");
        }

        #region Region 1: Top Area (Battle Arena)

        private static async ETTask InitializeTopArea(this BattlePanelComponent self, Scene battleScene)
        {
            // 初始化站位组件
            await self.InitializeFormation(battleScene, self.UIBase.OwnerRectTransform);

            // 初始化滚动背景组件
            await self.InitializeScrollingBackground(battleScene);

            // 初始化飘字组件
            await self.InitializeDamageNumber(battleScene);
        }

        /// <summary>
        /// 初始化滚动背景组件
        /// </summary>
        private static async ETTask InitializeScrollingBackground(this BattlePanelComponent self, Scene battleScene)
        {
            // 查找ScrollingBackground MonoBehaviour
            var scrollingBgGO = self.u_ComBackgroundRectTransform;
            if (scrollingBgGO == null)
            {
                Log.Warning("[BattlePanel] 未找到Background节点，跳过滚动背景初始化");
                return;
            }

            var controller = scrollingBgGO.GetComponent<ScrollingBackground>();
            if (controller == null)
            {
                Log.Warning("[BattlePanel] Background节点缺少ScrollingBackground组件");
                return;
            }

            // 添加或获取组件
            var scrollBgComponent = battleScene.GetComponent<ScrollingBackgroundComponent>();
            if (scrollBgComponent == null)
            {
                scrollBgComponent = battleScene.AddComponent<ScrollingBackgroundComponent>();
            }

            // 初始化
            scrollBgComponent.Initialize(controller);
            //scrollBgComponent.StartScrolling();
            Log.Info("[BattlePanel] 滚动背景组件初始化完成");

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 初始化站位组件的UI根节点
        /// </summary>
        private static async ETTask InitializeFormation(this BattlePanelComponent self, Scene battleScene, RectTransform battleRoot)
        {
            // 获取站位组件（在BattleSceneHelper中已添加）
            var formationComponent = battleScene.GetComponent<FormationComponent>();
            if (formationComponent == null)
            {
                Log.Warning("[BattlePanel] 站位组件未找到，可能初始化顺序有问题");
                return;
            }

            // 设置BattleRoot用于坐标转换
            formationComponent.SetBattleRoot(battleRoot);
            Log.Info("[BattlePanel] 站位组件BattleRoot设置完成");

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 初始化飘字组件
        /// </summary>
        private static async ETTask InitializeDamageNumber(this BattlePanelComponent self, Scene battleScene)
        {
            // 添加飘字组件
            var dnComponent = battleScene.GetComponent<DamageNumberComponent>();
            if (dnComponent == null)
            {
                dnComponent = battleScene.AddComponent<DamageNumberComponent>();
            }

            // 获取容器（使用BattlePanel的RectTransform）
            RectTransform container = self.UIBase.OwnerRectTransform;

            // 获取UI相机
            Camera uiCamera = self.YIUIMgr().UICamera;

            // 初始化飘字组件
            await dnComponent.InitializeAsync(container, uiCamera);

            Log.Info("[BattlePanel] 飘字组件初始化完成");
        }

        #endregion

        #region Region 2: Middle Area (Info Bar)

        private static async ETTask InitializeMiddleArea(this BattlePanelComponent self, Scene battleScene)
        {
            // TODO: 初始化回合数显示、目标提示等
            // 由于目前Gen代码中没有特定的InfoBar容器，暂时留空或关联到UIBase
            await ETTask.CompletedTask;
        }

        #endregion

        #region Region 3: Bottom Area (Match-3 Board)

        private static async ETTask InitializeBottomArea(this BattlePanelComponent self, Scene battleScene, Match3BoardComponent board)
        {
            if (self.u_ComBoardCenterTransform == null)
            {
                Log.Warning("[BattlePanel] u_ComBoardCenterTransform 未设置，无法初始化UI渲染模式");
                return;
            }

            // 获取或转换为RectTransform
            var boardRectTransform = self.u_ComBoardCenterTransform.GetComponent<RectTransform>();
            if (boardRectTransform == null)
            {
                Log.Warning("[BattlePanel] BoardCenter 缺少 RectTransform");
                return;
            }

            // 获取Canvas
            var canvas = self.u_ComBoardCenterTransform.GetComponentInParent<Canvas>();

            // 获取TilePoolComponent
            var uiTilePool = battleScene.GetComponent<TilePoolComponent>();

            // 设置UITilePool的根节点
            uiTilePool.BoardRoot = boardRectTransform;
            uiTilePool.TileSize = new Vector2(Match3RenderConfig.UITileSize, Match3RenderConfig.UITileSize);
            uiTilePool.TileSpacing = new Vector2(Match3RenderConfig.UITileSpacing, Match3RenderConfig.UITileSpacing);

            // 初始化UITilePool
            await uiTilePool.InitializeAsync();

            // 创建UI棋盘视图（瓦片）
            await board.InitializeBoardUIViewAsync(board.Level);

            // 设置Match3InputComponent的UI相关字段
            var inputComponent = board?.GetComponent<Match3InputComponent>();
            if (inputComponent != null)
            {
                inputComponent.InitializeUI(
                    uiTilePool.TileContainer,
                    canvas,
                    uiTilePool.TileSize,
                    uiTilePool.TileSpacing
                );
                Log.Info("[BattlePanel] UI模式 - 输入组件初始化完成");
            }

            // 初始化特效池
            var uiFxPool = board.GetComponent<FxPoolComponent>();

            // 设置特效容器
            uiFxPool.FxContainer = boardRectTransform;
            await uiFxPool.InitializeAsync();
        }

        #endregion

        #region YIUIEvent开始

        [YIUIInvoke(BattlePanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this BattlePanelComponent self)
        {
            BattleSceneHelper.ExitBattleAsync(self.Root()).NoContext();
            self.YIUIMgr().ClosePanel<BattlePanelComponent>(); ;
        }
        #endregion YIUIEvent结束
    }
}
