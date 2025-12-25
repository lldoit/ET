using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.12.25
    /// Desc
    /// </summary>
    [FriendOf(typeof(ClanPanelComponent))]
    [FriendOf(typeof(ClanItemComponent))]
    public static partial class ClanPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this ClanPanelComponent self)
        {
            self.m_Loop = self.AddChild<YIUILoopScrollChild, LoopScrollRect, Type, string>(self.u_ComLoopScrollVertical,
                typeof(ClanItemComponent), "u_EventSelect");
        }

        [EntitySystem]
        private static void Destroy(this ClanPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ClanPanelComponent self)
        {
            await ETTask.CompletedTask;
            
            List<int> list = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(i);
            }

            self.Loop.ClearSelect();
            self.Loop.SetDataRefresh(list, 0).NoContext();
            
            return true;
        }

        [EntitySystem]
        private static void YIUILoopRenderer(this ClanPanelComponent self, ClanItemComponent item, int data, 
        int index, bool select)
        {
            item.u_DataSelectedState.SetValue(select);
        }

        [EntitySystem]
        private static void YIUILoopOnClick(this ClanPanelComponent self, ClanItemComponent item, int data, int index,
        bool select)
        {
            item.u_DataSelectedState.SetValue(select);
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(ClanPanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this ClanPanelComponent self)
        {
            self.YIUIMgr().HomePanel<MainPanelComponent>().NoContext();
        }
        #endregion YIUIEvent结束
    }
}
