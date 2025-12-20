using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    [FriendOf(typeof(MainPanelComponent))]
    public static partial class MainPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this MainPanelComponent self)
        {
            self.u_DataViewTable.AddValueChangeAction(ViewTableValueChange);
        }

        [EntitySystem]
        private static void Destroy(this MainPanelComponent self)
        {
            self.u_DataViewTable.RemoveValueChangeAction(ViewTableValueChange);
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MainPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        public static async ETTask<bool> YIUIOpen(this MainPanelComponent self, EMainPanelViewEnum a)
        {
            self.u_DataViewTable.SetValue((int)a, false, false);

            await self.UIPanel.OpenViewAsync(a.ToString());

            return true;
        }

        private static void ViewTableValueChange(int arg1, int arg2)
        {
            throw new NotImplementedException();
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
