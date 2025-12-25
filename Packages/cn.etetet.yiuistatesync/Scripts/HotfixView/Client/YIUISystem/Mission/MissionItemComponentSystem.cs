using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2025.12.25
    /// Desc
    /// </summary>
    [FriendOf(typeof(MissionItemComponent))]
    public static partial class MissionItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this MissionItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this MissionItemComponent self)
        {
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
