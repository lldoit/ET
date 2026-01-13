using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// Buff数据结构
    /// </summary>
    public struct BuffData
    {
        /// <summary>
        /// Buff配置Id
        /// </summary>
        public int BuffId;

        /// <summary>
        /// 叠加层数（效果叠加）
        /// </summary>
        public int StackCount;

        /// <summary>
        /// 剩余回合数（-1为永久）
        /// </summary>
        public int RemainingTurns;

        /// <summary>
        /// 施放者Id
        /// </summary>
        public long CasterId;
    }

    /// <summary>
    /// Buff效果类型
    /// </summary>
    public enum EBuffEffectType
    {
        /// <summary>
        /// 无效果
        /// </summary>
        None = 0,

        /// <summary>
        /// 属性加成
        /// </summary>
        AttributeModify = 1,

        /// <summary>
        /// 持续伤害
        /// </summary>
        DamageOverTime = 2,

        /// <summary>
        /// 持续治疗
        /// </summary>
        HealOverTime = 3,

        /// <summary>
        /// 护盾
        /// </summary>
        Shield = 4,

        /// <summary>
        /// 状态施加
        /// </summary>
        StateApply = 5
    }

    /// <summary>
    /// Buff触发阶段
    /// </summary>
    public enum EBuffTriggerPhase
    {
        /// <summary>
        /// 添加时
        /// </summary>
        OnAdd = 0,

        /// <summary>
        /// 回合开始时
        /// </summary>
        OnTurnStart = 1,

        /// <summary>
        /// 回合结束时
        /// </summary>
        OnTurnEnd = 2,

        /// <summary>
        /// 受到伤害时
        /// </summary>
        OnDamaged = 3,

        /// <summary>
        /// 造成伤害时
        /// </summary>
        OnDealDamage = 4,

        /// <summary>
        /// 移除时
        /// </summary>
        OnRemove = 5
    }

    /// <summary>
    /// Buff组件 - 管理实体的所有Buff效果
    /// 只包含数据，逻辑在BuffComponentSystem中实现
    /// </summary>
    [ComponentOf(typeof(EntityHero))]
    public class BuffComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前所有Buff列表
        /// </summary>
        public List<BuffData> Buffs;

        /// <summary>
        /// 所属实体引用
        /// </summary>
        public EntityRef<EntityHero> OwnerRef;
    }
}
