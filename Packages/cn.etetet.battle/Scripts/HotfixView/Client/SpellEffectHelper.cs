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

            // 2. 缓存伤害信息，在Spine Attack事件时触发受击效果
            CacheDamageInfos(casterView, damageInfos);

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

            // 缓存伤害信息，在Spine Attack事件时触发受击效果
            CacheDamageInfos(casterView, damageInfos);

            await casterView.PlaySpell();

            // TODO: 这里可以添加弹道特效逻辑

            casterView = casterRef;
            if (casterView == null || casterView.IsDisposed)
                return;

            // 动画结束后返回待机
            casterView.PlayIdle();
        }

        /// <summary>
        /// 缓存伤害信息到施法者视图组件（等待Spine Attack事件时触发）
        /// </summary>
        /// <param name="casterView">施法者视图组件</param>
        /// <param name="damageInfos">伤害信息列表</param>
        private static void CacheDamageInfos(BattleCharacterViewComponent casterView, List<DamageInfo> damageInfos)
        {
            if (casterView == null || damageInfos == null)
                return;

            // 初始化或清空缓存列表
            if (casterView.PendingDamageInfos == null)
            {
                casterView.PendingDamageInfos = new List<DamageInfo>();
            }
            else
            {
                casterView.PendingDamageInfos.Clear();
            }

            // 缓存伤害信息
            casterView.PendingDamageInfos.AddRange(damageInfos);
        }
    }
}
