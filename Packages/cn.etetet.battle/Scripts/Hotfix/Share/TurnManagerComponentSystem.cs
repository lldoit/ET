using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// TurnManagerComponent系统类 - 回合管理逻辑
    /// 遵循ET框架ECS规范
    /// </summary>
    [FriendOf(typeof(TurnManagerComponent))]
    [FriendOf(typeof(BattleSceneComponent))]
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(EnemyAIComponent))]
    [EntitySystemOf(typeof(TurnManagerComponent))]
    public static partial class TurnManagerComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TurnManagerComponent self)
        {
            self.CurrentTurn = 0;
            self.MaxTurns = 30; // 默认最大回合数
            self.IsBattleRunning = false;
            self.CurrentPhase = ETurnPhase.WaitingPlayerInput;
            self.BattleResult = EBattleResult.InProgress;
        }

        [EntitySystem]
        private static void Destroy(this TurnManagerComponent self)
        {
            self.IsBattleRunning = false;
        }

        #endregion

        #region 战斗控制

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="self">回合管理器</param>
        /// <param name="maxTurns">最大回合数</param>
        public static void StartBattle(this TurnManagerComponent self, int maxTurns = 30)
        {
            self.CurrentTurn = 1;
            self.MaxTurns = maxTurns;
            self.IsBattleRunning = true;
            self.CurrentPhase = ETurnPhase.WaitingPlayerInput;
            self.BattleResult = EBattleResult.InProgress;

            // 发布回合开始事件
            BattleSceneComponent battleScene = self.BattleSceneRef;
            if (battleScene != null)
            {
                Scene scene = battleScene.IScene as Scene;
                EventSystem.Instance.Publish(scene, new TurnChangedEvent
                {
                    Turn = self.CurrentTurn,
                    MaxTurns = self.MaxTurns
                });
            }

            Log.Info($"[TurnManager] 战斗开始，最大回合数: {maxTurns}");
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        public static void EndBattle(this TurnManagerComponent self, EBattleResult result)
        {
            self.IsBattleRunning = false;
            self.BattleResult = result;
            self.CurrentPhase = ETurnPhase.TurnEnd;

            Log.Info($"[TurnManager] 战斗结束，结果: {result}");
        }

        #endregion

        #region 三消触发处理

        /// <summary>
        /// 处理三消消除事件
        /// </summary>
        /// <param name="self">回合管理器</param>
        /// <param name="color">消除的糖果颜色</param>
        /// <param name="matchCount">消除数量</param>
        /// <param name="isSkillCandy">是否为技能糖果</param>
        public static async ETTask OnMatch3Combo(this TurnManagerComponent self, int color, int matchCount, bool isSkillCandy = false)
        {
            if (!self.IsBattleRunning)
                return;

            // 保存EntityRef用于await后访问
            EntityRef<TurnManagerComponent> selfRef = self;

            Log.Info($"[TurnManager] 三消触发 - 颜色: {color}, 数量: {matchCount}, 技能糖果: {isSkillCandy}");

            BattleSceneComponent battleScene = self.BattleSceneRef;
            if (battleScene == null)
                return;

            EntityGroup playerGroup = battleScene.RedGroup;
            if (playerGroup == null)
                return;

            // 1. 找到对应颜色的英雄
            EntityHero targetHero = self.FindHeroByColor(playerGroup, color);
            if (targetHero == null)
            {
                Log.Warning($"[TurnManager] 未找到颜色 {color} 对应的英雄");
                return;
            }

            EntityRef<EntityHero> targetHeroRef = targetHero;

            // 2. 增加英雄能量
            int oldEnergy = targetHero.Energy;
            targetHero.ModEnergy(matchCount);

            // 发布能量变化事件
            Scene scene = battleScene.IScene as Scene;
            EventSystem.Instance.Publish(scene, new EnergyChangedEvent
            {
                HeroId = targetHero.Id,
                OldEnergy = oldEnergy,
                NewEnergy = targetHero.Energy,
                MaxEnergy = targetHero.MaxEnergy
            });

            // 3. 进入玩家行动阶段
            self = selfRef;
            self.CurrentPhase = ETurnPhase.PlayerAction;

            // 技能释放顺序：普通攻击 → 小技能(NormalSpell) → 大技能(SpecialSpell)

            if (isSkillCandy)
            {
                // 技能糖果消除：触发NormalSpell（次数=消除数量）
                targetHero = targetHeroRef;
                await self.ExecuteNormalSpell(targetHero, matchCount);
            }
            else
            {
                // 普通糖果消除：触发普通攻击（次数=消除数量）
                targetHero = targetHeroRef;
                await self.ExecuteNormalAttacks(targetHero, matchCount);
            }

            // 4. 检查满能量释放大技能(SpecialSpell)
            self = selfRef;
            targetHero = targetHeroRef;
            if (targetHero != null && targetHero.Energy >= targetHero.MaxEnergy)
            {
                await self.ExecuteSpecialSpell(targetHero);
                targetHero = targetHeroRef;
                if (targetHero != null)
                {
                    targetHero.Energy = 0; // 重置能量
                }
            }

            // 5. 检查战斗结束
            self = selfRef;
            if (self.CheckBattleEnd())
                return;

            // 6. 处理敌方回合
            await self.ProcessEnemyTurn();

            // 7. 回合结束，进入下一回合
            self = selfRef;
            self.NextTurn();
        }

        /// <summary>
        /// 根据颜色找到对应英雄
        /// </summary>
        private static EntityHero FindHeroByColor(this TurnManagerComponent self, EntityGroup group, int color)
        {
            if (group?.Entitys == null)
                return null;

            foreach (var heroRef in group.Entitys)
            {
                EntityHero hero = heroRef;
                if (hero != null && hero.HeroColor == color)
                    return hero;
            }

            return null;
        }

        #endregion

        #region 行动执行

        /// <summary>
        /// 执行普通攻击
        /// </summary>
        /// <param name="self">回合管理器</param>
        /// <param name="attacker">攻击者</param>
        /// <param name="attackCount">攻击次数</param>
        private static async ETTask ExecuteNormalAttacks(this TurnManagerComponent self, EntityHero attacker, int attackCount)
        {
            if (attacker == null)
                return;

            EntityRef<TurnManagerComponent> selfRef = self;
            EntityRef<EntityHero> attackerRef = attacker;

            BattleSceneComponent battleScene = self.BattleSceneRef;
            EntityGroup enemyGroup = battleScene?.BlueGroup;
            if (enemyGroup == null)
                return;

            for (int i = 0; i < attackCount; i++)
            {
                self = selfRef;
                attacker = attackerRef;

                if (attacker == null || self.CheckBattleEnd())
                    break;

                // 找到一个有效敌人
                EntityHero target = self.FindValidTarget(enemyGroup);
                if (target == null)
                    break;

                // 执行普通攻击（使用Melee类型技能）
                if (attacker.Entry?.MeleeSpell > 0)
                {
                    var spellEntry = DREntitySpellEntryCategory.Instance.Get(attacker.Entry.MeleeSpell);
                    if (spellEntry != null)
                    {
                        attacker.CastActiveSpell(spellEntry, target);
                    }
                }

                // 等待一小段时间（用于动画）
                await self.Root().GetComponent<TimerComponent>().WaitAsync(100);
            }
        }

        /// <summary>
        /// 执行小技能（技能糖果触发）
        /// </summary>
        /// <param name="self">回合管理器</param>
        /// <param name="hero">英雄</param>
        /// <param name="count">释放次数</param>
        private static async ETTask ExecuteNormalSpell(this TurnManagerComponent self, EntityHero hero, int count)
        {
            if (hero == null)
                return;

            EntityRef<TurnManagerComponent> selfRef = self;
            EntityRef<EntityHero> heroRef = hero;

            BattleSceneComponent battleScene = self.BattleSceneRef;
            EntityGroup enemyGroup = battleScene?.BlueGroup;
            if (enemyGroup == null)
                return;

            for (int i = 0; i < count; i++)
            {
                self = selfRef;
                hero = heroRef;

                if (hero == null || self.CheckBattleEnd())
                    break;

                // 找到一个有效敌人
                EntityHero target = self.FindValidTarget(enemyGroup);
                if (target == null)
                    break;

                // 执行小技能（NormalSpell）
                if (hero.Entry?.NormalSpell > 0)
                {
                    var spellEntry = DREntitySpellEntryCategory.Instance.Get(hero.Entry.NormalSpell);
                    if (spellEntry != null)
                    {
                        Log.Info($"[TurnManager] 英雄 {hero.HeroId} 释放小技能 {spellEntry.Id}");
                        hero.CastActiveSpell(spellEntry, target);
                    }
                }

                // 等待一小段时间（用于动画）
                await self.Root().GetComponent<TimerComponent>().WaitAsync(100);
            }
        }

        /// <summary>
        /// 执行大技能（满能量触发）
        /// </summary>
        private static async ETTask ExecuteSpecialSpell(this TurnManagerComponent self, EntityHero hero)
        {
            if (hero == null)
                return;

            EntityRef<TurnManagerComponent> selfRef = self;
            EntityRef<EntityHero> heroRef = hero;

            BattleSceneComponent battleScene = self.BattleSceneRef;
            EntityGroup enemyGroup = battleScene?.BlueGroup;
            if (enemyGroup == null)
                return;

            // 找到一个有效敌人
            EntityHero target = self.FindValidTarget(enemyGroup);
            if (target == null)
                return;

            // 执行大技能（SpecialSpell）
            hero = heroRef;
            if (hero.Entry?.SpecialSpell > 0)
            {
                var spellEntry = DREntitySpellEntryCategory.Instance.Get(hero.Entry.SpecialSpell);
                if (spellEntry != null)
                {
                    Log.Info($"[TurnManager] 英雄 {hero.HeroId} 释放大技能 {spellEntry.Id}");
                    hero.CastActiveSpell(spellEntry, target);
                }
            }

            await self.Root().GetComponent<TimerComponent>().WaitAsync(200);
        }

        /// <summary>
        /// 找到一个有效目标
        /// </summary>
        private static EntityHero FindValidTarget(this TurnManagerComponent self, EntityGroup group)
        {
            if (group?.Entitys == null)
                return null;

            foreach (var heroRef in group.Entitys)
            {
                EntityHero hero = heroRef;
                if (hero != null && hero.IsValid())
                    return hero;
            }

            return null;
        }

        #endregion

        #region 敌方回合

        /// <summary>
        /// 处理敌方回合
        /// </summary>
        private static async ETTask ProcessEnemyTurn(this TurnManagerComponent self)
        {
            EntityRef<TurnManagerComponent> selfRef = self;

            self.CurrentPhase = ETurnPhase.EnemyAction;

            BattleSceneComponent battleScene = self.BattleSceneRef;
            EntityGroup enemyGroup = battleScene?.BlueGroup;
            EntityGroup playerGroup = battleScene?.RedGroup;

            if (enemyGroup?.Entitys == null || playerGroup == null)
                return;

            foreach (var enemyRef in enemyGroup.Entitys)
            {
                self = selfRef;
                EntityHero enemy = enemyRef;

                if (enemy == null || !enemy.IsValid())
                    continue;

                EnemyAIComponent ai = enemy.GetComponent<EnemyAIComponent>();
                if (ai == null)
                    continue;

                EntityRef<EntityHero> enemyHeroRef = enemy;

                // 1. 增加能量
                int oldEnergy = enemy.Energy;
                enemy.ModEnergy(ai.EnergyPerTurn);

                // 发布能量变化事件
                Scene scene = battleScene.IScene as Scene;
                EventSystem.Instance.Publish(scene, new EnergyChangedEvent
                {
                    HeroId = enemy.Id,
                    OldEnergy = oldEnergy,
                    NewEnergy = enemy.Energy,
                    MaxEnergy = enemy.MaxEnergy
                });

                // 2. 检查普攻冷却
                if (ai.AttackCooldown <= 0)
                {
                    // 执行普通攻击
                    enemy = enemyHeroRef;
                    EntityHero target = self.FindValidTarget(playerGroup);
                    if (target != null && enemy.Entry?.MeleeSpell > 0)
                    {
                        var spellEntry = DREntitySpellEntryCategory.Instance.Get(enemy.Entry.MeleeSpell);
                        if (spellEntry != null)
                        {
                            enemy.CastActiveSpell(spellEntry, target);
                        }
                    }
                    ai.AttackCooldown = ai.AttackInterval; // 重置冷却
                }
                else
                {
                    ai.AttackCooldown--; // 冷却减少
                }

                // 3. 检查满能量释放技能
                enemy = enemyHeroRef;
                if (enemy != null && enemy.Energy >= enemy.MaxEnergy)
                {
                    EntityHero skillTarget = self.FindValidTarget(playerGroup);
                    if (skillTarget != null && enemy.Entry?.SpecialSpell > 0)
                    {
                        var spellEntry = DREntitySpellEntryCategory.Instance.Get(enemy.Entry.SpecialSpell);
                        if (spellEntry != null)
                        {
                            Log.Info($"[TurnManager] 敌人 {enemy.HeroId} 释放技能 {spellEntry.Id}");
                            enemy.CastActiveSpell(spellEntry, skillTarget);
                        }
                    }
                    enemy = enemyHeroRef;
                    if (enemy != null)
                    {
                        enemy.Energy = 0; // 重置能量
                    }
                }

                // 检查战斗结束
                self = selfRef;
                if (self.CheckBattleEnd())
                    return;

                await self.Root().GetComponent<TimerComponent>().WaitAsync(100);
            }

            self = selfRef;
            self.CurrentPhase = ETurnPhase.TurnEnd;
        }

        #endregion

        #region 回合控制

        /// <summary>
        /// 进入下一回合
        /// </summary>
        private static void NextTurn(this TurnManagerComponent self)
        {
            self.CurrentTurn++;
            self.CurrentPhase = ETurnPhase.WaitingPlayerInput;

            // 检查回合限制
            if (self.CurrentTurn > self.MaxTurns)
            {
                self.EndBattle(EBattleResult.TurnLimit);
                return;
            }

            // 发布回合变化事件
            BattleSceneComponent battleScene = self.BattleSceneRef;
            if (battleScene != null)
            {
                Scene scene = battleScene.IScene as Scene;
                EventSystem.Instance.Publish(scene, new TurnChangedEvent
                {
                    Turn = self.CurrentTurn,
                    MaxTurns = self.MaxTurns
                });
            }

            Log.Info($"[TurnManager] 进入回合 {self.CurrentTurn}/{self.MaxTurns}");
        }

        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        /// <returns>战斗是否结束</returns>
        public static bool CheckBattleEnd(this TurnManagerComponent self)
        {
            if (!self.IsBattleRunning)
                return true;

            BattleSceneComponent battleScene = self.BattleSceneRef;
            if (battleScene == null)
                return true;

            EntityGroup playerGroup = battleScene.RedGroup;
            EntityGroup enemyGroup = battleScene.BlueGroup;

            // 检查敌方全灭
            if (enemyGroup == null || !enemyGroup.IsValid())
            {
                self.EndBattle(EBattleResult.Victory);
                return true;
            }

            // 检查我方全灭
            if (playerGroup == null || !playerGroup.IsValid())
            {
                self.EndBattle(EBattleResult.Defeat);
                return true;
            }

            return false;
        }

        #endregion
    }
}
