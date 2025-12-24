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
    [FriendOf(typeof(RankingItemComponent))]
    public static partial class RankingItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RankingItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RankingItemComponent self)
        {
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(RankingItemComponent.OnEventSelectInvoke)]
        private static void OnEventSelectInvoke(this RankingItemComponent self)
        {

        }
        #endregion YIUIEvent结束
    }
}
