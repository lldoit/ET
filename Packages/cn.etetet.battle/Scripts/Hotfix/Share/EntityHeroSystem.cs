using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// EntityHero系统类 - 英雄实体逻辑
    /// 遵循ET框架ECS规范
    /// </summary>
    [FriendOf(typeof(EntityHero))]
    [EntitySystemOf(typeof(EntityHero))]
    public static partial class EntityHeroSystem
    {
        [EntitySystem]
        private static void Awake(this EntityHero self, int id)
        {
            self.HeroId = id;
            self.Entry = DREntityBaseEntryCategory.Instance.Get(id);
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
            if (self.StateCom == null) return true;
            return !self.StateCom.Entity.HasCombatState(EEntityState.Dead) &&
                   !self.StateCom.Entity.HasCombatState(EEntityState.Escape);
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static int GetAttValue(this EntityHero self, EAttType type)
        {
            return self.AttCom.Entity?.GetAttValue(type) ?? 0;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static int GetAttValue(this EntityHero self, int type)
        {
            return self.AttCom.Entity?.GetAttValue(type) ?? 0;
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

        public static int CastActiveSpell(this EntityHero self, DREntitySpellEntry spellEntry, EntityHero target, int amount = 0)
        {
            // ECombatErr dwErrCode = EntitySpell.CheckCasterLimit(Owner, spellEntry, eType);
            // if (ECombatErr.Success != dwErrCode)
            //     return dwErrCode;
            
            EntitySpell spell = new();
            spell.Init(self, target, spellEntry, amount);
            spell.Cast();
            
            return 0;
        }
    }
}
