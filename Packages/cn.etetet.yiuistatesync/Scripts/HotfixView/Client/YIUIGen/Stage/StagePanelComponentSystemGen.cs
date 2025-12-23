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
    [EntitySystemOf(typeof(StagePanelComponent))]
    public static partial class StagePanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this StagePanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this StagePanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this StagePanelComponent self)
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

            self.u_EventBack = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventBack");
            self.u_EventBackHandle = self.u_EventBack.Add(self,StagePanelComponent.OnEventBackInvoke);
            self.u_EventEnterMap = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventEnterMap");
            self.u_EventEnterMapHandle = self.u_EventEnterMap.Add(self,StagePanelComponent.OnEventEnterMapInvoke);

        }
    }
}
