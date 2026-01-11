using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.1.11
    /// Desc
    /// </summary>
    [FriendOf(typeof(Match3BoardPanelComponent))]
    public static partial class Match3BoardPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this Match3BoardPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this Match3BoardPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this Match3BoardPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
