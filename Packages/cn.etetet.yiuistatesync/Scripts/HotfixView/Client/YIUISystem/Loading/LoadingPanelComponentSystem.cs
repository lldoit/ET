using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.1.7
    /// Desc
    /// </summary>
    [FriendOf(typeof(LoadingPanelComponent))]
    public static partial class LoadingPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LoadingPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoadingPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LoadingPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
