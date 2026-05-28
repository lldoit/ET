using UnityEngine;

namespace ET.Client
{
    public static partial class CrawlersPanelComponentSystem
    {
        private const string BackButtonPath = "CrawlersView/TopStatusBar/BackButton";
        private const string EndTurnButtonPath = "CrawlersView/RightHud/EndTurnButton";
        private const string HandTuningPanelPath = "CrawlersView/HandTuningPanel";
        private const string CardCountSliderPath = "CrawlersView/HandTuningPanel/CardCountSlider";
        private const string AngleSliderPath = "CrawlersView/HandTuningPanel/AngleSlider";
        private const string SpacingSliderPath = "CrawlersView/HandTuningPanel/SpacingSlider";
        private const string CardCountValuePath = "CrawlersView/HandTuningPanel/CardCountValue";
        private const string AngleValuePath = "CrawlersView/HandTuningPanel/AngleValue";
        private const string SpacingValuePath = "CrawlersView/HandTuningPanel/SpacingValue";
        private const string TurnCounterPath = "CrawlersView/TopStatusBar/TurnCounter";
        private const string ManaValuePath = "CrawlersView/RightHud/EnergyOrb/Value";
        private const string BossHpLabelPath = "CrawlersView/TopStatusBar/BossHp/Label";
        private const string RightHudPath = "CrawlersView/RightHud";
        private const string ManaWidgetPath = "CrawlersView/RightHud/ManaWidget";
        private const string ManaWidgetValuePath = "CrawlersView/RightHud/ManaWidget/Value";
        private const string PlayedPilePath = "CrawlersView/RightHud/PlayedPile";
        private const string DiscardPilePath = "CrawlersView/RightHud/DiscardPile";
        private const string DrawPilePath = "CrawlersView/LeftHud/DrawPile";
        private const string CrawlersViewPath = "CrawlersView";

        private static T FindComponent<T>(this CrawlersPanelComponent self, string path) where T : Component
        {
            Transform transform = self.FindTransform(path);
            return transform != null ? transform.GetComponent<T>() : null;
        }

        private static Transform FindTransform(this CrawlersPanelComponent self, string path)
        {
            GameObject owner = self.UIBase?.OwnerGameObject;
            if (owner == null)
            {
                return null;
            }

            return owner.transform.Find(path);
        }

        private static RectTransform FindRectTransform(this CrawlersPanelComponent self, string path)
        {
            Transform transform = self.FindTransform(path);
            return transform as RectTransform;
        }

        private static RectTransform GetOrCreateRectTransform(this CrawlersPanelComponent self, string path, string name)
        {
            RectTransform rectTransform = self.FindRectTransform(path);
            if (rectTransform != null)
            {
                return rectTransform;
            }

            RectTransform parent = self.FindRectTransform(RightHudPath);
            if (parent == null)
            {
                return null;
            }

            var gameObject = new GameObject(name, typeof(RectTransform));
            rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -24f);
            rectTransform.sizeDelta = new Vector2(140f, 260f);
            return rectTransform;
        }

        private static RectTransform GetRightHudRectTransform(this CrawlersPanelComponent self)
        {
            return self.FindRectTransform(RightHudPath);
        }

        private static void SetCrawlersViewVisible(this CrawlersPanelComponent self, bool visible)
        {
            Transform view = self.FindTransform(CrawlersViewPath);
            if (view != null)
            {
                view.gameObject.SetActive(visible);
            }
        }
    }
}
