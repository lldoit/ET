using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2025.12.24
    /// Desc
    /// </summary>
    [FriendOf(typeof(RankingGlobalViewComponent))]
    public static partial class RankingGlobalViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RankingGlobalViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RankingGlobalViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this RankingGlobalViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
