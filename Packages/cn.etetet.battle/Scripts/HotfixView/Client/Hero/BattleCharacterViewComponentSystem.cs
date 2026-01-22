using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 战斗角色视图组件System
    /// 处理角色视图的所有逻辑，包括动画播放、面向控制等
    /// </summary>
    [FriendOf(typeof(BattleCharacterViewComponent))]
    [EntitySystemOf(typeof(BattleCharacterViewComponent))]
    public static partial class BattleCharacterViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleCharacterViewComponent self)
        {
            self.CurrentAnimState = EBattleAnimState.None;
            self.FacingLeft = false;
        }

        [EntitySystem]
        private static void Destroy(this BattleCharacterViewComponent self)
        {
            // 销毁时清理GameObject和引用
            if (self.CharacterGO != null)
            {
                // 先禁用使其立即不可见（Destroy是延迟销毁的）
                self.CharacterGO.SetActive(false);
                // 销毁角色GameObject（在AfterEntityHeroCreate_CreateHeroView中Instantiate创建的）
                UnityEngine.Object.Destroy(self.CharacterGO);
                self.CharacterGO = null;
            }
            self.Animancer = null;
            self.CurrentAnimState = EBattleAnimState.None;
        }

        /// <summary>
        /// 初始化视图组件
        /// </summary>
        /// <param name="self">视图组件</param>
        /// <param name="characterGO">角色GameObject</param>
        /// <param name="owner">所属英雄实体</param>
        public static void Initialize(this BattleCharacterViewComponent self, GameObject characterGO, int slotIndex)
        {
            self.CharacterGO = characterGO;

            if (characterGO != null)
            {
                self.Animancer = characterGO.GetComponent<BattleCharacterAnimancer>();
                if (self.Animancer == null)
                {
                    self.Animancer = characterGO.GetComponentInChildren<BattleCharacterAnimancer>();
                }
                
                self.Animancer.Renderer.sortingOrder = slotIndex;
            }

            // 默认播放待机动画
            self.PlayIdle();
        }

        /// <summary>
        /// 设置面向方向
        /// </summary>
        /// <param name="self">视图组件</param>
        /// <param name="facingLeft">是否面向左侧</param>
        public static void SetFacing(this BattleCharacterViewComponent self, bool facingLeft)
        {
            self.FacingLeft = facingLeft;
            if (self.Animancer != null)
            {
                self.Animancer.FacingLeft = facingLeft;
            }
        }

        /// <summary>
        /// 播放待机动画
        /// </summary>
        public static void PlayIdle(this BattleCharacterViewComponent self)
        {
            if (self.Animancer != null)
            {
                self.Animancer.PlayIdle();
                self.CurrentAnimState = EBattleAnimState.Idle;
            }
        }

        /// <summary>
        /// 播放跑步动画
        /// </summary>
        public static void PlayRun(this BattleCharacterViewComponent self)
        {
            if (self.Animancer != null)
            {
                self.Animancer.PlayRun();
                self.CurrentAnimState = EBattleAnimState.Run;
            }
        }

        /// <summary>
        /// 播放攻击动画（异步，等待动画完成）
        /// </summary>
        public static async ETTask PlayAttack(this BattleCharacterViewComponent self)
        {
            if (self.Animancer == null)
            {
                return;
            }

            self.CurrentAnimState = EBattleAnimState.Attack;

            // 创建EntityRef以便await后安全访问
            EntityRef<BattleCharacterViewComponent> selfRef = self;

            var tcs = ETTask.Create(true);
            self.Animancer.PlayAttack(() =>
            {
                tcs.SetResult();
            });

            await tcs;

            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                self.CurrentAnimState = EBattleAnimState.Idle;
            }
        }

        /// <summary>
        /// 播放技能动画（异步，等待动画完成）
        /// </summary>
        public static async ETTask PlaySpell(this BattleCharacterViewComponent self)
        {
            if (self.Animancer == null)
            {
                return;
            }

            self.CurrentAnimState = EBattleAnimState.Spell;

            // 创建EntityRef以便await后安全访问
            EntityRef<BattleCharacterViewComponent> selfRef = self;

            var tcs = ETTask.Create(true);
            self.Animancer.PlaySpell(() =>
            {
                tcs.SetResult();
            });

            await tcs;

            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                self.CurrentAnimState = EBattleAnimState.Idle;
            }
        }

        /// <summary>
        /// 播放受击动画（异步，等待动画完成）
        /// </summary>
        public static async ETTask PlayHit(this BattleCharacterViewComponent self)
        {
            if (self.Animancer == null)
            {
                return;
            }

            self.CurrentAnimState = EBattleAnimState.Hit;

            // 创建EntityRef以便await后安全访问
            EntityRef<BattleCharacterViewComponent> selfRef = self;

            var tcs = ETTask.Create(true);
            self.Animancer.PlayHit(() =>
            {
                tcs.SetResult();
            });

            await tcs;

            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                self.CurrentAnimState = EBattleAnimState.Idle;
            }
        }

        /// <summary>
        /// 播放死亡动画（异步，等待动画完成）
        /// </summary>
        public static async ETTask PlayDie(this BattleCharacterViewComponent self)
        {
            if (self.Animancer == null)
            {
                return;
            }

            self.CurrentAnimState = EBattleAnimState.Die;

            // 创建EntityRef以便await后安全访问
            EntityRef<BattleCharacterViewComponent> selfRef = self;

            var tcs = ETTask.Create(true);
            self.Animancer.PlayDie(() =>
            {
                tcs.SetResult();
            });

            await tcs;

            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                // 死亡后保持死亡状态
                self.CurrentAnimState = EBattleAnimState.Die;
            }
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        public static void StopAll(this BattleCharacterViewComponent self)
        {
            if (self.Animancer != null)
            {
                self.Animancer.StopAll();
                self.CurrentAnimState = EBattleAnimState.None;
            }
        }

        /// <summary>
        /// 检查当前是否正在播放指定动画
        /// </summary>
        public static bool IsPlayingAnim(this BattleCharacterViewComponent self, EBattleAnimState state)
        {
            return self.CurrentAnimState == state;
        }

        /// <summary>
        /// 检查当前是否处于动作动画中（Attack/Spell/Hit）
        /// </summary>
        public static bool IsInActionAnim(this BattleCharacterViewComponent self)
        {
            return self.CurrentAnimState == EBattleAnimState.Attack ||
                   self.CurrentAnimState == EBattleAnimState.Spell ||
                   self.CurrentAnimState == EBattleAnimState.Hit;
        }
    }
}
