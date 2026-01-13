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
    [EntitySystemOf(typeof(BattlePanelComponent))]
    public static partial class BattlePanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattlePanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this BattlePanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this BattlePanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.BanOpenTween|EWindowOption.BanCloseTween;
            self.UIPanel.Layer = EPanelLayer.Panel;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 100;
            self.UIPanel.CachePanelTime = 10;

            self.u_ComBoardCenterTransform = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComBoardCenterTransform");
            self.u_EventBack = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventBack");
            self.u_EventBackHandle = self.u_EventBack.Add(self,BattlePanelComponent.OnEventBackInvoke);

        }
    }
}
