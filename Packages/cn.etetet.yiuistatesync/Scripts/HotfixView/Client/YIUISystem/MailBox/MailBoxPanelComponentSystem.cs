using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.05.03
    /// Desc
    /// </summary>
    [FriendOf(typeof(MailBoxPanelComponent))]
    [FriendOf(typeof(MailBoxItemComponent))]
    public static partial class MailBoxPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this MailBoxPanelComponent self)
        {
            self.m_Loop = self.AddChild<YIUILoopScrollChild, LoopScrollRect, Type>(self.u_ComLoopScrollVertical,
                typeof(MailBoxItemComponent));
        }

        [EntitySystem]
        private static void Destroy(this MailBoxPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MailBoxPanelComponent self)
        {
            await ETTask.CompletedTask;

            List<int> list = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                list.Add(i);
            }

            self.Loop.ClearSelect();
            self.Loop.SetDataRefresh(list, 0).NoContext();

            return true;
        }

        [EntitySystem]
        private static void YIUILoopRenderer(this MailBoxPanelComponent self, MailBoxItemComponent item, int data,
        int index, bool select)
        {
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(MailBoxPanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this MailBoxPanelComponent self)
        {
            self.YIUIMgr().ClosePanel<MailBoxPanelComponent>();
        }
        #endregion YIUIEvent结束
    }
}
