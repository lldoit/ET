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
            self.u_DataViewTable.AddValueChangeAction(ViewChangeAction);
        }

        [EntitySystem]
        private static void Destroy(this MainPanelComponent self)
        {
            self.u_DataViewTable.RemoveValueChangeAction(ViewChangeAction);
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MainPanelComponent self)
        {
            await self.YIUIOpen(EMainPanelViewEnum.LobbyView);
            return true;
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MainPanelComponent self, EMainPanelViewEnum param1)
        {
            self.u_DataViewTable.SetValue((int)param1, true);

            await self.UIPanel.OpenViewAsync(param1.ToString());

            return true;
        }

        private static void ViewChangeAction(int newValue, int oldValue)
        {
            Fiber.Instance.Root.YIUIMgr().GetPanel<MainPanelComponent>()
                    .UIPanel.OpenViewAsync(((EMainPanelViewEnum)newValue).ToString()).NoContext();
        }

        /// <summary>
        /// 显示隐藏TabMenu
        /// </summary>
        /// <param name="self"></param>
        /// <param name="isShow"></param>
        public static void ShowTab(this MainPanelComponent self, bool isShow)
        {
            self.u_ComTabMenuRectTransform.gameObject.SetActive(isShow);
        }

        /// <summary>
        /// 返回LobbyTab
        /// </summary>
        /// <param name="self"></param>
        public static void BackLobby(this MainPanelComponent self)
        {
            self.u_DataViewTable.SetValue((int)EMainPanelViewEnum.LobbyView);
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
