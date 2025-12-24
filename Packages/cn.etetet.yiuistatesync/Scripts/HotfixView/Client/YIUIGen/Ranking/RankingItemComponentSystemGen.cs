using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIChild))]
    [EntitySystemOf(typeof(RankingItemComponent))]
    public static partial class RankingItemComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RankingItemComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this RankingItemComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this RankingItemComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

            self.u_EventSelect = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventSelect");
            self.u_EventSelectHandle = self.u_EventSelect.Add(self,RankingItemComponent.OnEventSelectInvoke);

        }
    }
}
