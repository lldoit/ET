using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.12.23
    /// Desc
    /// </summary>
    [FriendOf(typeof(StagePanelComponent))]
    public static partial class StagePanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this StagePanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this StagePanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this StagePanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(StagePanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this StagePanelComponent self)
        {
            self.YIUIMgr().HomePanel<MainPanelComponent>().NoContext();
        }
        
        [YIUIInvoke(StagePanelComponent.OnEventEnterMapInvoke)]
        private static async ETTask OnEventEnterMapInvoke(this StagePanelComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
