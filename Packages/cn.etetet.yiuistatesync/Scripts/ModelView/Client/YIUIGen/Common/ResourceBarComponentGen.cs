using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Common)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class ResourceBarComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize
    {
        public const string PkgName = "Common";
        public const string ResName = "ResourceBar";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public UIEventP0 u_EventAddEnergy;
        public UIEventHandleP0 u_EventAddEnergyHandle;
        public const string OnEventAddEnergyInvoke = "ResourceBarComponent.OnEventAddEnergyInvoke";
        public UIEventP0 u_EventAddGem;
        public UIEventHandleP0 u_EventAddGemHandle;
        public const string OnEventAddGemInvoke = "ResourceBarComponent.OnEventAddGemInvoke";
        public UIEventP0 u_EventAddCoin;
        public UIEventHandleP0 u_EventAddCoinHandle;
        public const string OnEventAddCoinInvoke = "ResourceBarComponent.OnEventAddCoinInvoke";
        public UIEventP0 u_EventAddDiamond;
        public UIEventHandleP0 u_EventAddDiamondHandle;
        public const string OnEventAddDiamondInvoke = "ResourceBarComponent.OnEventAddDiamondInvoke";

    }
}