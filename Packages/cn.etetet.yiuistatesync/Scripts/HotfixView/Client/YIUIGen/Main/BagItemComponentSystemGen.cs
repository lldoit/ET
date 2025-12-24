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
    [EntitySystemOf(typeof(BagItemComponent))]
    public static partial class BagItemComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BagItemComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this BagItemComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this BagItemComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

            self.u_DataBackIcon = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataBackIcon");
            self.u_DataItemIcon = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataItemIcon");
            self.u_DataCount = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueInt>("u_DataCount");
            self.u_EventSelect = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventSelect");
            self.u_EventSelectHandle = self.u_EventSelect.Add(self,BagItemComponent.OnEventSelectInvoke);

        }
    }
}
