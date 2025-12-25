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
    [FriendOf(typeof(ResourceBarComponent))]
    public static partial class ResourceBarComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this ResourceBarComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ResourceBarComponent self)
        {
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(ResourceBarComponent.OnEventAddDiamondInvoke)]
        private static void OnEventAddDiamondInvoke(this ResourceBarComponent self)
        {

        }
        
        [YIUIInvoke(ResourceBarComponent.OnEventAddCoinInvoke)]
        private static void OnEventAddCoinInvoke(this ResourceBarComponent self)
        {

        }
        
        [YIUIInvoke(ResourceBarComponent.OnEventAddGemInvoke)]
        private static void OnEventAddGemInvoke(this ResourceBarComponent self)
        {

        }
        
        [YIUIInvoke(ResourceBarComponent.OnEventAddEnergyInvoke)]
        private static void OnEventAddEnergyInvoke(this ResourceBarComponent self)
        {

        }
        #endregion YIUIEvent结束
    }
}
