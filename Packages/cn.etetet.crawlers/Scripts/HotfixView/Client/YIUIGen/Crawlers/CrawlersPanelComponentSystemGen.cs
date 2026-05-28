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
    [EntitySystemOf(typeof(CrawlersPanelComponent))]
    public static partial class CrawlersPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CrawlersPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this CrawlersPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this CrawlersPanelComponent self)
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

            self.u_ComHandView = self.UIBase.ComponentTable.FindComponent<ET.Client.CrawlerHandView>("u_ComHandView");
            self.u_DataTurnStatus = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataTurnStatus");
            self.u_DataBattleSummary = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataBattleSummary");
            self.u_DataMana = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataMana");
            self.u_DataPlayerHp = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataPlayerHp");
            self.u_DataDrawPile = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataDrawPile");
            self.u_DataDiscardPile = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataDiscardPile");
            self.u_DataPlayerShield = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataPlayerShield");

        }
    }
}
