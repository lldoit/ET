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
    public partial class CrawlerCardComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize
    {
        public const string PkgName = "Crawlers";
        public const string ResName = "CrawlerCardConfig";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public ET.Client.CrawlerCardView u_ComCardView;
        public ET.Client.CrawlerCardInput u_ComCardInput;
        public ET.Client.CrawlerCardAnimator u_ComCardAnimator;
        public UnityEngine.CanvasGroup u_ComCanvasGroup;
        public UnityEngine.UI.Image u_ComFrameImage;
        public UnityEngine.UI.Image u_ComBodyImage;
        public UnityEngine.UI.Image u_ComArtworkImage;
        public TMPro.TMP_Text u_ComTitleText;
        public TMPro.TMP_Text u_ComBodyText;
        public TMPro.TMP_Text u_ComCostText;
        public UnityEngine.RectTransform u_ComWildMarker;

    }
}
