using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.12.24
    /// Desc
    /// </summary>
    [FriendOf(typeof(BagItemComponent))]
    public static partial class BagItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BagItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this BagItemComponent self)
        {
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(BagItemComponent.OnEventSelectInvoke)]
        private static void OnEventSelectInvoke(this BagItemComponent self)
        {

        }
        #endregion YIUIEvent结束
    }
}
