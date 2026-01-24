using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// EntityGroup系统类 - 队伍逻辑
    /// 遵循ET框架ECS规范
    /// </summary>
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(StateComponent))]
    [EntitySystemOf(typeof(EntityGroup))]
    public static partial class EntityGroupSystem
    {
        [EntitySystem]
        private static void Awake(this EntityGroup self)
        {
            self.Entitys = new List<EntityRef<EntityHero>>();
        }

        [EntitySystem]
        private static void Destroy(this EntityGroup self)
        {

        }

        /// <summary>
        /// 获取阵营
        /// </summary>
        public static ECamp GetCamp(this EntityGroup self)
        {
            return self.Camp;
        }

        /// <summary>
        /// 获取敌方队伍
        /// </summary>
        public static EntityGroup GetOtherGroup(this EntityGroup self)
        {
            return self.OtherGroupRef;
        }

        /// <summary>
        /// 设置敌方队伍
        /// </summary>
        public static void SetOtherGroup(this EntityGroup self, EntityGroup other)
        {
            self.OtherGroupRef = other;
        }

        /// <summary>
        /// 获取战场
        /// </summary>
        public static BattleSceneComponent GetScene(this EntityGroup self)
        {
            return self.BattleFieldRef;
        }

        /// <summary>
        /// 初始化队伍
        /// </summary>
        public static void Init(this EntityGroup self, ECamp camp, BattleSceneComponent battleField, List<int> entityIds = null)
        {
            self.Camp = camp;
            self.BattleFieldRef = battleField;

            if (entityIds == null) return;
            foreach (var entityId in entityIds)
            {
                self.Entitys.Add(self.AddChild<EntityHero, int>(entityId));
            }
        }

        /// <summary>
        /// 队伍是否有效
        /// </summary>
        public static bool IsValid(this EntityGroup self)
        {
            foreach (var entityRef in self.Entitys)
            {
                EntityHero hero = entityRef;
                if (hero == null) continue;
                if (hero.Delete) continue;

                // 内联有效性判断逻辑，避免调用EntityHeroSystem
                StateComponent stateCom = hero.StateCom;
                if (stateCom == null) return true;
                if (!stateCom.HasCombatState(EEntityState.Dead) && !stateCom.HasCombatState(EEntityState.Escape))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取存活实体数量
        /// </summary>
        public static int GetValidEntityNum(this EntityGroup self)
        {
            int num = 0;
            foreach (var entityRef in self.Entitys)
            {
                EntityHero hero = entityRef;
                if (hero == null) continue;
                if (hero.Delete) continue;

                // 内联有效性判断逻辑，避免调用EntityHeroSystem
                StateComponent stateCom = hero.StateCom;
                if (stateCom == null)
                {
                    num++;
                    continue;
                }
                if (!stateCom.HasCombatState(EEntityState.Dead) && !stateCom.HasCombatState(EEntityState.Escape))
                    num++;
            }
            return num;
        }
    }
}
