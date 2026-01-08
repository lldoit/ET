using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.1.8
    /// Desc    确认弹窗面板
    /// </summary>
    public partial class ConfirmPopupPanelComponent : Entity, IYIUIOpen<string, string, string>
    {
        /// <summary>
        /// 确认弹窗来源标识，用于在确认后发布对应事件
        /// </summary>
        public string ConfirmSource;
    }
}
