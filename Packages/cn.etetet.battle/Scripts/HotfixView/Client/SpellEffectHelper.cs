using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 技能效果辅助类
    /// 提供技能视图效果的复用逻辑
    /// </summary>
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(BattleSceneComponent))]
    public static class SpellEffectHelper
    {
        /// <summary>
        /// 根据HeroId查找EntityHero
        /// </summary>
        /// <param name="scene">战斗场景</param>
        /// <param name="heroId">英雄配置Id</param>
        /// <returns>找到的英雄实体，未找到返回null</returns>
        public static EntityHero FindHeroByHeroId(Scene scene, int heroId)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
                return null;

            // 在红方队伍中查找
            EntityGroup redGroup = battleScene.RedGroup;
            if (redGroup?.Entitys != null)
            {
                foreach (var heroRef in redGroup.Entitys)
                {
                    EntityHero hero = heroRef;
                    if (hero != null && hero.HeroId == heroId)
                        return hero;
                }
            }

            // 在蓝方队伍中查找
            EntityGroup blueGroup = battleScene.BlueGroup;
            if (blueGroup?.Entitys != null)
            {
                foreach (var heroRef in blueGroup.Entitys)
                {
                    EntityHero hero = heroRef;
                    if (hero != null && hero.HeroId == heroId)
                        return hero;
                }
            }

            return null;
        }

        /// <summary>
        /// 根据EntityId查找EntityHero
        /// </summary>
        /// <param name="scene">战斗场景</param>
        /// <param name="entityId">实体Id</param>
        /// <returns>找到的英雄实体，未找到返回null</returns>
        public static EntityHero FindHeroByEntityId(Scene scene, long entityId)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
                return null;

            // 在红方队伍中查找
            EntityGroup redGroup = battleScene.RedGroup;
            if (redGroup?.Entitys != null)
            {
                foreach (var heroRef in redGroup.Entitys)
                {
                    EntityHero hero = heroRef;
                    if (hero != null && hero.Id == entityId)
                        return hero;
                }
            }

            // 在蓝方队伍中查找
            EntityGroup blueGroup = battleScene.BlueGroup;
            if (blueGroup?.Entitys != null)
            {
                foreach (var heroRef in blueGroup.Entitys)
                {
                    EntityHero hero = heroRef;
                    if (hero != null && hero.Id == entityId)
                        return hero;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取攻击类型（近战/远程）
        /// </summary>
        /// <param name="spellId">技能配置Id</param>
        /// <returns>true=近战, false=远程</returns>
        public static bool IsMeleeAttack(int spellId)
        {
            if (spellId <= 0)
                return true; // 默认近战

            DREntitySpellEntry spellEntry = DREntitySpellEntryCategory.Instance.Get(spellId);
            if (spellEntry == null)
                return true;

            // 根据技能类型判断
            // Melee类型为近战，其他类型为远程
            return spellEntry.SpellType == (int)EEntitySpellType.Melee;
        }

        /// <summary>
        /// 判断是否为普通攻击技能（决定使用Attack还是Spell动画）
        /// </summary>
        /// <param name="spellId">技能配置Id</param>
        /// <returns>true=普攻用Attack动画, false=技能用Spell动画</returns>
        public static bool IsNormalAttack(int spellId)
        {
            if (spellId <= 0)
                return false; // SpellId为0表示普通糖果伤害，用Attack动画

            DREntitySpellEntry spellEntry = DREntitySpellEntryCategory.Instance.Get(spellId);
            if (spellEntry == null)
                return true;

            // Melee类型使用Attack动画，其他使用Spell动画
            return spellEntry.SpellType == (int)EEntitySpellType.Melee;
        }

        /// <summary>
        /// 检查目标是否死亡
        /// </summary>
        /// <param name="hero">英雄实体</param>
        /// <returns>是否死亡</returns>
        public static bool IsDead(EntityHero hero)
        {
            if (hero == null)
                return true;

            AttComponent attCom = hero.AttCom;
            if (attCom == null)
                return false;

            int currentHp = attCom.GetAttValue(EAttType.CurHP);
            return currentHp <= 0;
        }

        /// <summary>
        /// 获取视图组件
        /// </summary>
        /// <param name="hero">英雄实体</param>
        /// <returns>视图组件，未找到返回null</returns>
        public static BattleCharacterViewComponent GetViewComponent(EntityHero hero)
        {
            return hero?.GetComponent<BattleCharacterViewComponent>();
        }
    }
}
