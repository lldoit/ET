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
    [EntitySystemOf(typeof(LobbyViewComponent))]
    public static partial class LobbyViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LobbyViewComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this LobbyViewComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this LobbyViewComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIView = self.UIBase.GetComponent<YIUIViewComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIView.ViewWindowType = EViewWindowType.View;
            self.UIView.StackOption = EViewStackOption.VisibleTween;

            self.u_ComMenuRectTransform = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComMenuRectTransform");
            self.u_EventMenu = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventMenu");
            self.u_EventMenuHandle = self.u_EventMenu.Add(self,LobbyViewComponent.OnEventMenuInvoke);
            self.u_EventHideMenu = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventHideMenu");
            self.u_EventHideMenuHandle = self.u_EventHideMenu.Add(self,LobbyViewComponent.OnEventHideMenuInvoke);

        }
    }
}
