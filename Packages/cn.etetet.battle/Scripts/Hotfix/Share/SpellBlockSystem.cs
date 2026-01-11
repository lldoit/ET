using System;
using System.Collections.Generic;

namespace ET
{
    [FriendOfAttribute(typeof(ET.EntityHero))]
    [FriendOfAttribute(typeof(ET.EntitySpell))]/// <summary>
                                               /// 技能效果块系统 - 技能效果处理
                                               /// 静态逻辑类，用于处理技能效果
                                               /// </summary>
    public static class SpellBlockSystem
    {
        /// <summary>
        /// 执行效果
        /// </summary>
        public static bool DoEffect(EntitySpell spell, DREntitySpellBlockEntry entry, EntityHero target)
        {
            if (entry == null) return false;

            return entry.Effect switch
            {
                0 => true,                              // 空效果
                1 => NormalDamage(spell, entry, target), // 普通伤害
                2 => FixedDamage(spell, entry, target),  // 固定伤害
                3 => NormalHeal(spell, entry, target),   // 普通治疗
                4 => FixHeal(spell, entry, target),      // 固定治疗
                _ => true
            };
        }

        /// <summary>
        /// 普通伤害
        /// </summary>
        private static bool NormalDamage(EntitySpell spell, DREntitySpellBlockEntry entry, EntityHero target)
        {
            if (target == null)
                return false;

            AttComponent targetAtt = target.AttCom.Entity;
            if (targetAtt == null)
                return false;

            if (entry.Param == null || entry.Param.Length < 3)
                return false;

            double damageParam = entry.Param[0] / 10000.0;
            int damageType = entry.Param[1];
            int fixDamage = entry.Param[2];

            EntityHero caster = spell.CasterRef;
            if (caster == null)
                return false;

            AttComponent casterAtt = caster.AttCom.Entity;
            if (casterAtt == null)
                return false;

            int attack = casterAtt.GetAttValue(EAttType.AttackMelee);
            int defence = targetAtt.GetAttValue(EAttType.DefenceMelee);

            if (attack <= 0)
            {
                Log.Error($"NormalDamage: spell:{spell.Entry?.Id} attack = 0");
                return false;
            }

            int baseDamage = (int)(attack / (1.0 * defence + attack) * attack * damageParam);
            int totalDamage = baseDamage + fixDamage;

            // 应用伤害
            targetAtt.ModAttValue(EAttType.CurHP, -totalDamage);

            spell.TargetDmgInfos.Add(new DamageInfo()
            {
                TargetId = target.HeroId,
                Damage = totalDamage,
                SpellResult = (int)SpellResult.Damage,
            });

            return true;
        }

        /// <summary>
        /// 固定伤害
        /// </summary>
        private static bool FixedDamage(EntitySpell spell, DREntitySpellBlockEntry entry, EntityHero target)
        {
            if (target == null || entry.Param == null || entry.Param.Length < 1)
                return false;

            AttComponent targetAtt = target.AttCom.Entity;
            if (targetAtt == null)
                return false;

            int totalDamage = entry.Param[0];

            targetAtt.ModAttValue(EAttType.CurHP, -totalDamage);

            spell.TargetDmgInfos.Add(new DamageInfo()
            {
                TargetId = target.HeroId,
                Damage = totalDamage,
                SpellResult = (int)SpellResult.Damage,
            });

            return true;
        }

        /// <summary>
        /// 普通治疗
        /// </summary>
        private static bool NormalHeal(EntitySpell spell, DREntitySpellBlockEntry entry, EntityHero target)
        {
            if (target == null || entry.Param == null || entry.Param.Length < 1)
                return false;

            AttComponent targetAtt = target.AttCom.Entity;
            if (targetAtt == null)
                return false;

            double healParam = entry.Param[0] / 10000.0;

            EntityHero caster = spell.CasterRef;
            if (caster == null)
                return false;

            AttComponent casterAtt = caster.AttCom.Entity;
            if (casterAtt == null)
                return false;

            int healPower = casterAtt.GetAttValue(EAttType.AttackMagic);
            int baseHeal = (int)(healPower * healParam);

            int maxHp = targetAtt.GetAttValue(EAttType.MaxHP);
            int curHp = targetAtt.GetAttValue(EAttType.CurHP);
            int newHp = Math.Min(curHp + baseHeal, maxHp);

            targetAtt.ModAttValue(EAttType.CurHP, newHp);

            spell.TargetDmgInfos.Add(new DamageInfo()
            {
                TargetId = target.HeroId,
                Damage = baseHeal,
                SpellResult = (int)SpellResult.Heal,
            });

            return true;
        }

        /// <summary>
        /// 固定治疗
        /// </summary>
        private static bool FixHeal(EntitySpell spell, DREntitySpellBlockEntry entry, EntityHero target)
        {
            if (target == null || entry.Param == null || entry.Param.Length < 1)
                return false;

            AttComponent targetAtt = target.AttCom.Entity;
            if (targetAtt == null)
                return false;

            int heal = entry.Param[0];

            int maxHp = targetAtt.GetAttValue(EAttType.MaxHP);
            int curHp = targetAtt.GetAttValue(EAttType.CurHP);
            int newHp = Math.Min(curHp + heal, maxHp);

            targetAtt.ModAttValue(EAttType.CurHP, newHp);

            spell.TargetDmgInfos.Add(new DamageInfo()
            {
                TargetId = target.HeroId,
                Damage = heal,
                SpellResult = (int)SpellResult.Heal,
            });

            return true;
        }
    }
}