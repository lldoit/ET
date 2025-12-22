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
    [FriendOf(typeof(YIUIWindowComponent))]
    [FriendOf(typeof(YIUIViewComponent))]
    [EntitySystemOf(typeof(ShopViewComponent))]
    public static partial class ShopViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ShopViewComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this ShopViewComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this ShopViewComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIView = self.UIBase.GetComponent<YIUIViewComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIView.ViewWindowType = EViewWindowType.View;
            self.UIView.StackOption = EViewStackOption.VisibleTween;

            self.u_EventBack = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventBack");
            self.u_EventBackHandle = self.u_EventBack.Add(self,ShopViewComponent.OnEventBackInvoke);

        }
    }
}
