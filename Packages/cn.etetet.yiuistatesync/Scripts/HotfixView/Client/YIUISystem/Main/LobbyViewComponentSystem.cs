using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;
using Sirenix.OdinInspector;

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
        
        [YIUIInvoke(LobbyViewComponent.OnEventMenuInvoke)]
        private static void OnEventMenuInvoke(this LobbyViewComponent self)
        {
            self.u_ComMenuRectTransform.gameObject.SetActive(true);
        }
        
        [YIUIInvoke(LobbyViewComponent.OnEventHideMenuInvoke)]
        private static void OnEventHideMenuInvoke(this LobbyViewComponent self)
        {
            self.u_ComMenuRectTransform.gameObject.SetActive(false);
        }
        
        [YIUIInvoke(LobbyViewComponent.OnEventStageInvoke)]
        private static async ETTask OnEventStageInvoke(this LobbyViewComponent self)
        {
            await self.YIUIMgr().Root.OpenPanelAsync<StagePanelComponent>();
        }
        #endregion YIUIEvent结束
    }
}
