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
    [FriendOf(typeof(DamageNumberComponent))]
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
        private static bool IsMeleeAttack(int spellId)
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
        /// 判断是否为小技能
        /// </summary>
        /// <param name="spellId">技能配置Id</param>
        /// <returns>true=普攻用Attack动画, false=技能用Spell动画</returns>
        private static bool IsNormalAttack(int spellId)
        {
            if (spellId <= 0)
                return false; // SpellId为0表示普通糖果伤害，用Attack动画

            DREntitySpellEntry spellEntry = DREntitySpellEntryCategory.Instance.Get(spellId);
            if (spellEntry == null)
                return true;

            // Melee类型使用Attack动画，其他使用Spell动画
            return spellEntry.SpellType == (int)EEntitySpellType.Normal;
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
        /// <param name="scene">战斗场景</param>
        /// <param name="args">技能施放参数</param>
        /// <param name="shouldMoveBack">近战攻击后是否返回原位（用于连续普攻时，只在最后一次攻击后返回原位）</param>
        public static async ETTask PlaySpellEffect(Scene scene, EntityCastSpell args, bool shouldMoveBack = true)
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
                return;
            }

            // 判断攻击类型
            bool isNormalAttack = IsMeleeAttack(args.SpellId) || IsNormalAttack(args.SpellId);

            // 如果有目标，获取第一个目标用于移动/朝向
            BattleCharacterViewComponent firstTargetView = null;
            if (args.DamageInfos != null && args.DamageInfos.Count > 0)
            {
                var firstTarget = FindHeroByHeroId(scene, args.DamageInfos[0].TargetId);
                if (firstTarget != null)
                {
                    firstTargetView = GetViewComponent(firstTarget);
                }
            }

            // 2. 处理施法者动作（攻击动画与受击动画同时播放）
            if (isNormalAttack && firstTargetView != null)
            {
                // 近战攻击：移动到目标 → 攻击动画+受击动画 → 返回（如果需要）
                await ProcessNormalAttack(scene, casterView, firstTargetView, args.DamageInfos, shouldMoveBack);
            }
            else
            {
                // 远程攻击/技能：攻击动画+受击动画
                await ProcessSpellAttack(scene, casterView, firstTargetView, args.DamageInfos);
            }
        }

        /// <summary>
        /// 处理近战普通攻击
        /// </summary>
        /// <param name="scene">战斗场景</param>
        /// <param name="casterView">施法者视图组件</param>
        /// <param name="targetView">目标视图组件</param>
        /// <param name="damageInfos">伤害信息列表</param>
        /// <param name="shouldMoveBack">攻击后是否返回原位（连续普攻时只在最后一次返回）</param>
        private static async ETTask ProcessNormalAttack(Scene scene, BattleCharacterViewComponent casterView, BattleCharacterViewComponent targetView, List<DamageInfo> damageInfos, bool shouldMoveBack = true)
        {
            EntityRef<BattleCharacterViewComponent> casterRef = casterView;

            // 计算冲向目标的位置（在目标前方一小段距离）
            Vector3 targetPos = targetView.CharacterGO.transform.position;
            Vector3 moveToPos = targetPos + (casterView.CharacterGO.transform.position - targetPos).normalized * 1f;

            // 1. 移动到目标
            await casterView.MoveToPosition(moveToPos, 8f);

            casterView = casterRef;
            if (casterView == null || casterView.IsDisposed)
                return;

            // 2. 播放攻击动画的同时触发受击动画（fire-and-forget）
            TriggerTargetHitEffects(scene, damageInfos);

            await casterView.PlayAttack();

            casterView = casterRef;
            if (casterView == null || casterView.IsDisposed)
                return;

            // 3. 只有在需要返回时才返回原位（连续普攻时只在最后一次返回）
            if (shouldMoveBack)
            {
                await casterView.MoveBack(8f);
            }
        }

        private static async ETTask ProcessSpellAttack(Scene scene, BattleCharacterViewComponent casterView, BattleCharacterViewComponent targetView, List<DamageInfo> damageInfos)
        {
            EntityRef<BattleCharacterViewComponent> casterRef = casterView;

            // 播放攻击动画的同时触发受击动画（fire-and-forget）
            TriggerTargetHitEffects(scene, damageInfos);

            await casterView.PlaySpell();

            // TODO: 这里可以添加弹道特效逻辑

            casterView = casterRef;
            if (casterView == null || casterView.IsDisposed)
                return;

            // 动画结束后返回待机
            casterView.PlayIdle();
        }

        /// <summary>
        /// 触发所有目标的受击效果（fire-and-forget）
        /// </summary>
        private static void TriggerTargetHitEffects(Scene scene, List<DamageInfo> damageInfos)
        {
            if (damageInfos == null) return;

            foreach (var damageInfo in damageInfos)
            {
                EntityHero target = FindHeroByHeroId(scene, damageInfo.TargetId);
                if (target != null)
                {
                    ProcessTargetHit(target, damageInfo);
                }
            }
        }

        /// <summary>
        /// 处理目标受击效果（飘字、动画）
        /// </summary>
        /// <param name="target">目标英雄</param>
        /// <param name="damageInfo">伤害信息</param>
        private static void ProcessTargetHit(EntityHero target, DamageInfo damageInfo)
        {
            BattleCharacterViewComponent targetView = GetViewComponent(target);
            if (targetView == null)
                return;

            // 获取飘字组件
            Scene scene = target.Scene();
            DamageNumberComponent dnComponent = scene?.GetComponent<DamageNumberComponent>();

            // 获取目标世界坐标（身体中间）
            Vector3 worldPos = targetView.CharacterGO.transform.position;
            worldPos.y += targetView.Animancer.Renderer.bounds.size.y * 0.5f;

            // 检查是否造成伤害
            bool isDamage = (damageInfo.SpellResult & (int)SpellResult.Damage) != 0;
            bool isCrit = (damageInfo.SpellResult & (int)SpellResult.Crit) != 0;
            bool isHeal = (damageInfo.SpellResult & (int)SpellResult.Heal) != 0;

            // 显示飘字（使用队列方法，自动处理延迟）
            if (dnComponent != null && dnComponent.IsInitialized)
            {
                if (isHeal)
                {
                    dnComponent.QueueHeal(worldPos, damageInfo.Damage);
                }
                else if (isCrit)
                {
                    dnComponent.QueueCriticalDamage(worldPos, damageInfo.Damage);
                }
                else if (isDamage)
                {
                    dnComponent.QueueNormalDamage(worldPos, damageInfo.Damage);
                }
            }

            if (isDamage)
            {
                // 检查是否死亡
                if (IsDead(target))
                {
                    // 播放死亡动画（不等待）
                    targetView.PlayDie().NoContext();
                }
                else
                {
                    // 播放受击动画（不等待）
                    targetView.PlayHit().NoContext();
                }
            }
        }
    }
}
