using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 阵营枚举
    /// </summary>
    public enum ECamp
    {
        None = 0,
        Red = 1,
        Blue = 2
    }

    /// <summary>
    /// 实体组 - 队伍组件，只包含数据，不包含方法
    /// 所有逻辑请使用 EntityGroupSystem 扩展方法
    /// </summary>
    [ComponentOf(typeof(BattleSceneComponent))]
    public class EntityGroup : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 阵营
        /// </summary>
        public ECamp Camp;

        /// <summary>
        /// 当前波次
        /// </summary>
        public int Wave;

        /// <summary>
        /// 战力评分
        /// </summary>
        public int GroupScore;
        
        /// <summary>
        /// 出战实体列表
        /// </summary>
        public List< EntityRef<EntityHero>> Entitys;
        
        /// <summary>
        /// 敌方队伍引用
        /// </summary>
        public EntityRef<EntityGroup> OtherGroupRef;

        /// <summary>
        /// 所在战场引用
        /// </summary>
        public EntityRef<BattleSceneComponent> BattleFieldRef;
    }
}