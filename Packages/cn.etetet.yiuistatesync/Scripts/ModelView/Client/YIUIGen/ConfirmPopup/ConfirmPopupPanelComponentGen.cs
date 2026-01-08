using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Popup)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class ConfirmPopupPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "ConfirmPopup";
        public const string ResName = "ConfirmPopupPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public YIUIFramework.UIDataValueString u_DataTitle;
        public YIUIFramework.UIDataValueString u_DataContent;
        public UIEventP0 u_EventCancel;
        public UIEventHandleP0 u_EventCancelHandle;
        public const string OnEventCancelInvoke = "ConfirmPopupPanelComponent.OnEventCancelInvoke";
        public UITaskEventP0 u_EventOk;
        public UITaskEventHandleP0 u_EventOkHandle;
        public const string OnEventOkInvoke = "ConfirmPopupPanelComponent.OnEventOkInvoke";

    }
}