using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 技能效果辅助类
    /// 提供技能视图效果的复用逻辑
    /// </summary>
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(BattleSceneComponent))]
    [FriendOf(typeof(BattleCharacterViewComponent))]
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

        /// <summary>
        /// 播放技能视图效果
        /// </summary>
        public static async ETTask PlaySpellEffect(Scene scene, EntityCastSpell args)
        {
            // 1. 找到施法者英雄
            EntityHero caster = FindHeroByHeroId(scene, args.CasterId);
            if (caster == null)
            {
                Log.Warning($"[SpellEffectHelper] 未找到施法者 HeroId={args.CasterId}");
                return;
            }

            BattleCharacterViewComponent casterView = GetViewComponent(caster);
            if (casterView == null)
            {
                Log.Warning($"[SpellEffectHelper] 施法者没有视图组件 HeroId={args.CasterId}");
                return;
            }

            if (args.SpellId == 0)
            {
                // 这是棋子对目标的直接伤害，播放棋子伤害动画
                // 目前逻辑兼容这种case，往下走默认为近战攻击
            }

            // 判断是否为近战攻击
            bool isMelee = IsMeleeAttack(args.SpellId);
            bool isNormalAttack = IsNormalAttack(args.SpellId);

            // 如果有目标，获取第一个目标用于移动/朝向
            EntityHero firstTarget = null;
            BattleCharacterViewComponent firstTargetView = null;
            if (args.DamageInfos != null && args.DamageInfos.Count > 0)
            {
                firstTarget = FindHeroByHeroId(scene, args.DamageInfos[0].TargetId);
                if (firstTarget != null)
                {
                    firstTargetView = GetViewComponent(firstTarget);
                }
            }

            // 2. 处理施法者动作
            if (isMelee && firstTarget != null && firstTargetView != null)
            {
                // 近战攻击：移动到目标 → 攻击动画 → 返回
                await ProcessMeleeAttack(casterView, firstTargetView, isNormalAttack);
            }
            else
            {
                // 远程攻击/技能：播放动画
                await ProcessRangedAttack(casterView, firstTargetView, isNormalAttack);
            }

            // 3. 短暂延迟后处理目标受击效果
            //await scene.Root().GetComponent<TimerComponent>().WaitAsync(100);

            // 4. 处理所有目标的受击效果
            if (args.DamageInfos != null)
            {
                foreach (var damageInfo in args.DamageInfos)
                {
                    EntityHero target = FindHeroByHeroId(scene, damageInfo.TargetId);
                    if (target != null)
                    {
                        await ProcessTargetHit(target, damageInfo);
                    }
                }
            }
        }

        private static async ETTask ProcessMeleeAttack(BattleCharacterViewComponent casterView, BattleCharacterViewComponent targetView, bool isNormalAttack)
        {
            EntityRef<BattleCharacterViewComponent> casterRef = casterView;

            // 计算冲向目标的位置（在目标前方一小段距离）
            Vector3 targetPos = targetView.CharacterGO.transform.position;
            Vector3 moveToPos = targetPos + (casterView.CharacterGO.transform.position - targetPos).normalized * 0.5f;

            // 1. 移动到目标
            await casterView.MoveToPosition(moveToPos, 8f);

            casterView = casterRef;
            if (casterView == null || casterView.IsDisposed)
                return;

            // 2. 播放攻击动画
            if (isNormalAttack)
            {
                await casterView.PlayAttack();
            }
            else
            {
                await casterView.PlaySpell();
            }

            casterView = casterRef;
            if (casterView == null || casterView.IsDisposed)
                return;

            // 3. 返回原位
            await casterView.MoveBack(8f);
        }

        private static async ETTask ProcessRangedAttack(BattleCharacterViewComponent casterView, BattleCharacterViewComponent targetView, bool isNormalAttack)
        {
            EntityRef<BattleCharacterViewComponent> casterRef = casterView;

            // 播放攻击/技能动画
            if (isNormalAttack)
            {
                await casterView.PlayAttack();
            }
            else
            {
                await casterView.PlaySpell();
            }

            // TODO: 这里可以添加弹道特效逻辑

            casterView = casterRef;
            if (casterView == null || casterView.IsDisposed)
                return;

            // 动画结束后返回待机
            casterView.PlayIdle();
        }

        private static async ETTask ProcessTargetHit(EntityHero target, DamageInfo damageInfo)
        {
            BattleCharacterViewComponent targetView = GetViewComponent(target);
            if (targetView == null)
                return;

            EntityRef<BattleCharacterViewComponent> targetRef = targetView;

            // 检查是否造成伤害
            bool isDamage = (damageInfo.SpellResult & (int)SpellResult.Damage) != 0;

            if (isDamage)
            {
                // 播放受击动画
                await targetView.PlayHit();

                targetView = targetRef;
                if (targetView == null || targetView.IsDisposed)
                    return;

                // 检查是否死亡
                if (IsDead(target))
                {
                    // 播放死亡动画
                    await targetView.PlayDie();
                }
                else
                {
                    // 返回待机
                    targetView.PlayIdle();
                }
            }
        }
    }
}
