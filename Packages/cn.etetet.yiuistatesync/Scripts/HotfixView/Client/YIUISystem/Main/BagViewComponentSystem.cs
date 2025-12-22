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
    [FriendOf(typeof(BagViewComponent))]
    public static partial class BagViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BagViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this BagViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this BagViewComponent self)
        {
            await ETTask.CompletedTask;

            EventSystem.Instance?.YIUIInvokeEntitySync(self, new UIInvokeMainPanel_ShowHideTab
            {
                ShowTab = false
            });
            
            return true;
        }
        
        [EntitySystem]
        private static async ETTask<bool> YIUIClose(this ET.Client.BagViewComponent self)
        {
            await ETTask.CompletedTask;

            EventSystem.Instance?.YIUIInvokeEntitySync(self, new UIInvokeMainPanel_ShowHideTab
            {
                ShowTab = true
            });

            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(BagViewComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this BagViewComponent self)
        {
            EventSystem.Instance?.YIUIInvokeEntitySync(self, new UIInvokeMainPanel_BackLobby{});
        }
        #endregion YIUIEvent结束
    }
}
