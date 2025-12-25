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
    [EntitySystemOf(typeof(ClanItemComponent))]
    public static partial class ClanItemComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ClanItemComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this ClanItemComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this ClanItemComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

            self.u_DataSelectedState = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataSelectedState");
            self.u_EventSelect = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventSelect");
            self.u_EventSelectHandle = self.u_EventSelect.Add(self,ClanItemComponent.OnEventSelectInvoke);

        }
    }
}
