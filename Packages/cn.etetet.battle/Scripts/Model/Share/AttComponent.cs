using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 血量状态枚举
    /// </summary>
    public enum HpState
    {
        Less10,
        Less30,
        Less50,
        Less70,
        Less100
    }

    /// <summary>
    /// 动态属性类型枚举
    /// </summary>
    public enum EDynAttType
    {
        HeroAttStart = 1000
    }

    /// <summary>
    /// 实体属性类型枚举
    /// </summary>
    public enum EEntityAttType
    {
        End = 5
    }

    /// <summary>
    /// 属性类型枚举
    /// </summary>
    public enum EAttType
    {
        Null = -1,

        Start = 0,

        AStart = Start,
        AttackMelee = AStart + 0, // 0  物理攻击力
        AttackMeleeBase = AStart + 1, // 1  物理攻击力基础值
        AttackMeleeBasePct = AStart + 2, // 2  物理攻击力基础值百分比加成
        AttackMeleeFlat = AStart + 3, // 3  物理攻击力平值加成
        AttackMeleePct = AStart + 4, // 4  物理攻击力百分比加成
        AttackMagic = AStart + 5, // 5  法术攻击力
        AttackMagicBase = AStart + 6, // 6  法术攻击力基础值
        AttackMagicBasePct = AStart + 7, // 7  法术攻击力基础值百分比加成
        AttackMagicFlat = AStart + 8, // 8  法术攻击力平值加成
        AttackMagicPct = AStart + 9, // 9  法术攻击力百分比加成
        DefenceMelee = AStart + 10, // 10 物理防御
        DefenceMeleeBase = AStart + 11, // 11 物理防御基础值
        DefenceMeleeBasePct = AStart + 12, // 12 物理防御基础值百分比加成
        DefenceMeleeFlat = AStart + 13, // 13 物理防御平值加成
        DefenceMeleePct = AStart + 14, // 14 物理防御百分比加成
        DefenceMagic = AStart + 15, // 15 法术防御
        DefenceMagicBase = AStart + 16, // 16 法术防御基础值
        DefenceMagicBasePct = AStart + 17, // 17 法术防御基础值百分比加成
        DefenceMagicFlat = AStart + 18, // 18 法术防御平值加成
        DefenceMagicPct = AStart + 19, // 19 法术防御百分比加成
        MaxHP = AStart + 20, // 20 生命值上限
        MaxHPBase = AStart + 21, // 21 生命值上限基础值
        MaxHPBasePct = AStart + 22, // 22 生命值上限基础值百分比加成
        MaxHPFlat = AStart + 23, // 23 生命值上限平值加成
        MaxHPPct = AStart + 24, // 24 生命值上限百分比加成
        AEnd = AStart + 25,

        BStart = AEnd,
        CurHP = BStart + 0, // 25 当前生命值
        Speed = BStart + 1, // 26 速度
        Crit = BStart + 2, // 27 暴击
        Resilience = BStart + 3, // 28 韧性
        Block = BStart + 4, // 29 格挡
        Broken = BStart + 5, // 30 强击
        NumLives = BStart + 6, // 31 血条数量
        Dodge = BStart + 7, // 32 闪避
        CritInc = BStart + 8, // 33 暴击伤害加成
        CritDec = BStart + 9, // 34 暴击伤害减免
        PctDmgInc = BStart + 10, // 35 攻击百分比伤害改变
        PctDmgDec = BStart + 11, // 36 被攻击百分比伤害改变
        PVPPctDmgInc = BStart + 12, // 37 PVP百分比伤害加成
        PVPPctDmgDec = BStart + 13, // 38 PVP百分比伤害减免
        MeleeDmgInc = BStart + 14, // 39 物理伤害加成
        MagicDmgInc = BStart + 15, // 40 法术伤害加成
        MeleeDmgDec = BStart + 16, // 41 物理伤害减免
        MagicDmgDec = BStart + 17, // 42 法术伤害减免
        PctHealDoneInc = BStart + 18, // 43 治疗百分比加成
        PctHealTakenInc = BStart + 19, // 44 被治疗百分比加成
        CurShield = BStart + 20, // 45 当前护盾
        StrikeBack = BStart + 21, // 46 反击率
        JoinAttack = BStart + 22, // 47 合击率
        ClassDone1 = BStart + 23, // 48 职业百分比加成
        ClassDone2 = BStart + 24, // 49 
        ClassDone3 = BStart + 25, // 50 
        ClassDone4 = BStart + 26, // 51 
        ClassDone5 = BStart + 27, // 52 
        ClassDone6 = BStart + 28, // 53 
        ClassDone7 = BStart + 29, // 54 
        BossDone = BStart + 30, // 55 Boss百分比加成
        ClassTaken1 = BStart + 31, // 56 职业百分比减免
        ClassTaken2 = BStart + 32, // 57 
        ClassTaken3 = BStart + 33, // 58 
        ClassTaken4 = BStart + 34, // 59 
        ClassTaken5 = BStart + 35, // 60 
        ClassTaken6 = BStart + 36, // 61
        ClassTaken7 = BStart + 37, // 62
        BossTaken = BStart + 38, // 63 Boss百分比减免
        SchoolDone1 = BStart + 39, // 64 伤害类型百分比加成
        SchoolDone2 = BStart + 40, // 65 
        SchoolDone3 = BStart + 41, // 66 
        SchoolDone4 = BStart + 42, // 67 
        SchoolDone5 = BStart + 43, // 68 
        SchoolDone6 = BStart + 44, // 69 
        SchoolTaken1 = BStart + 45, // 70 伤害类型百分比减免
        SchoolTaken2 = BStart + 46, // 71 
        SchoolTaken3 = BStart + 47, // 72 
        SchoolTaken4 = BStart + 48, // 73 
        SchoolTaken5 = BStart + 49, // 74 
        SchoolTaken6 = BStart + 50, // 75  
        EffectHit = BStart + 51, // 76 效果命中
        EffectDodge = BStart + 52, // 77 效果抵抗
        SkillLvMod0 = BStart + 53, // 78 普通攻击技能等级
        SkillLvMod1 = BStart + 54, // 79 小技能攻击技能等级
        SkillLvMod2 = BStart + 55, // 80 大技能攻击技能等级
        ShieldPct = BStart + 56, // 81 护盾效果
        BEnd,

        End = BEnd
    }

    /// <summary>
    /// 属性组件 - 只包含数据，不包含方法
    /// 所有逻辑请使用 AttComponentSystem 扩展方法
    /// </summary>
    [ComponentOf(typeof(EntityHero))]
    public class AttComponent : Entity, IAwake<DREntityAttEntry>, IDestroy
    {
        /// <summary>
        /// 最小属性值数组
        /// </summary>
        [StaticField]
        public static int[] MinAttData = new int[(int)EAttType.End];

        /// <summary>
        /// 固定属性数组
        /// </summary>
        public int[] AttData = new int[(int)EAttType.End];

        /// <summary>
        /// 重算计数锁
        /// </summary>
        public bool RecalLock;
    }
}