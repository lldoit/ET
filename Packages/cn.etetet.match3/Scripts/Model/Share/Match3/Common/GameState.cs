using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 游戏状态，存储游戏在某个时间点的状态
    /// </summary>
    public class GameState : Object
    {
        public int score;
        public Dictionary<CandyColor, int> collectedCandies = new Dictionary<CandyColor, int>();
        public Dictionary<ElementType, int> collectedElements = new Dictionary<ElementType, int>();
        public Dictionary<SpecialBlockType, int> collectedSpecialBlocks = new Dictionary<SpecialBlockType, int>();
        public Dictionary<CollectableType, int> collectedCollectables = new Dictionary<CollectableType, int>();
        public bool destroyedAllChocolates;

        /// <summary>
        /// 重置游戏状态到初始状态
        /// </summary>
        public void Reset()
        {
            score = 0;
            collectedCandies.Clear();
            collectedElements.Clear();
            collectedSpecialBlocks.Clear();
            collectedCollectables.Clear();
            foreach (var value in Enum.GetValues(typeof(CandyColor)))
            {
                collectedCandies.Add((CandyColor)value, 0);
            }
            foreach (var value in Enum.GetValues(typeof(ElementType)))
            {
                collectedElements.Add((ElementType)value, 0);
            }
            foreach (var value in Enum.GetValues(typeof(SpecialBlockType)))
            {
                collectedSpecialBlocks.Add((SpecialBlockType)value, 0);
            }
            foreach (var value in Enum.GetValues(typeof(CollectableType)))
            {
                collectedCollectables.Add((CollectableType)value, 0);
            }

            destroyedAllChocolates = false;
        }

        public void AddCandy(CandyColor candy)
        {
            collectedCandies[candy] += 1;
        }

        public void AddElement(ElementType element)
        {
            collectedElements[element] += 1;
        }

        public void AddSpecialBlock(SpecialBlockType block)
        {
            collectedSpecialBlocks[block] += 1;
        }

        public void AddCollectable(CollectableType collectable)
        {
            collectedCollectables[collectable] += 1;
        }
    }
}

