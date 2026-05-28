using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.5.15
    /// Desc
    /// </summary>
    [FriendOf(typeof(HomePanelComponent))]
    public static partial class HomePanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this HomePanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this HomePanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this HomePanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始

        [YIUIInvoke(HomePanelComponent.OnEventStageInvoke)]
        private static async ETTask OnEventStageInvoke(this HomePanelComponent self)
        {
            await self.YIUIMgr().Root.OpenPanelAsync<StagePanelComponent>();
        }
        #endregion YIUIEvent结束
    }
}
