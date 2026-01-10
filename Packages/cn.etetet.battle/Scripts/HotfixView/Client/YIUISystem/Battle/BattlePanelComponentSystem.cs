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
    /// </summary>
    [FriendOf(typeof(BattlePanelComponent))]
    [FriendOf(typeof(Match3InputComponent))]
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
            // 使用 u_ComBoardCenterTransform 的世界坐标设置 BoardRoot 位置
            // BoardRoot 保持在世界空间独立渲染，不作为 UI 子节点
            if (self.u_ComBoardCenterTransform != null)
            {
                var currentScenesComponent = self.Root().GetComponent<CurrentScenesComponent>();
                var battleScene = currentScenesComponent?.Scene;
                var tilePool = battleScene?.GetComponent<TilePoolComponent>();
                if (tilePool != null)
                {
                    tilePool.SetBoardRootPosition(self.u_ComBoardCenterTransform.position, 0.4f);
                }
                
                // 获取 Canvas 的相机并设置给 Match3InputComponent
                // 因为棋盘在 UI 坐标系中，需要使用 UI 相机进行射线检测
                var canvas = self.u_ComBoardCenterTransform.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    Camera uiCamera = canvas.worldCamera;
                    if (uiCamera == null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        // ScreenSpaceOverlay 模式使用 Camera.main
                        uiCamera = Camera.main;
                    }
                    
                    var board = battleScene?.GetComponent<Match3BoardComponent>();
                    var inputComponent = board?.GetComponent<Match3InputComponent>();
                    if (inputComponent != null && uiCamera != null)
                    {
                        inputComponent.GameCamera = uiCamera;
                        Log.Info($"[BattlePanel] 设置 Match3InputComponent 相机: {uiCamera.name}");
                    }
                }
            }
            
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(BattlePanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this BattlePanelComponent self)
        {
            BattleSceneHelper.ExitBattleAsync(self.Root()).NoContext();
            self.YIUIMgr().ClosePanel<BattlePanelComponent>();;
            /*// 打开确认弹窗，使用来源标识
            self.YIUIMgr().Root.OpenPanelAsync<ConfirmPopupPanelComponent, string, string, string>(
                "Title",
                "Exit?",
                ConfirmSource_ExitBattle).NoContext();*/
        }
        #endregion YIUIEvent结束
    }
}
