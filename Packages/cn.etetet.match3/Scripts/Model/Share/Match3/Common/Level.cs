using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 限制类型枚举
    /// </summary>
    public enum LimitType
    {
        Moves,
        Time
    }

    /// <summary>
    /// 关卡配置类，存储游戏关卡设置
    /// </summary>
    public class Level : Object
    {
        public int id;

        public int width;
        public int height;
        public List<LevelTile> tiles = new List<LevelTile>();

        public LimitType limitType;
        public int limit;

        public List<Goal> goals = new List<Goal>();
        public List<CandyColor> availableColors = new List<CandyColor>();

        public int score1;
        public int score2;
        public int score3;

        public bool awardSpecialCandies;
        public AwardedSpecialCandyType awardedSpecialCandyType;

        public int collectableChance;

        public Dictionary<BoosterType, bool> availableBoosters = new Dictionary<BoosterType, bool>();
    }
}

