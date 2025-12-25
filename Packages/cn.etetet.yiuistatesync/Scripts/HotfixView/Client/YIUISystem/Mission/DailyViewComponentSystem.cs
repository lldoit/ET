using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2025.12.25
    /// Desc
    /// </summary>
    [FriendOf(typeof(DailyViewComponent))]
    [FriendOf(typeof(MissionItemComponent))]
    public static partial class DailyViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this DailyViewComponent self)
        {
            self.m_Loop = self.AddChild<YIUILoopScrollChild, LoopScrollRect, Type>(self.u_ComLoopScrollVertical,
                typeof(MissionItemComponent));
        }

        [EntitySystem]
        private static void Destroy(this DailyViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this DailyViewComponent self)
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
        private static void YIUILoopRenderer(this DailyViewComponent self, MissionItemComponent item, int data, 
            int index, bool select)
        {
            switch (index)
            {
                case 0:
                    item.u_DataItemColor.SetValue(new Color32(125, 87, 242, 255));
                    break;
                
                case 1:
                    item.u_DataItemColor.SetValue(new Color32(106, 69, 147, 255));
                    break;
                
                default:
                    item.u_DataItemColor.SetValue(new Color32(62, 52, 92, 255));
                    break;
            }
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
