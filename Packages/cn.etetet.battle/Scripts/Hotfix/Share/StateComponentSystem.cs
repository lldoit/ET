using System;
using System.Collections.Specialized;

namespace ET
{
    /// <summary>
    /// StateComponent系统类 - 状态组件逻辑
    /// 遵循ET框架ECS规范：所有逻辑放在System类中
    /// </summary>
    [FriendOf(typeof(StateComponent))]
    [EntitySystemOf(typeof(StateComponent))]
    [FriendOfAttribute(typeof(ET.EntityHero))]
    public static partial class StateComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this StateComponent self)
        {
            self.State = new BitVector32(0);
            self.Count = new sbyte[(int)EEntityState.End];
        }

        [EntitySystem]
        private static void Destroy(this StateComponent self)
        {
            self.Reset();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 设置所属实体
        /// </summary>
        public static void SetOwner(this StateComponent self, EntityHero owner)
        {
            self.OwnerRef = owner;
        }

        /// <summary>
        /// 获取所属实体
        /// </summary>
        public static EntityHero GetOwner(this StateComponent self)
        {
            return self.OwnerRef;
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public static void Reset(this StateComponent self)
        {
            self.State = default;
            if (self.Count != null)
            {
                Array.Clear(self.Count, 0, self.Count.Length);
            }
        }

        /// <summary>
        /// 初始化状态
        /// </summary>
        public static void InitState(this StateComponent self, int states)
        {
            self.State = new BitVector32(states);
        }

        #endregion

        #region 状态检查

        /// <summary>
        /// 检查是否有指定状态
        /// </summary>
        private static bool HasState(this StateComponent self, int eState)
        {
            return self.State[1 << eState];
        }

        /// <summary>
        /// 检查是否有任意指定状态
        /// </summary>
        private static bool HasStateAny(this StateComponent self, uint flags)
        {
            return (self.State.Data & flags) != 0;
        }

        /// <summary>
        /// 检查是否有全部指定状态
        /// </summary>
        private static bool HasStateAll(this StateComponent self, uint flags)
        {
            return (self.State.Data & flags) == flags;
        }

        /// <summary>
        /// 战斗状态判断
        /// </summary>
        public static bool HasCombatState(this StateComponent self, EEntityState state)
        {
            return self.HasState((int)state);
        }

        /// <summary>
        /// 检查是否有任意战斗状态
        /// </summary>
        public static bool HasAnyCombatState(this StateComponent self, uint flags)
        {
            return self.HasStateAny(flags);
        }

        /// <summary>
        /// 检查是否有任意战斗状态(int版本)
        /// </summary>
        public static bool HasAnyCombatState(this StateComponent self, int flags)
        {
            return self.HasStateAny((uint)flags);
        }

        /// <summary>
        /// 检查是否有全部战斗状态
        /// </summary>
        public static bool HasAllCombatState(this StateComponent self, uint flags)
        {
            return self.HasStateAll(flags);
        }

        /// <summary>
        /// 获得状态添加次数
        /// </summary>
        public static sbyte GetStateCount(this StateComponent self, int eState)
        {
            return self.Count[eState];
        }

        #endregion

        #region 状态修改

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="self">状态组件</param>
        /// <param name="eState">状态类型</param>
        /// <param name="count">添加次数</param>
        /// <returns>返回是否为新添加的状态</returns>
        public static bool AddState(this StateComponent self, int eState, sbyte count)
        {
            int state = eState;
            int stateFlag = 1 << state;

            if (self.State[stateFlag])
            {
                self.Count[state] += count;
                return false;
            }
            else
            {
                self.State[stateFlag] = true;
                self.Count[state] += count;
                return true;
            }
        }

        /// <summary>
        /// 删除状态
        /// </summary>
        /// <param name="self">状态组件</param>
        /// <param name="eState">状态类型</param>
        /// <returns>返回是否状态删除完毕</returns>
        public static bool DecState(this StateComponent self, int eState)
        {
            int state = eState;
            int stateFlag = 1 << state;

            if (!self.State[stateFlag])
                return true;

            if ((self.Count[state] -= 1) <= 0)
            {
                self.State[stateFlag] = false;
                self.Count[state] = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 添加战斗状态
        /// </summary>
        public static void AddCombatState(this StateComponent self, EEntityState state, sbyte count = 1)
        {
            if (self.AddState((int)state, count))
            {
                EntityHero owner = self.OwnerRef;
                if (owner == null) return;

                // 直接通过GroupRef获取场景，避免调用EntityHeroSystem
                EntityGroup group = owner.GroupRef;
                BattleSceneComponent scene = group?.BattleFieldRef;
                if (scene == null) return;

                EventSystem.Instance.Publish(scene, new SetEntityState
                {
                    EntityId = owner.HeroId,
                    state = (int)state,
                });
            }
        }

        /// <summary>
        /// 移除战斗状态
        /// </summary>
        public static void DecCombatState(this StateComponent self, EEntityState state)
        {
            if (self.DecState((int)state))
            {
                EntityHero owner = self.OwnerRef;
                if (owner == null) return;

                // 直接通过GroupRef获取场景，避免调用EntityHeroSystem
                EntityGroup group = owner.GroupRef;
                BattleSceneComponent scene = group?.BattleFieldRef;
                if (scene == null) return;

                EventSystem.Instance.Publish(scene, new UnsetEntityState
                {
                    EntityId = owner.HeroId,
                    state = (int)state,
                });
            }
        }

        #endregion
    }
}
