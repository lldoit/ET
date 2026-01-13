using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 设置实体状态事件
    /// </summary>
    public struct SetEntityState
    {
        public int EntityId;
        public int state;
    }

    /// <summary>
    /// 取消实体状态事件
    /// </summary>
    public struct UnsetEntityState
    {
        public int EntityId;
        public int state;
    }

    public struct DamageInfo
    {
        public int TargetId;
        public int Damage;
        public int SpellResult;
    }

    public struct EntityCastSpell
    {
        public int CasterId;
        public int SpellId;
        public List<DamageInfo> DamageInfos;
    }

    /// <summary>
    /// 回合变化事件
    /// </summary>
    public struct TurnChangedEvent
    {
        /// <summary>
        /// 当前回合数
        /// </summary>
        public int Turn;

        /// <summary>
        /// 最大回合数
        /// </summary>
        public int MaxTurns;
    }

    /// <summary>
    /// 能量变化事件
    /// </summary>
    public struct EnergyChangedEvent
    {
        /// <summary>
        /// 英雄Id
        /// </summary>
        public long HeroId;

        /// <summary>
        /// 旧能量值
        /// </summary>
        public int OldEnergy;

        /// <summary>
        /// 新能量值
        /// </summary>
        public int NewEnergy;

        /// <summary>
        /// 满能量值
        /// </summary>
        public int MaxEnergy;
    }

    /// <summary>
    /// 三消战斗触发事件
    /// </summary>
    public struct Match3BattleTriggerEvent
    {
        /// <summary>
        /// 消除的糖果颜色
        /// </summary>
        public int Color;

        /// <summary>
        /// 消除数量
        /// </summary>
        public int MatchCount;

        /// <summary>
        /// 是否为技能糖果（消除技能糖果触发NormalSpell）
        /// </summary>
        public bool IsSkillCandy;
    }

    /// <summary>
    /// Buff添加事件
    /// </summary>
    public struct BuffAddedEvent
    {
        /// <summary>
        /// 目标实体Id
        /// </summary>
        public long TargetId;

        /// <summary>
        /// Buff配置Id
        /// </summary>
        public int BuffId;

        /// <summary>
        /// 当前叠加层数
        /// </summary>
        public int StackCount;
    }

    /// <summary>
    /// Buff移除事件
    /// </summary>
    public struct BuffRemovedEvent
    {
        /// <summary>
        /// 目标实体Id
        /// </summary>
        public long TargetId;

        /// <summary>
        /// Buff配置Id
        /// </summary>
        public int BuffId;
    }
}

