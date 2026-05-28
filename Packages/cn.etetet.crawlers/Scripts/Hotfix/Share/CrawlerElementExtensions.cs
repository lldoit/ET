namespace ET
{
    public static class CrawlerElementExtensions
    {
        public static string ToDisplayName(this CrawlerElement element)
        {
            return element switch
            {
                CrawlerElement.Metal => "金",
                CrawlerElement.Wood => "木",
                CrawlerElement.Water => "水",
                CrawlerElement.Fire => "火",
                CrawlerElement.Earth => "土",
                _ => "无"
            };
        }
    }
}
