using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.12.22
    /// Desc
    /// </summary>
    [FriendOf(typeof(BagViewComponent))]
    [FriendOf(typeof(BagItemComponent))]
    public static partial class BagViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this BagViewComponent self)
        {
            self.m_Loop = self.AddChild<YIUILoopScrollChild, LoopScrollRect, Type, string>(self.u_ComLoopScrollVerticalGroup,
                typeof(BagItemComponent), "u_EventSelect");
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
            
            List<int> list = new List<int>();
            for (int i = 0; i < 50; i++)
            {
                list.Add(i);
            }

            self.Loop.ClearSelect();
            self.Loop.SetDataRefresh(list, 0).NoContext();
            
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

        [EntitySystem]
        private static void YIUILoopRenderer(this BagViewComponent self, BagItemComponent item, int data, 
        int index, bool select)
        {
            item.u_DataCount.SetValue(index);
            //item.u_DataSelect.SetValue(select);
        }

        [EntitySystem]
        private static void YIUILoopOnClick(this BagViewComponent self, BagItemComponent item, int data, int index,
        bool select)
        {
            //item.u_DataSelect.SetValue(select);
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
