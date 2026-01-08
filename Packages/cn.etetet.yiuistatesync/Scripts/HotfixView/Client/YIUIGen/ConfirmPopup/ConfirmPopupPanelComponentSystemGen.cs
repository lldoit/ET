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
    [EntitySystemOf(typeof(ConfirmPopupPanelComponent))]
    public static partial class ConfirmPopupPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ConfirmPopupPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this ConfirmPopupPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this ConfirmPopupPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.BanOpenTween;
            self.UIPanel.Layer = EPanelLayer.Popup;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;
            self.UIPanel.CachePanelTime = 10;

            self.u_DataTitle = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataTitle");
            self.u_DataContent = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataContent");
            self.u_EventCancel = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventCancel");
            self.u_EventCancelHandle = self.u_EventCancel.Add(self,ConfirmPopupPanelComponent.OnEventCancelInvoke);
            self.u_EventOk = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventOk");
            self.u_EventOkHandle = self.u_EventOk.Add(self,ConfirmPopupPanelComponent.OnEventOkInvoke);

        }
    }
}
