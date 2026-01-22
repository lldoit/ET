using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// EntitySpell系统类 - 技能逻辑
    /// 遵循ET框架ECS规范
    /// </summary>
    [FriendOf(typeof(EntitySpell))]
    [EntitySystemOf(typeof(EntitySpell))]
    [FriendOfAttribute(typeof(ET.EntityHero))]
    [FriendOfAttribute(typeof(ET.EntityGroup))]
    public static partial class EntitySpellSystem
    {
        [EntitySystem]
        private static void Awake(this EntitySpell self)
        {
            self.Targets = new List<EntityRef<EntityHero>>();
            self.TargetDmgInfos = new List<DamageInfo>();
        }

        [EntitySystem]
        private static void Destroy(this EntitySpell self)
        {
            self.Clear();
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public static void Clear(this EntitySpell self)
        {
            self.Entry = null;
            self.CasterRef = default;
            self.SelectTargetRef = default;
            self.Targets?.Clear();
            self.TargetDmgInfos?.Clear();
            self.TotalSpellResult = 0;
            self.CurAuraTriggerParam = default;
        }

        /// <summary>
        /// 获取施放者
        /// </summary>
        public static EntityHero GetCaster(this EntitySpell self)
        {
            return self.CasterRef;
        }

        /// <summary>
        /// 获取选取目标
        /// </summary>
        public static EntityHero GetSelectTarget(this EntitySpell self)
        {
            return self.SelectTargetRef;
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        public static void Init(this EntitySpell self, EntityHero caster, EntityHero target, DREntitySpellEntry entry, int amount)
        {
            self.Entry = entry;
            self.CasterRef = caster;
            self.SelectTargetRef = target;
            // self.SpellType = type;
            self.Amount = amount;
            self.TotalSpellResult = 0;
            self.Targets?.Clear();
            self.TargetDmgInfos?.Clear();
        }

        /// <summary>
        /// 获取技能配置Id
        /// </summary>
        public static int GetTypeId(this EntitySpell self)
        {
            return self.Entry?.Id ?? 0;
        }

        /// <summary>
        /// 是否可触发
        /// </summary>
        private static bool CanTrigger(this EntitySpell self)
        {
            return self.Entry != null && !self.Entry.NotTrigger && self.SpellType <= EEntitySpellType.Defence;
        }

        /// <summary>
        /// 施放技能
        /// </summary>
        public static ECombatErr Cast(this EntitySpell self)
        {
            self.CastStart();
            self.FindTargets();

            if (self.Targets.Count == 0)
                return ECombatErr.NoTarget;

            if (self.TriggerBeforCast())
            {
                self.CalEffect();
                self.CastSubSpell();
            }

            self.CastEnd();

            return ECombatErr.Success;
        }

        /// <summary>
        /// 触发施放
        /// </summary>
        public static ECombatErr TriggerCast(this EntitySpell self)
        {
            self.CastStart();
            self.FindTargets();
            if (self.Targets.Count == 0)
                return ECombatErr.NoTarget;

            self.CalEffect();
            self.CastSubSpell();

            self.CastEnd();
            return ECombatErr.Success;
        }

        /// <summary>
        /// 查找目标
        /// </summary>
        public static void FindTargets(this EntitySpell self, List<EntityRef<EntityHero>> parentTargets = null)
        {
            self.Targets.Clear();
            EntityHero caster = self.CasterRef;
            EntityHero selectTarget = self.SelectTargetRef;
            if (caster == null) return;

            var entry = self.Entry;
            if (entry == null) return;

            switch ((SelectTargetType)entry.SelectType)
            {
                case SelectTargetType.Null:
                    if (parentTargets != null)
                    {
                        foreach (var targetRef in parentTargets)
                        {
                            EntityHero target = targetRef;
                            if (self.IsHostTargetValid(target))
                                self.Targets.Add(target);
                        }
                    }
                    break;

                case SelectTargetType.Self:
                    if (self.IsFriendTargetValid(caster))
                        self.Targets.Add(caster);
                    break;

                case SelectTargetType.Friend_Single:
                    // 直接访问字段，避免调用EntityHeroSystem
                    {
                        EntityGroup selectGroup = selectTarget.GroupRef;
                        EntityGroup casterGroup = caster.GroupRef;
                        if (self.IsFriendTargetValid(selectTarget) && selectGroup?.Camp == casterGroup?.Camp)
                            self.Targets.Add(selectTarget);
                    }
                    break;

                case SelectTargetType.Enemy_Single:
                    // 直接访问字段，避免调用EntityHeroSystem
                    {
                        EntityGroup selectGroup2 = selectTarget.GroupRef;
                        EntityGroup casterGroup2 = caster.GroupRef;
                        if (self.IsHostTargetValid(selectTarget) && selectGroup2?.Camp != casterGroup2?.Camp)
                            self.Targets.Add(selectTarget);
                    }
                    break;

                case SelectTargetType.Friend_All:
                    self.FindAllFriends(caster);
                    break;

                case SelectTargetType.Enemy_All:
                    self.FindAllEnemies(caster);
                    break;

                    // 更多目标类型可以在此扩展...
            }
        }

        private static void FindAllFriends(this EntitySpell self, EntityHero caster)
        {
            // 直接访问字段，避免调用EntityHeroSystem
            EntityGroup group = caster.GroupRef;
            if (group == null) return;

            foreach (var entityRef in group.Entitys)
            {
                EntityHero target = entityRef;
                if (self.IsFriendTargetValid(target))
                    self.Targets.Add(target);
            }
        }

        private static void FindAllEnemies(this EntitySpell self, EntityHero caster)
        {
            // 直接访问字段，避免调用EntityHeroSystem
            EntityGroup group = caster.GroupRef;
            EntityGroup otherGroup = group?.OtherGroupRef;
            if (otherGroup == null) return;

            foreach (var entityRef in otherGroup.Entitys)
            {
                EntityHero target = entityRef;
                if (self.IsHostTargetValid(target))
                    self.Targets.Add(target);
            }
        }

        /// <summary>
        /// 友方目标是否有效
        /// </summary>
        public static bool IsFriendTargetValid(this EntitySpell self, EntityHero target)
        {
            if (target == null)
                return false;

            StateComponent stateCom = target.StateCom.Entity;
            if (stateCom != null && stateCom.HasAnyCombatState(self.Entry.TargetStateLimit))
                return false;

            return true;
        }

        /// <summary>
        /// 敌方目标是否有效
        /// </summary>
        public static bool IsHostTargetValid(this EntitySpell self, EntityHero target)
        {
            if (target == null)
                return false;

            StateComponent stateCom = target.StateCom.Entity;
            if (stateCom != null && stateCom.HasAnyCombatState(self.Entry.TargetStateLimit))
                return false;

            return true;
        }

        private static bool TriggerBeforCast(this EntitySpell self)
        {
            if (!self.CanTrigger())
                return true;

            EntityHero caster = self.CasterRef;
            if (caster == null) return false;

            StateComponent stateCom = caster.StateCom.Entity;
            if (stateCom != null && stateCom.HasAnyCombatState(self.Entry.CasterStateLimit))
                return false;

            return true;
        }

        private static void CalEffect(this EntitySpell self)
        {
            foreach (var targetRef in self.Targets)
            {
                EntityHero target = targetRef;
                if (target != null)
                    self.DoEffect(target);
            }
        }

        private static void DoEffect(this EntitySpell self, EntityHero target)
        {
            // 效果计算逻辑
            if (self.Entry?.EffectBlocks == null) return;

            foreach (var blockId in self.Entry.EffectBlocks)
            {
                DREntitySpellBlockEntry blockEntry = DREntitySpellBlockEntryCategory.Instance.Get(blockId);
                if (self.CheckEffectCondition(target, blockEntry))
                {
                    // 计算效果 - 需要实现EffectAction
                    SpellBlockSystem.DoEffect(self, blockEntry, target);
                }
            }
        }

        /// <summary>
        /// 检查效果条件
        /// </summary>
        public static bool CheckEffectCondition(this EntitySpell self, EntityHero target, DREntitySpellBlockEntry blockEntry)
        {
            // if (blockEntry.Probability != 0)
            // {
            //     EntityHero caster = self.CasterRef;
            //     BattleSceneComponent scene = caster?.GetScene();
            //     if (scene != null && scene.Random.Next(1, 10000) > blockEntry.Probability)
            //         return false;
            // }

            if (blockEntry.Condition == null || blockEntry.Condition.Length <= 0)
                return true;

            return true; // 简化版，完整版需要实现CheckCondition逻辑
        }

        private static void CastSubSpell(this EntitySpell self)
        {
            // if (self.Entry?.SubSpell == null)
            //     return;
            //
            // EntityHero caster = self.CasterRef;
            // EntityHero selectTarget = self.SelectTargetRef;
            // if (caster == null) return;
            //
            // // 创建子技能
            // EntitySpell subSpell = caster.AddChild<EntitySpell>();
            // subSpell.Init(caster, selectTarget, self.Entry.SubSpell, self.SpellType, self.Amount);
            // subSpell.FindTargets(self.Targets);
            // subSpell.CalEffect();
            // subSpell.CastSubSpell();
            //
            // // 移除子技能
            // caster.RemoveChild(subSpell.Id);
        }

        private static void CastStart(this EntitySpell self)
        {
            // 技能开始消息
        }

        private static void CastEnd(this EntitySpell self)
        {
            // 技能结束消息
            // 直接访问字段获取场景，避免调用EntityHeroSystem
            EntityHero caster = self.CasterRef;
            if (caster == null) return;

            EntityGroup group = caster.GroupRef;
            var scene = group?.BattleFieldRef.Entity.Scene();
            if (scene == null) return;

            EventSystem.Instance.Publish(scene, new EntityCastSpell()
            {
                CasterId = caster.HeroId,
                SpellId = self.Entry.Id,
                DamageInfos = self.TargetDmgInfos
            });
        }

        /// <summary>
        /// 检查施放者限制
        /// </summary>
        public static ECombatErr CheckCasterLimit(EntityHero caster, DREntitySpellEntry spellEntry, EEntitySpellType eType)
        {
            if (caster == null || spellEntry == null)
                return ECombatErr.CasterState;

            StateComponent stateCom = caster.StateCom.Entity;
            if (stateCom != null && stateCom.HasAnyCombatState(spellEntry.CasterStateLimit))
                return ECombatErr.CasterState;

            return ECombatErr.Success;
        }

        /// <summary>
        /// 检查目标选择
        /// </summary>
        public static ECombatErr CheckTargetSelect(EntityHero caster, EntityHero target, DREntitySpellEntry spellEntry)
        {
            if (caster == null || target == null || spellEntry == null)
                return ECombatErr.TargetState;

            // 直接访问字段获取阵营，避免调用EntityHeroSystem
            EntityGroup casterGroup = caster.GroupRef;
            EntityGroup targetGroup = target.GroupRef;
            ECamp casterCamp = casterGroup?.Camp ?? ECamp.None;
            ECamp targetCamp = targetGroup?.Camp ?? ECamp.None;

            if (casterCamp != targetCamp)
            {
                StateComponent casterStateCom = caster.StateCom.Entity;
                StateComponent targetStateCom = target.StateCom.Entity;

                if (targetStateCom != null && casterStateCom != null)
                {
                    if (targetStateCom.HasCombatState(EEntityState.Stealth) &&
                        !casterStateCom.HasCombatState(EEntityState.AntiHidden))
                    {
                        return ECombatErr.TargetState;
                    }
                }
            }

            StateComponent stateCom = target.StateCom.Entity;
            if (stateCom != null && stateCom.HasAnyCombatState(spellEntry.TargetStateLimit))
                return ECombatErr.TargetState;

            return ECombatErr.Success;
        }

        /// <summary>
        /// 执行技能消耗
        /// </summary>
        public static void DoSpellCost(this EntitySpell self)
        {
            EntityHero caster = self.CasterRef;
            if (caster == null || self.Entry == null) return;

            switch ((SpellCost)self.Entry.CostType)
            {
                case SpellCost.Null:
                    return;

                case SpellCost.Energy:
                    // 内联能量修改逻辑，避免调用EntityHeroSystem
                    int costValue = self.Entry.CostValue;
                    caster.Energy -= costValue;
                    if (caster.Energy > 100)
                        caster.Energy = 100;
                    if (caster.Energy < 0)
                        caster.Energy = 0;
                    break;

                    // 其他消耗类型可以在此扩展...
            }
        }
    }
}
