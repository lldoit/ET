using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    public static partial class CrawlersPanelComponentSystem
    {
        private static void BindHandTuningControls(this CrawlersPanelComponent self)
        {
            self.SetHandTuningVisible(false);

            CrawlerHandView handView = self.GetHandView();
            if (handView == null)
            {
                Log.Warning("[CrawlersPanel] 未找到 CrawlerHandView，无法绑定手牌调参控件");
                return;
            }

            Slider cardCountSlider = self.FindComponent<Slider>(CardCountSliderPath);
            Slider angleSlider = self.FindComponent<Slider>(AngleSliderPath);
            Slider spacingSlider = self.FindComponent<Slider>(SpacingSliderPath);
            TMP_Text cardCountValue = self.FindComponent<TMP_Text>(CardCountValuePath);
            TMP_Text angleValue = self.FindComponent<TMP_Text>(AngleValuePath);
            TMP_Text spacingValue = self.FindComponent<TMP_Text>(SpacingValuePath);

            if (cardCountSlider != null)
            {
                ConfigureSlider(cardCountSlider, 1f, 12f, true, handView.PreviewCardCount);
                UpdateCountLabel(cardCountValue, handView.PreviewCardCount);
                cardCountSlider.onValueChanged.RemoveAllListeners();
                cardCountSlider.onValueChanged.AddListener(value =>
                {
                    int count = Mathf.RoundToInt(value);
                    handView.SetPreviewCardCount(count);
                    UpdateCountLabel(cardCountValue, count);
                });
            }

            if (angleSlider != null)
            {
                ConfigureSlider(angleSlider, 0f, 60f, false, handView.MaxFanAngle);
                UpdateFloatLabel(angleValue, handView.MaxFanAngle, "°");
                angleSlider.onValueChanged.RemoveAllListeners();
                angleSlider.onValueChanged.AddListener(value =>
                {
                    handView.SetMaxFanAngle(value);
                    UpdateFloatLabel(angleValue, value, "°");
                });
            }

            if (spacingSlider != null)
            {
                ConfigureSlider(spacingSlider, 40f, 220f, false, handView.CardSpacing);
                UpdateFloatLabel(spacingValue, handView.CardSpacing, string.Empty);
                spacingSlider.onValueChanged.RemoveAllListeners();
                spacingSlider.onValueChanged.AddListener(value =>
                {
                    handView.SetCardSpacing(value);
                    UpdateFloatLabel(spacingValue, value, string.Empty);
                });
            }
        }

        private static void ClearHandTuningListeners(this CrawlersPanelComponent self)
        {
            self.FindComponent<Slider>(CardCountSliderPath)?.onValueChanged.RemoveAllListeners();
            self.FindComponent<Slider>(AngleSliderPath)?.onValueChanged.RemoveAllListeners();
            self.FindComponent<Slider>(SpacingSliderPath)?.onValueChanged.RemoveAllListeners();
        }

        [EntitySystem]
        private static void Update(this CrawlersPanelComponent self)
        {
            if (self.ToggleHandTuningKey == KeyCode.None)
            {
                return;
            }

            if (Input.GetKeyDown(self.ToggleHandTuningKey))
            {
                self.ToggleHandTuningPanel();
            }
        }

        private static void ToggleHandTuningPanel(this CrawlersPanelComponent self)
        {
            Transform panel = self.FindTransform(HandTuningPanelPath);
            if (panel == null)
            {
                Log.Warning("[CrawlersPanel] 未找到 HandTuningPanel，无法切换手牌调参界面");
                return;
            }

            panel.gameObject.SetActive(!panel.gameObject.activeSelf);
        }

        private static void SetHandTuningVisible(this CrawlersPanelComponent self, bool visible)
        {
            Transform panel = self.FindTransform(HandTuningPanelPath);
            if (panel == null)
            {
                Log.Warning("[CrawlersPanel] 未找到 HandTuningPanel，无法设置手牌调参界面显示状态");
                return;
            }

            panel.gameObject.SetActive(visible);
        }

        private static void ConfigureSlider(Slider slider, float min, float max, bool wholeNumbers, float value)
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.SetValueWithoutNotify(Mathf.Clamp(value, min, max));
        }

        private static void UpdateCountLabel(TMP_Text label, int value)
        {
            if (label != null)
            {
                label.text = value.ToString();
            }
        }

        private static void UpdateFloatLabel(TMP_Text label, float value, string suffix)
        {
            if (label != null)
            {
                label.text = $"{value:0}{suffix}";
            }
        }
    }
}
