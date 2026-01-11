using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// EntityGroup系统类 - 队伍逻辑
    /// 遵循ET框架ECS规范
    /// </summary>
    [FriendOf(typeof(EntityGroup))]
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
        public static void Init(this EntityGroup self, ECamp camp, BattleSceneComponent battleField, List<int> entityIds)
        {
            self.Camp = camp;
            self.BattleFieldRef = battleField;

            foreach (int entityId in entityIds)
            {
                self.Entitys.Add(self.AddComponent<EntityHero, int>(entityId));
            }
        }

        /// <summary>
        /// 队伍是否有效
        /// </summary>
        public static bool IsValid(this EntityGroup self)
        {
            foreach (var entity in self.Entitys)
            {
                if (entity.Entity.IsValid())
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
            foreach (var entity in self.Entitys)
            {
                if (entity.Entity.IsValid())
                    num++;
            }
            return num;
        }
    }
}
