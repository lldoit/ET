namespace ET.Client
{
    public static partial class CrawlersPanelComponentSystem
    {
        private static void EnsureManaWidget(this CrawlersPanelComponent self)
        {
            if (self.FindTransform(ManaRootPath) != null && self.FindTransform(MultiplierRootPath) != null)
            {
                return;
            }

            Log.Warning("[CrawlersPanel] 未找到右侧灵力或倍数控件，请检查 RightHud/Bp001 和 RightHud/EnergyOrb");
        }
    }
}
