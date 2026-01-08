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
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(BattlePanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this BattlePanelComponent self)
        {
            // 打开确认弹窗，使用来源标识
            self.YIUIMgr().Root.OpenPanelAsync<ConfirmPopupPanelComponent, string, string, string>(
                "Title",
                "Exit?",
                ConfirmSource_ExitBattle).NoContext();
        }
        #endregion YIUIEvent结束
    }
}
