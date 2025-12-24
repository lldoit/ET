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
    [FriendOf(typeof(RankingPanelComponent))]
    public static partial class RankingPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RankingPanelComponent self)
        {
            self.u_DataViewTable.AddValueChangeAction(ViewChangeAction);
        }

        [EntitySystem]
        private static void Destroy(this RankingPanelComponent self)
        {
            self.u_DataViewTable.RemoveValueChangeAction(ViewChangeAction);
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this RankingPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }
        
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this RankingPanelComponent self, ERankingPanelViewEnum param1)
        {
            self.u_DataViewTable.SetValue((int)param1, true, false);

            await self.UIPanel.OpenViewAsync(param1.ToString());
            
            return true;
        }

        private static void ViewChangeAction(int newValue, int oldValue)
        {
            Fiber.Instance.Root.YIUIMgr().GetPanel<RankingPanelComponent>()
                .UIPanel.OpenViewAsync(((ERankingPanelViewEnum)newValue).ToString()).NoContext();
        }

        #region YIUIEvent开始

        [YIUIInvoke(RankingPanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this RankingPanelComponent self)
        {
            self.YIUIMgr().HomePanel<MainPanelComponent>().NoContext();
        }
        #endregion YIUIEvent结束
    }
}
