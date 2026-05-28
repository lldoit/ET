using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.5.18
    /// Desc
    /// </summary>
    [FriendOf(typeof(CrawlerCardComponent))]
    public static partial class CrawlerCardComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this CrawlerCardComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this CrawlerCardComponent self)
        {
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
