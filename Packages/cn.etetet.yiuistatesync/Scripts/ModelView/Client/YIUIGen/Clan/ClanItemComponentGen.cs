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
    public partial class ClanItemComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize
    {
        public const string PkgName = "Clan";
        public const string ResName = "ClanItem";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public YIUIFramework.UIDataValueBool u_DataSelectedState;
        public UIEventP0 u_EventSelect;
        public UIEventHandleP0 u_EventSelectHandle;
        public const string OnEventSelectInvoke = "ClanItemComponent.OnEventSelectInvoke";

    }
}