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
    [EntitySystemOf(typeof(MailBoxItemComponent))]
    public static partial class MailBoxItemComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MailBoxItemComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this MailBoxItemComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this MailBoxItemComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

        }
    }
}
