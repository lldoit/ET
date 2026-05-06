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
    [FriendOf(typeof(YIUIPanelComponent))]
    [EntitySystemOf(typeof(MailBoxPanelComponent))]
    public static partial class MailBoxPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MailBoxPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this MailBoxPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this MailBoxPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Panel;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;
            self.UIPanel.CachePanelTime = 10;

            self.u_ComLoopScrollVertical = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.LoopVerticalScrollRect>("u_ComLoopScrollVertical");
            self.u_EventBack = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventBack");
            self.u_EventBackHandle = self.u_EventBack.Add(self,MailBoxPanelComponent.OnEventBackInvoke);

        }
    }
}
