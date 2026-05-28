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
    public partial class CrawlersPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Crawlers";
        public const string ResName = "CrawlersPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public ET.Client.CrawlerHandView u_ComHandView;
        public YIUIFramework.UIDataValueString u_DataTurnStatus;
        public YIUIFramework.UIDataValueString u_DataBattleSummary;
        public YIUIFramework.UIDataValueString u_DataMana;
        public YIUIFramework.UIDataValueString u_DataPlayerHp;
        public YIUIFramework.UIDataValueString u_DataDrawPile;
        public YIUIFramework.UIDataValueString u_DataDiscardPile;
        public YIUIFramework.UIDataValueString u_DataPlayerShield;

    }
}
