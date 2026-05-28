using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Panel)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class StagePanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Stage";
        public const string ResName = "StagePanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UIEventP0 u_EventBack;
        public UIEventHandleP0 u_EventBackHandle;
        public const string OnEventBackInvoke = "StagePanelComponent.OnEventBackInvoke";
        public UITaskEventP0 u_EventEnterMap;
        public UITaskEventHandleP0 u_EventEnterMapHandle;
        public const string OnEventEnterMapInvoke = "StagePanelComponent.OnEventEnterMapInvoke";

    }
}