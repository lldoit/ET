using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2025.12.24
    /// Desc
    /// </summary>
    [FriendOf(typeof(RankingClanViewComponent))]
    [FriendOf(typeof(RankingItemComponent))]
    public static partial class RankingClanViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RankingClanViewComponent self)
        {
            self.m_Loop = self.AddChild<YIUILoopScrollChild, LoopScrollRect, Type, string>(self.u_ComLoopScrollVertical,
                typeof(RankingItemComponent), "u_EventSelect");
        }

        [EntitySystem]
        private static void Destroy(this RankingClanViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this RankingClanViewComponent self)
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
        private static void YIUILoopRenderer(this RankingClanViewComponent self, RankingItemComponent item, int data, 
            int index, bool select)
        {
            
        }

        [EntitySystem]
        private static void YIUILoopOnClick(this RankingClanViewComponent self, RankingItemComponent item, int data, int index,
            bool select)
        {
            
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
