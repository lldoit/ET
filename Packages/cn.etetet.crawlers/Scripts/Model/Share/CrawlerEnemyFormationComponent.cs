using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(CrawlerBattleComponent))]
    public class CrawlerEnemyFormationComponent : Entity, IAwake, IDestroy
    {
        public List<CrawlerEnemyData> EnemyLibrary;
        public List<List<CrawlerEnemyState>> Rows;
        public long NextEnemyInstanceId;
        public int MaxColumns;
        public int LastAdvancedRows;
        public int LastFrontRowAttackers;
        public CrawlerEnemyTurnResult LastEnemyTurnResult;
    }
}
