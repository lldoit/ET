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
    [FriendOf(typeof(MissionPanelComponent))]
    public static partial class MissionPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this MissionPanelComponent self)
        {
            self.u_DataViewTable.AddValueChangeAction(ViewChangeAction);
        }

        [EntitySystem]
        private static void Destroy(this MissionPanelComponent self)
        {
            self.u_DataViewTable.RemoveValueChangeAction(ViewChangeAction);
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MissionPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }
        
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ET.Client.MissionPanelComponent self, ET.Client.EMissionPanelViewEnum param1)
        {
            self.u_DataViewTable.SetValue((int)param1, true, false);

            await self.UIPanel.OpenViewAsync(param1.ToString());

            return true;
        }

        private static void ViewChangeAction(int newValue, int oldValue)
        {
            Fiber.Instance.Root.YIUIMgr().GetPanel<MissionPanelComponent>()
                .UIPanel.OpenViewAsync(((EMissionPanelViewEnum)newValue).ToString()).NoContext();
        }

        #region YIUIEvent开始

        [YIUIInvoke(MissionPanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this MissionPanelComponent self)
        {
            self.YIUIMgr().HomePanel<MainPanelComponent>().NoContext();
        }
        #endregion YIUIEvent结束
    }
}
