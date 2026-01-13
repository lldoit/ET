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

            // UI渲染模式初始化
            await self.InitializeUIRenderMode(battleScene, board);

            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 初始化UI渲染模式
        /// </summary>
        private static async ETTask InitializeUIRenderMode(this BattlePanelComponent self, Scene battleScene, Match3BoardComponent board)
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

            Log.Info("[BattlePanel] UI渲染模式初始化完成");
        }

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
