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
    [EntitySystemOf(typeof(MissionItemComponent))]
    public static partial class MissionItemComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MissionItemComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this MissionItemComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this MissionItemComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

            self.u_DataItemColor = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueColor>("u_DataItemColor");

        }
    }
}
