using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.View)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class LobbyViewComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Main";
        public const string ResName = "LobbyView";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIViewComponent> u_UIView;
        public YIUIViewComponent UIView => u_UIView;
        public UnityEngine.RectTransform u_ComMenuRectTransform;
        public UIEventP0 u_EventMenu;
        public UIEventHandleP0 u_EventMenuHandle;
        public const string OnEventMenuInvoke = "LobbyViewComponent.OnEventMenuInvoke";
        public UIEventP0 u_EventHideMenu;
        public UIEventHandleP0 u_EventHideMenuHandle;
        public const string OnEventHideMenuInvoke = "LobbyViewComponent.OnEventHideMenuInvoke";
        public UITaskEventP0 u_EventStage;
        public UITaskEventHandleP0 u_EventStageHandle;
        public const string OnEventStageInvoke = "LobbyViewComponent.OnEventStageInvoke";
        public UITaskEventP0 u_EventRanking;
        public UITaskEventHandleP0 u_EventRankingHandle;
        public const string OnEventRankingInvoke = "LobbyViewComponent.OnEventRankingInvoke";

    }
}