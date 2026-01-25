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

    /// <summary>
    /// 玩家回合开始事件(用于视觉表现)
    /// </summary>
    public struct PlayerTurnBeginEvent
    {
    }

    /// <summary>
    /// 敌方回合开始事件(用于视觉表现)
    /// </summary>
    public struct EnemyTurnBeginEvent
    {
    }

    /// <summary>
    /// 通知三消可以消除事件(视觉流程结束)
    /// </summary>
    public struct Match3CanEliminateEvent
    {
    }
}
