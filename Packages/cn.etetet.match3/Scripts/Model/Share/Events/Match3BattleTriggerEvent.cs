using System.Collections.Generic;

namespace ET
{
    public struct Match3BattleTriggerEvent
    {
        public int Color;
        public int MatchCount;
        public bool IsSkillCandy;
        public List<Match3TilePosition> TilePositions;
    }

    public struct Match3TilePosition
    {
        public int X;
        public int Y;
    }
}
