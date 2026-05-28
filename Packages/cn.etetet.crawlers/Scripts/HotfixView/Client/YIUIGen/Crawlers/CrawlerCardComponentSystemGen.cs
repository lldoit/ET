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
    [EntitySystemOf(typeof(CrawlerCardComponent))]
    public static partial class CrawlerCardComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CrawlerCardComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this CrawlerCardComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this CrawlerCardComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

            self.u_ComCardView = self.UIBase.ComponentTable.FindComponent<ET.Client.CrawlerCardView>("u_ComCardView");
            self.u_ComCardInput = self.UIBase.ComponentTable.FindComponent<ET.Client.CrawlerCardInput>("u_ComCardInput");
            self.u_ComCardAnimator = self.UIBase.ComponentTable.FindComponent<ET.Client.CrawlerCardAnimator>("u_ComCardAnimator");
            self.u_ComCanvasGroup = self.UIBase.ComponentTable.FindComponent<UnityEngine.CanvasGroup>("u_ComCanvasGroup");
            self.u_ComFrameImage = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Image>("u_ComFrameImage");
            self.u_ComBodyImage = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Image>("u_ComBodyImage");
            self.u_ComArtworkImage = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Image>("u_ComArtworkImage");
            self.u_ComTitleText = self.UIBase.ComponentTable.FindComponent<TMPro.TMP_Text>("u_ComTitleText");
            self.u_ComBodyText = self.UIBase.ComponentTable.FindComponent<TMPro.TMP_Text>("u_ComBodyText");
            self.u_ComCostText = self.UIBase.ComponentTable.FindComponent<TMPro.TMP_Text>("u_ComCostText");
            self.u_ComWildMarker = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComWildMarker");

        }
    }
}
