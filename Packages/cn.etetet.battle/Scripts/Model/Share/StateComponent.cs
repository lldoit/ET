using System;
using System.Collections.Specialized;

namespace ET
{
    /// <summary>
    /// 状态类型
    /// </summary>
    public enum EEntityState
    {
        Dead = 0,           // 死亡
        Stun = 1,           // 眩晕
        Freeze = 2,         // 冰冻
        Sleep = 3,          // 睡眠
        Taunt = 4,          // 挑衅
        Stealth = 5,        // 隐匿
        UnDead = 6,         // 不屈
        CastSpell = 7,      // 蓄力(策划使用状态)
        Escape = 8,         // 逃跑
        AntiHidden = 9,     // 鹰眼
        Seal = 10,          // 沉默
        IngoreIntervene = 11, // 忽略援护
        Reaction = 12,      // 再动
        Reborn = 13,        // 重生
        SingleInvalid = 14, // 无法被单体技能指定为目标
        Numbness = 15,      // 麻痹
        JoinAttack = 16,    // 合击(程序使用状态)
        StrikeBack = 17,    // 反击(程序使用状态)
        Puncture = 18,      // 穿刺(忽略护盾)
        Defence = 19,       // 防御状态
        GroupAttack = 20,   // 集火状态
        Awake = 21,         // 觉醒状态
        ChangeLocation = 22, // 位移状态
        KeepAuraDuration = 23, // 蛊毒
        DoubleAuraDamage = 24, // 裂伤
        Interveneing = 25,  // 援护中
        Shield = 26,        // 护盾中
        IngnoreSpellDelay = 27, // 快速蓄力
        Stuck = 28,         // 禁锢
        LimitHeal = 29,     // 禁疗
        LockHp = 30,        // 锁血
        End
    }

    /// <summary>
    /// 状态标志枚举
    /// </summary>
    [Flags]
    public enum EEntityStateFlag
    {
        Dead = 1 << EEntityState.Dead,
        Stun = 1 << EEntityState.Stun,
        Freeze = 1 << EEntityState.Freeze,
        Sleep = 1 << EEntityState.Sleep,
        Taunt = 1 << EEntityState.Taunt,
        Stealth = 1 << EEntityState.Stealth,
        UnDead = 1 << EEntityState.UnDead,
        CastSpell = 1 << EEntityState.CastSpell,
        Escape = 1 << EEntityState.Escape,
        AntiHidden = 1 << EEntityState.AntiHidden,
        Seal = 1 << EEntityState.Seal,
        Numbness = 1 << EEntityState.Numbness,
        JoinAttack = 1 << EEntityState.JoinAttack,
        StrikeBack = 1 << EEntityState.StrikeBack,
        Stuck = 1 << EEntityState.Stuck,
        Puncture = 1 << EEntityState.Puncture,
        Defence = 1 << EEntityState.Defence,
        Awake = 1 << EEntityState.Awake,

        NotMove = Dead | Stun | Freeze | Sleep | Numbness | Escape | CastSpell,
        NotJoinAttack = NotMove | JoinAttack | CastSpell,
    }

    /// <summary>
    /// 状态组件 - 只包含数据，不包含方法
    /// 所有逻辑请使用 StateComponentSystem 扩展方法
    /// </summary>
    [ComponentOf(typeof(EntityHero))]
    public class StateComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 状态位向量
        /// </summary>
        public BitVector32 State;

        /// <summary>
        /// 状态计数数组
        /// </summary>
        public sbyte[] Count;

        /// <summary>
        /// 所属实体引用
        /// </summary>
        public EntityRef<EntityHero> OwnerRef;

        /// <summary>
        /// 状态数据（只读访问用）
        /// </summary>
        public int Data => State.Data;
    }
}