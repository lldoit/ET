using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.12.22
    /// Desc
    /// </summary>
    [FriendOf(typeof(ShopViewComponent))]
    public static partial class ShopViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this ShopViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ShopViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ShopViewComponent self)
        {
            await ETTask.CompletedTask;

            EventSystem.Instance?.YIUIInvokeEntitySync(self, new UIInvokeMainPanel_ShowHideTab
            {
                ShowTab = false
            });

            return true;
        }
        
        [EntitySystem]
        private static async ETTask<bool> YIUIClose(this ET.Client.ShopViewComponent self)
        {
            await ETTask.CompletedTask;

            EventSystem.Instance?.YIUIInvokeEntitySync(self, new UIInvokeMainPanel_ShowHideTab
            {
                ShowTab = true
            });

            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(ShopViewComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this ShopViewComponent self)
        {
            EventSystem.Instance?.YIUIInvokeEntitySync(self, new UIInvokeMainPanel_BackLobby{});
        }
        #endregion YIUIEvent结束
    }
}
