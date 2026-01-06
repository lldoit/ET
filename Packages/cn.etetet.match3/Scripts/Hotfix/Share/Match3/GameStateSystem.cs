using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 游戏状态系统（数据与逻辑分离，符合ET框架规范）
    /// </summary>
    public static class GameStateSystem
    {
        /// <summary>
        /// 重置游戏状态到初始状态
        /// </summary>
        public static void Reset(ref GameState state)
        {
            state.Score = 0;
            state.DestroyedAllChocolates = false;
            
            state.CollectedCandies ??= new Dictionary<CandyColor, int>();
            state.CollectedElements ??= new Dictionary<ElementType, int>();
            state.CollectedSpecialBlocks ??= new Dictionary<SpecialBlockType, int>();
            state.CollectedCollectables ??= new Dictionary<CollectableType, int>();
            
            state.CollectedCandies.Clear();
            state.CollectedElements.Clear();
            state.CollectedSpecialBlocks.Clear();
            state.CollectedCollectables.Clear();
            
            foreach (var value in Enum.GetValues(typeof(CandyColor)))
            {
                state.CollectedCandies.Add((CandyColor)value, 0);
            }
            foreach (var value in Enum.GetValues(typeof(ElementType)))
            {
                state.CollectedElements.Add((ElementType)value, 0);
            }
            foreach (var value in Enum.GetValues(typeof(SpecialBlockType)))
            {
                state.CollectedSpecialBlocks.Add((SpecialBlockType)value, 0);
            }
            foreach (var value in Enum.GetValues(typeof(CollectableType)))
            {
                state.CollectedCollectables.Add((CollectableType)value, 0);
            }
            
            state.IsInitialized = true;
        }

        /// <summary>
        /// 添加分数
        /// </summary>
        public static void AddScore(ref GameState state, int score)
        {
            state.Score += score;
        }

        /// <summary>
        /// 添加收集的糖果
        /// </summary>
        public static void AddCandy(ref GameState state, CandyColor candy)
        {
            if (state.CollectedCandies != null && state.CollectedCandies.ContainsKey(candy))
            {
                state.CollectedCandies[candy] += 1;
            }
        }

        /// <summary>
        /// 添加收集的元素
        /// </summary>
        public static void AddElement(ref GameState state, ElementType element)
        {
            if (state.CollectedElements != null && state.CollectedElements.ContainsKey(element))
            {
                state.CollectedElements[element] += 1;
            }
        }

        /// <summary>
        /// 添加收集的特殊方块
        /// </summary>
        public static void AddSpecialBlock(ref GameState state, SpecialBlockType block)
        {
            if (state.CollectedSpecialBlocks != null && state.CollectedSpecialBlocks.ContainsKey(block))
            {
                state.CollectedSpecialBlocks[block] += 1;
            }
        }

        /// <summary>
        /// 添加收集的收集物
        /// </summary>
        public static void AddCollectable(ref GameState state, CollectableType collectable)
        {
            if (state.CollectedCollectables != null && state.CollectedCollectables.ContainsKey(collectable))
            {
                state.CollectedCollectables[collectable] += 1;
            }
        }
        
        /// <summary>
        /// 获取收集的糖果数量
        /// </summary>
        public static int GetCollectedCandies(ref GameState state, CandyColor candy)
        {
            if (state.CollectedCandies != null && state.CollectedCandies.TryGetValue(candy, out var count))
            {
                return count;
            }
            return 0;
        }
        
        /// <summary>
        /// 获取收集的元素数量
        /// </summary>
        public static int GetCollectedElements(ref GameState state, ElementType element)
        {
            if (state.CollectedElements != null && state.CollectedElements.TryGetValue(element, out var count))
            {
                return count;
            }
            return 0;
        }
        
        /// <summary>
        /// 获取收集的特殊方块数量
        /// </summary>
        public static int GetCollectedSpecialBlocks(ref GameState state, SpecialBlockType block)
        {
            if (state.CollectedSpecialBlocks != null && state.CollectedSpecialBlocks.TryGetValue(block, out var count))
            {
                return count;
            }
            return 0;
        }
        
        /// <summary>
        /// 获取收集的收集物数量
        /// </summary>
        public static int GetCollectedCollectables(ref GameState state, CollectableType collectable)
        {
            if (state.CollectedCollectables != null && state.CollectedCollectables.TryGetValue(collectable, out var count))
            {
                return count;
            }
            return 0;
        }
        
        /// <summary>
        /// 标记已摧毁所有巧克力
        /// </summary>
        public static void MarkAllChocolatesDestroyed(ref GameState state)
        {
            state.DestroyedAllChocolates = true;
        }
    }
}
