using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 技能释放事件处理器 - 处理技能视图效果
    /// 订阅 EntityCastSpell 事件，触发施法者/目标动画
    /// </summary>
    [Event(SceneType.Battle)]
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(BattleCharacterViewComponent))]
    public class EntityCastSpellEventHandler : AEvent<Scene, EntityCastSpell>
    {
        protected override async ETTask Run(Scene scene, EntityCastSpell args)
        {
            // 1. 找到施法者英雄
            EntityHero caster = SpellEffectHelper.FindHeroByHeroId(scene, args.CasterId);
            if (caster == null)
            {
                Log.Warning($"[EntityCastSpellEventHandler] 未找到施法者 HeroId={args.CasterId}");
                return;
            }

            BattleCharacterViewComponent casterView = SpellEffectHelper.GetViewComponent(caster);
            if (casterView == null)
            {
                Log.Warning($"[EntityCastSpellEventHandler] 施法者没有视图组件 HeroId={args.CasterId}");
                return;
            }

            if (args.SpellId == 0)
            {
                // 这是棋子对目标的直接伤害，播放棋子伤害动画
                Log.Warning("棋子伤害～～～～～");
            }
            else
            {
                // 判断是否为近战攻击
                bool isMelee = SpellEffectHelper.IsMeleeAttack(args.SpellId);
                bool isNormalAttack = SpellEffectHelper.IsNormalAttack(args.SpellId);

                // 如果有目标，获取第一个目标用于移动/朝向
                EntityHero firstTarget = null;
                BattleCharacterViewComponent firstTargetView = null;
                if (args.DamageInfos != null && args.DamageInfos.Count > 0)
                {
                    firstTarget = SpellEffectHelper.FindHeroByHeroId(scene, args.DamageInfos[0].TargetId);
                    if (firstTarget != null)
                    {
                        firstTargetView = SpellEffectHelper.GetViewComponent(firstTarget);
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
            }

            // 3. 短暂延迟后处理目标受击效果
            await scene.Root().GetComponent<TimerComponent>().WaitAsync(100);

            // 4. 处理所有目标的受击效果
            if (args.DamageInfos != null)
            {
                foreach (var damageInfo in args.DamageInfos)
                {
                    EntityHero target = SpellEffectHelper.FindHeroByHeroId(scene, damageInfo.TargetId);
                    if (target != null)
                    {
                        await ProcessTargetHit(target, damageInfo);
                    }
                }
            }

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 处理近战攻击流程
        /// </summary>
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

        /// <summary>
        /// 处理远程攻击/技能流程
        /// </summary>
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
            // 例如：创建飞行弹道GameObject，从施法者飞向目标

            casterView = casterRef;
            if (casterView == null || casterView.IsDisposed)
                return;

            // 动画结束后返回待机
            casterView.PlayIdle();
        }

        /// <summary>
        /// 处理目标受击效果
        /// </summary>
        private static async ETTask ProcessTargetHit(EntityHero target, DamageInfo damageInfo)
        {
            BattleCharacterViewComponent targetView = SpellEffectHelper.GetViewComponent(target);
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
                if (SpellEffectHelper.IsDead(target))
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
