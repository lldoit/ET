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
    [FriendOf(typeof(HeroesViewComponent))]
    public static partial class HeroesViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this HeroesViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this HeroesViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this HeroesViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
