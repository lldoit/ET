using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.12.25
    /// Desc
    /// </summary>
    [FriendOf(typeof(ClanItemComponent))]
    public static partial class ClanItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this ClanItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ClanItemComponent self)
        {
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(ClanItemComponent.OnEventSelectInvoke)]
        private static void OnEventSelectInvoke(this ClanItemComponent self)
        {
            
        }
        #endregion YIUIEvent结束
    }
}
