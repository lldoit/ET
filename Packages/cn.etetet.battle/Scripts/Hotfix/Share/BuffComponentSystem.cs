using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// BuffComponent系统类 - Buff管理逻辑
    /// 遵循ET框架ECS规范
    /// </summary>
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(EntityHero))]
    [EntitySystemOf(typeof(BuffComponent))]
    public static partial class BuffComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this BuffComponent self)
        {
            self.Buffs = new List<BuffData>();
        }

        [EntitySystem]
        private static void Destroy(this BuffComponent self)
        {
            self.Buffs?.Clear();
            self.OwnerRef = default;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 设置所属实体
        /// </summary>
        public static void SetOwner(this BuffComponent self, EntityHero owner)
        {
            self.OwnerRef = owner;
        }

        /// <summary>
        /// 获取所属实体
        /// </summary>
        public static EntityHero GetOwner(this BuffComponent self)
        {
            return self.OwnerRef;
        }

        #endregion

        #region Buff管理

        /// <summary>
        /// 添加Buff（效果叠加机制）
        /// </summary>
        /// <param name="self">Buff组件</param>
        /// <param name="buffId">Buff配置Id</param>
        /// <param name="duration">持续回合数</param>
        /// <param name="casterId">施放者Id</param>
        /// <returns>是否为新添加的Buff</returns>
        public static bool AddBuff(this BuffComponent self, int buffId, int duration, long casterId)
        {
            // 检查是否已存在相同Buff
            for (int i = 0; i < self.Buffs.Count; i++)
            {
                if (self.Buffs[i].BuffId == buffId)
                {
                    // 效果叠加：增加层数
                    var existingBuff = self.Buffs[i];
                    existingBuff.StackCount++;
                    // 刷新持续时间（取较大值）
                    if (duration > existingBuff.RemainingTurns)
                    {
                        existingBuff.RemainingTurns = duration;
                    }
                    self.Buffs[i] = existingBuff;

                    Log.Info($"[BuffComponent] Buff叠加 - BuffId: {buffId}, 层数: {existingBuff.StackCount}");

                    // 发布Buff叠加事件
                    self.PublishBuffEvent(buffId, existingBuff.StackCount, true);
                    return false;
                }
            }

            // 新增Buff
            var newBuff = new BuffData
            {
                BuffId = buffId,
                StackCount = 1,
                RemainingTurns = duration,
                CasterId = casterId
            };
            self.Buffs.Add(newBuff);

            Log.Info($"[BuffComponent] 添加Buff - BuffId: {buffId}, 持续: {duration}回合");

            // 发布Buff添加事件
            self.PublishBuffEvent(buffId, 1, true);
            return true;
        }

        /// <summary>
        /// 移除指定Buff
        /// </summary>
        /// <param name="self">Buff组件</param>
        /// <param name="buffId">Buff配置Id</param>
        /// <param name="removeAllStacks">是否移除所有层数</param>
        /// <returns>是否成功移除</returns>
        public static bool RemoveBuff(this BuffComponent self, int buffId, bool removeAllStacks = true)
        {
            for (int i = self.Buffs.Count - 1; i >= 0; i--)
            {
                if (self.Buffs[i].BuffId == buffId)
                {
                    if (removeAllStacks || self.Buffs[i].StackCount <= 1)
                    {
                        self.Buffs.RemoveAt(i);
                        Log.Info($"[BuffComponent] 移除Buff - BuffId: {buffId}");
                        self.PublishBuffEvent(buffId, 0, false);
                    }
                    else
                    {
                        var buff = self.Buffs[i];
                        buff.StackCount--;
                        self.Buffs[i] = buff;
                        Log.Info($"[BuffComponent] Buff减层 - BuffId: {buffId}, 剩余层数: {buff.StackCount}");
                        self.PublishBuffEvent(buffId, buff.StackCount, true);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查是否拥有指定Buff
        /// </summary>
        public static bool HasBuff(this BuffComponent self, int buffId)
        {
            foreach (var buff in self.Buffs)
            {
                if (buff.BuffId == buffId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取Buff叠加层数
        /// </summary>
        public static int GetBuffStackCount(this BuffComponent self, int buffId)
        {
            foreach (var buff in self.Buffs)
            {
                if (buff.BuffId == buffId)
                    return buff.StackCount;
            }
            return 0;
        }

        /// <summary>
        /// 获取叠加后的效果值
        /// </summary>
        /// <param name="self">Buff组件</param>
        /// <param name="buffId">Buff配置Id</param>
        /// <param name="baseValue">基础效果值</param>
        /// <returns>叠加后的效果值</returns>
        public static int GetStackedValue(this BuffComponent self, int buffId, int baseValue)
        {
            int stackCount = self.GetBuffStackCount(buffId);
            return baseValue * stackCount;
        }

        #endregion

        #region 回合处理

        /// <summary>
        /// 回合开始时处理
        /// </summary>
        public static void OnTurnStart(this BuffComponent self)
        {
            // 触发回合开始效果的Buff
            foreach (var buff in self.Buffs)
            {
                // TODO: 根据Buff配置执行回合开始效果
            }
        }

        /// <summary>
        /// 回合结束时处理（减少持续回合数）
        /// </summary>
        public static void OnTurnEnd(this BuffComponent self)
        {
            for (int i = self.Buffs.Count - 1; i >= 0; i--)
            {
                var buff = self.Buffs[i];

                // 永久Buff不减少回合
                if (buff.RemainingTurns == -1)
                    continue;

                buff.RemainingTurns--;

                if (buff.RemainingTurns <= 0)
                {
                    // Buff到期移除
                    self.Buffs.RemoveAt(i);
                    Log.Info($"[BuffComponent] Buff到期移除 - BuffId: {buff.BuffId}");
                    self.PublishBuffEvent(buff.BuffId, 0, false);
                }
                else
                {
                    self.Buffs[i] = buff;
                }
            }
        }

        /// <summary>
        /// 清除所有Buff
        /// </summary>
        public static void ClearAllBuffs(this BuffComponent self)
        {
            foreach (var buff in self.Buffs)
            {
                self.PublishBuffEvent(buff.BuffId, 0, false);
            }
            self.Buffs.Clear();
        }

        #endregion

        #region 事件发布

        /// <summary>
        /// 发布Buff事件
        /// </summary>
        private static void PublishBuffEvent(this BuffComponent self, int buffId, int stackCount, bool isAdd)
        {
            EntityHero owner = self.OwnerRef;
            if (owner == null)
                return;

            EntityGroup group = owner.GroupRef;
            BattleSceneComponent battleScene = group?.BattleFieldRef;
            if (battleScene == null)
                return;

            Scene scene = battleScene.IScene as Scene;
            if (isAdd)
            {
                EventSystem.Instance.Publish(scene, new BuffAddedEvent
                {
                    TargetId = owner.Id,
                    BuffId = buffId,
                    StackCount = stackCount
                });
            }
            else
            {
                EventSystem.Instance.Publish(scene, new BuffRemovedEvent
                {
                    TargetId = owner.Id,
                    BuffId = buffId
                });
            }
        }

        #endregion
    }
}
