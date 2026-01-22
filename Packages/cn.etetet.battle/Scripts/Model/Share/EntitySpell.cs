using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 技能类型枚举
    /// </summary>
    public enum EEntitySpellType
    {
        Melee = 0,          // 普攻
        Normal = 1,         // 小技能
        Special = 2,        // 大技能
        Defence = 3,        // 防御
        Awake = 4,          // 觉醒
        SupportSpell = 5,   // 支援技能
        AuraTrigger = 6,    // 光环触发
        AuraCast = 7,       // 光环施放
        End
    }

    /// <summary>
    /// 战斗错误码
    /// </summary>
    public enum ECombatErr
    {
        Success = 0,
        NoTarget,
        Cooldown,
        CasterState,
        TargetState,
        CostLimit,
    }

    /// <summary>
    /// 技能消耗类型
    /// </summary>
    public enum SpellCost
    {
        Null = 0,
        Rage = 1,
        Energy = 2,
        SupportPoint = 3,
        AwakePoint = 4,
    }

    /// <summary>
    /// 技能结果标志
    /// </summary>
    [Flags]
    public enum SpellResult
    {
        None = 0,
        Damage = 1 << 0,
        Heal = 1 << 1,
        Crit = 1 << 2,
        Block = 1 << 3,
        Miss = 1 << 4,
        Trigger = 1 << 5,
        PhysicsDamage = 1 << 6,
        MagicDamage = 1 << 7,
        Kill = 1 << 8, 
    }
    
    public enum SelectTargetType
    {
        Enemy_MaxAttackMelee = -17, // 敌方物理攻击最高
        Enemy_MaxAttackMagic = -16, // 敌方法术攻击最高
        Enemy_RightToLeft = -15, // 敌方全体从右到左
        Enemy_CounterPoint = -14, // 敌方对位
        Enemy_LeftToRight = -13, // 敌方全体从左到右
        Enemy_SelectRow = -12, // 敌方所选目标横排
        Enemy_SelectRound = -11,    // 敌方选择目标周围
        Enemy_RearLinePriority = -10,    // 敌方后排优先
        Enemy_FrontLinePriority = -9,    // 敌方前排优先
        Enemy_MaxSpeed = -8,    // 敌方速度最快
        Enemy_MaxAttack_Least = -7, // 随机选取N个敌人，最大数量不超过敌军当前人数-2
        Enemy_MaxAttack = -6,    // 敌方攻击力最高
        Enemy_MinHP = -5,   // 敌方生命最低
        Enemy_Random_Select = -4,   // 敌方随机目标包括选取目标
        Enemy_All = -3, // 敌方全体目标	
        Enemy_Random = -2,  // 敌方随机目标
        Enemy_Single = -1,  // 敌方单体目标
        Null = 0,
        Self = 1,   // 自己
        Friend_Single = 2,  // 友方单体目标
        Friend_All = 3, // 友方全体目标
        Friend_Random = 4,  // 友方随机目标
        Friend_MinHP = 5,   // 友方生命最低
        Friend_Random_Select = 6,   // 友方随机目标包括选取目标
        Friend_MaxSpeed = 7,    // 友方速度最快
        Friend_SelectRow = 8, // 友方所选目标横排
        Friend_CasterRow = 9, // 技能施放者所在横排
        Friend_MaxAttackMelee = 10, // 友方物理攻击最高
        Friend_MaxAttackMagic = 11, // 友方法术攻击最高
        Friend_SelectRound = 12,    // 友方选择目标周围
        Friend_MaxAttack = 13,    // 友方攻击力最高
        Friend_SelfRound = 14,    // 自己和周围目标
    };


    /// <summary>
    /// 技能实体 - 只包含数据，不包含方法
    /// 所有逻辑请使用 EntitySpellSystem 扩展方法
    /// </summary>
    [ChildOf(typeof(EntityHero))]
    public class EntitySpell : Entity, IAwake, IDestroy
    {
        

        /// <summary>
        /// 技能配置
        /// </summary>
        public DREntitySpellEntry Entry;

        /// <summary>
        /// 施放者引用
        /// </summary>
        public EntityRef<EntityHero> CasterRef;

        /// <summary>
        /// 技能选取目标引用
        /// </summary>
        public EntityRef<EntityHero> SelectTargetRef;

        /// <summary>
        /// 目标列表
        /// </summary>
        public List<EntityRef<EntityHero>> Targets;

        /// <summary>
        /// 总技能结果
        /// </summary>
        public int TotalSpellResult;

        /// <summary>
        /// 技能类型
        /// </summary>
        public EEntitySpellType SpellType;

        /// <summary>
        /// 数量
        /// </summary>
        public int Amount;

        /// <summary>
        /// 目标伤害信息列表
        /// </summary>
        public List<DamageInfo> TargetDmgInfos;

        /// <summary>
        /// 当前光环触发参数
        /// </summary>
        public DamageInfo CurAuraTriggerParam;
    }
}