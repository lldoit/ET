namespace ET.Client
{
    public static partial class CrawlersPanelComponentSystem
    {
        private static void EnsureManaWidget(this CrawlersPanelComponent self)
        {
            if (self.FindTransform(ManaWidgetPath) != null)
            {
                return;
            }

            Log.Warning("[CrawlersPanel] 未找到 ManaWidget，请在 CrawlersPanel.prefab 的 RightHud 下创建 ManaWidget/Value");
        }
    }
}
