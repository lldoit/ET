using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// EntityHero系统类 - 英雄实体逻辑
    /// 遵循ET框架ECS规范
    /// </summary>
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(BattleSceneComponent))]
    [EntitySystemOf(typeof(EntityHero))]
    public static partial class EntityHeroSystem
    {
        [EntitySystem]
        private static void Awake(this EntityHero self, int configId)
        {
            self.EntryId = configId;
            self.Entry = DREntityBaseEntryCategory.Instance.Get(configId);

            // 生成唯一运行时ID (从BattleSceneComponent获取)
            EntityGroup group = self.GetParent<EntityGroup>();
            BattleSceneComponent battleScene = group.GetParent<BattleSceneComponent>();
            self.HeroId = ++battleScene.NextHeroId;

            var attEntry = DREntityAttEntryCategory.Instance.Get(self.Entry.EntityAttEntry);
            self.AttCom = self.AddComponent<AttComponent, DREntityAttEntry>(attEntry);
            self.StateCom = self.AddComponent<StateComponent>();
        }

        [EntitySystem]
        private static void Destroy(this EntityHero self)
        {
            self.Reset();
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public static void Reset(this EntityHero self)
        {
            self.HeroId = 0;
            self.Level = 0;
            self.WakeUpLv = 0;
            self.Score = 0;
            self.Station = 0;
            self.Delete = false;
            self.Energy = 0;
            self.EntryId = 0;
            self.GroupRef = default;
        }

        /// <summary>
        /// 获取所属队伍
        /// </summary>
        public static EntityGroup GetGroup(this EntityHero self)
        {
            return self.GroupRef;
        }

        /// <summary>
        /// 设置所属队伍
        /// </summary>
        public static void SetGroup(this EntityHero self, EntityGroup group)
        {
            self.GroupRef = group;
        }

        /// <summary>
        /// 获取战场
        /// </summary>
        public static BattleSceneComponent GetScene(this EntityHero self)
        {
            EntityGroup group = self.GroupRef;
            return group?.GetScene();
        }

        /// <summary>
        /// 获取阵营
        /// </summary>
        public static ECamp GetCamp(this EntityHero self)
        {
            EntityGroup group = self.GroupRef;
            return group?.GetCamp() ?? ECamp.None;
        }

        /// <summary>
        /// 获取敌方队伍
        /// </summary>
        public static EntityGroup GetOtherGroup(this EntityHero self)
        {
            EntityGroup group = self.GroupRef;
            return group?.GetOtherGroup();
        }

        /// <summary>
        /// 是否有效(存活且未删除)
        /// </summary>
        public static bool IsValid(this EntityHero self)
        {
            if (self.Delete) return false;
            StateComponent stateCom = self.StateCom;
            if (stateCom == null) return true;
            return !stateCom.HasCombatState(EEntityState.Dead) &&
                   !stateCom.HasCombatState(EEntityState.Escape);
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static int GetAttValue(this EntityHero self, EAttType type)
        {
            AttComponent attCom = self.AttCom;
            return attCom?.GetAttValue(type) ?? 0;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static int GetAttValue(this EntityHero self, int type)
        {
            AttComponent attCom = self.AttCom;
            return attCom?.GetAttValue(type) ?? 0;
        }

        /// <summary>
        /// 修改能量
        /// </summary>
        public static void ModEnergy(this EntityHero self, int modVal)
        {
            self.Energy += modVal;
            if (self.Energy > 100)
                self.Energy = 100;
            if (self.Energy < 0)
                self.Energy = 0;
        }

        public static void OnDead(this EntityHero self, EntityHero caster)
        {
            StateComponent stateCom = self.StateCom;
            if (stateCom != null && stateCom.HasCombatState(EEntityState.Dead))
                return;

            AttComponent attCom = self.AttCom;
            attCom?.SetCurHP(0);

            //Group.OnEntityDead(this);
            // CombatCom.OnDead();
        }

        public static int CastActiveSpell(this EntityHero self, DREntitySpellEntry spellEntry, EntityHero target, int amount = 0)
        {
            // ECombatErr dwErrCode = EntitySpell.CheckCasterLimit(Owner, spellEntry, eType);
            // if (ECombatErr.Success != dwErrCode)
            //     return dwErrCode;

            var spell = self.AddChild<EntitySpell>();
            spell.Init(self, target, spellEntry, amount);
            spell.Cast();

            return 0;
        }

        /// <summary>
        /// 施放主动技能（静默模式，不发布事件，返回事件数据）
        /// 用于批量收集多个技能后统一发布
        /// </summary>
        /// <returns>元组：(错误码, 技能事件数据)</returns>
        public static (ECombatErr, EntityCastSpell?) CastActiveSpellSilent(this EntityHero self, DREntitySpellEntry spellEntry, EntityHero target, int amount = 0)
        {
            /*ECombatErr err = EntitySpellSystem.CheckCasterLimit(self, spellEntry, EEntitySpellType.Normal);
            if (err != ECombatErr.Success)
                return (err, null);

            err = EntitySpellSystem.CheckTargetSelect(self, target, spellEntry);
            if (err != ECombatErr.Success)
                return (err, null);*/

            var spell = self.AddChild<EntitySpell>();
            spell.Init(self, target, spellEntry, amount);

            var result = spell.CastSilent();
            return result;
        }
    }
}
