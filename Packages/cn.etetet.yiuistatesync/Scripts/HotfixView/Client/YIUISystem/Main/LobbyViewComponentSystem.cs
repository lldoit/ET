using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.12.21
    /// Desc
    /// </summary>
    [FriendOf(typeof(LobbyViewComponent))]
    public static partial class LobbyViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LobbyViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LobbyViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LobbyViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
