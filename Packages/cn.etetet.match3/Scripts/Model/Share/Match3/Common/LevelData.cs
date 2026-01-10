using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 关卡JSON序列化包装类（运行时版本）
    /// 用于JsonUtility序列化struct
    /// </summary>
    [EnableClass]
    public class LevelData
    {
        public int Id;
        public int Width;
        public int Height;
        public string LimitType;
        public int Limit;
        public int Score1;
        public int Score2;
        public int Score3;
        public bool AwardSpecialCandies;
        public string AwardedSpecialCandyType;
        public int CollectableChance;
        public List<LevelTileData> Tiles;
        public List<GoalData> Goals;
        public List<string> AvailableColors;
        public BoosterSettingsData AvailableBoosters;

        public LevelData()
        {
            Tiles = new List<LevelTileData>();
            Goals = new List<GoalData>();
            AvailableColors = new List<string>();
            AvailableBoosters = new BoosterSettingsData();
        }
    }

    /// <summary>
    /// 关卡瓦片序列化数据
    /// </summary>
    [Serializable]
    [EnableClass]
    public class LevelTileData
    {
        public string TileType;
        public string ElementType;
        public string CandyType;
        public string SpecialCandyType;
        public string SpecialBlockType;
        public string CollectableType;
    }

    /// <summary>
    /// 目标序列化数据
    /// </summary>
    [Serializable]
    [EnableClass]
    public class GoalData
    {
        public string GoalType;
        public int Amount;
        public string CandyColor;
        public string ElementType;
        public string SpecialBlockType;
        public string CollectableType;
    }

    /// <summary>
    /// 道具设置序列化数据
    /// </summary>
    [Serializable]
    [EnableClass]
    public class BoosterSettingsData
    {
        public bool Lollipop;
        public bool Bomb;
        public bool Switch;
        public bool ColorBomb;
    }
}
