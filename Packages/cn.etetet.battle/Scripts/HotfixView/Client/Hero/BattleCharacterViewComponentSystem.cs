using System.Collections.Generic;
using Spine;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 战斗角色视图组件System
    /// 处理角色视图的所有逻辑，包括动画播放、面向控制等
    /// </summary>
    [FriendOf(typeof(BattleCharacterViewComponent))]
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(BattleSceneComponent))]
    [FriendOf(typeof(DamageNumberComponent))]
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
            self.StopCurrentAnimTask();
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

                // 保存原始位置
                self.OriginalPosition = characterGO.transform.position;
                
                // Event
                self.Animancer.Animancer.AnimationState.Event -= self.HandleSpineEvent;
                self.Animancer.Animancer.AnimationState.Event += self.HandleSpineEvent;
            }

            // 默认播放待机动画
            self.PlayIdle();
        }
        
        /// <summary>
        /// 处理Spine动画事件
        /// </summary>
        /// <param name="self">视图组件</param>
        /// <param name="entry">动画轨道</param>
        /// <param name="e">Spine事件</param>
        private static void HandleSpineEvent(this BattleCharacterViewComponent self, TrackEntry entry, Spine.Event e)
        {
            if (e.Data.Name == "Attack")
            {
                // Attack事件触发时，处理缓存的伤害信息
                self.TriggerPendingDamage();
            }
        }

        /// <summary>
        /// 触发缓存的伤害效果
        /// </summary>
        /// <param name="self">视图组件</param>
        public static void TriggerPendingDamage(this BattleCharacterViewComponent self)
        {
            if (self.PendingDamageInfos == null || self.PendingDamageInfos.Count == 0)
                return;

            Scene scene = self.Scene();
            if (scene == null)
                return;

            // 触发所有缓存的伤害效果
            foreach (var damageInfo in self.PendingDamageInfos)
            {
                EntityHero target = FindHeroByHeroId(scene, damageInfo.TargetId);
                if (target != null)
                {
                    ProcessTargetHit(target, damageInfo);
                }
            }

            // 清空缓存
            self.PendingDamageInfos.Clear();
        }

        #region 伤害处理相关

        /// <summary>
        /// 根据HeroId查找EntityHero
        /// </summary>
        private static EntityHero FindHeroByHeroId(Scene scene, int heroId)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
                return null;

            // 在红方队伍中查找
            EntityGroup redGroup = battleScene.RedGroup;
            if (redGroup?.Entitys != null)
            {
                foreach (var heroRef in redGroup.Entitys)
                {
                    EntityHero hero = heroRef;
                    if (hero != null && hero.HeroId == heroId)
                        return hero;
                }
            }

            // 在蓝方队伍中查找
            EntityGroup blueGroup = battleScene.BlueGroup;
            if (blueGroup?.Entitys != null)
            {
                foreach (var heroRef in blueGroup.Entitys)
                {
                    EntityHero hero = heroRef;
                    if (hero != null && hero.HeroId == heroId)
                        return hero;
                }
            }

            return null;
        }

        /// <summary>
        /// 检查目标是否死亡
        /// </summary>
        private static bool IsDead(EntityHero hero)
        {
            if (hero == null)
                return true;

            AttComponent attCom = hero.AttCom;
            if (attCom == null)
                return false;

            int currentHp = attCom.GetAttValue(EAttType.CurHP);
            return currentHp <= 0;
        }

        /// <summary>
        /// 处理目标受击效果（飘字、动画）
        /// </summary>
        private static void ProcessTargetHit(EntityHero target, DamageInfo damageInfo)
        {
            BattleCharacterViewComponent targetView = target?.GetComponent<BattleCharacterViewComponent>();
            if (targetView == null)
                return;

            // 获取飘字组件
            Scene scene = target.Scene();
            DamageNumberComponent dnComponent = scene?.GetComponent<DamageNumberComponent>();

            // 获取目标世界坐标（身体中间）
            Vector3 worldPos = targetView.CharacterGO.transform.position;
            worldPos.y += targetView.Animancer.Renderer.bounds.size.y * 0.5f;

            // 检查是否造成伤害
            bool isDamage = (damageInfo.SpellResult & (int)SpellResult.Damage) != 0;
            bool isCrit = (damageInfo.SpellResult & (int)SpellResult.Crit) != 0;
            bool isHeal = (damageInfo.SpellResult & (int)SpellResult.Heal) != 0;

            // 显示飘字（使用队列方法，自动处理延迟）
            if (dnComponent != null && dnComponent.IsInitialized)
            {
                if (isHeal)
                {
                    dnComponent.QueueHeal(worldPos, damageInfo.Damage);
                }
                else if (isCrit)
                {
                    dnComponent.QueueCriticalDamage(worldPos, damageInfo.Damage);
                }
                else if (isDamage)
                {
                    dnComponent.QueueNormalDamage(worldPos, damageInfo.Damage);
                }
            }

            if (isDamage)
            {
                // 检查是否死亡
                if (IsDead(target))
                {
                    // 播放死亡动画（不等待）
                    targetView.PlayDie().NoContext();
                }
                else
                {
                    // 播放受击动画（不等待）
                    targetView.PlayHit().NoContext();
                }
            }
        }

        #endregion

        /// <summary>
        /// 停止当前正在播放的动画任务
        /// </summary>
        private static void StopCurrentAnimTask(this BattleCharacterViewComponent self)
        {
            if (self.CurrentAnimTask != null && !self.CurrentAnimTask.IsCompleted)
            {
                self.CurrentAnimTask.SetResult();
            }
            self.CurrentAnimTask = null;
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

            // 防重入：如果当前已经在播放攻击动画，直接返回
            // if (self.CurrentAnimTask != null && !self.CurrentAnimTask.IsCompleted && self.CurrentAnimState == EBattleAnimState.Attack)
            // {
            //     return;
            // }

            // 打断上一个动画
            //self.StopCurrentAnimTask();

            self.CurrentAnimState = EBattleAnimState.Attack;

            // 创建EntityRef以便await后安全访问
            EntityRef<BattleCharacterViewComponent> selfRef = self;

            var tcs = ETTask.Create(true);
            self.CurrentAnimTask = tcs;

            //bool isCompleted = false;
            self.Animancer.PlayAttack(() =>
            {
                // if (self.IsDisposed || self.CurrentAnimTask != tcs) return;
                // if (isCompleted) return;
                // isCompleted = true;
                tcs.SetResult();
            });

            await tcs;

            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                // 如果当前任务还是这个任务（没有被新任务打断），则自然结束
                if (self.CurrentAnimTask == tcs)
                {
                    self.CurrentAnimTask = null;
                    self.CurrentAnimState = EBattleAnimState.Idle;
                }
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

            // 防重入
            // if (self.CurrentAnimTask != null && !self.CurrentAnimTask.IsCompleted && self.CurrentAnimState == EBattleAnimState.Spell)
            // {
            //     return;
            // }

            // 打断上一个动画
            //self.StopCurrentAnimTask();

            self.CurrentAnimState = EBattleAnimState.Spell;

            // 创建EntityRef以便await后安全访问
            EntityRef<BattleCharacterViewComponent> selfRef = self;

            var tcs = ETTask.Create(true);
            self.CurrentAnimTask = tcs;

            //bool isCompleted = false;
            self.Animancer.PlaySpell(() =>
            {
            //     if (self.IsDisposed || self.CurrentAnimTask != tcs) return;
            //     if (isCompleted) return;
            //     isCompleted = true;
                tcs.SetResult();
            });

            await tcs;

            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                if (self.CurrentAnimTask == tcs)
                {
                    self.CurrentAnimTask = null;
                    self.CurrentAnimState = EBattleAnimState.Idle;
                }
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

            // 防重入
            // if (self.CurrentAnimTask != null && !self.CurrentAnimTask.IsCompleted && self.CurrentAnimState == EBattleAnimState.Hit)
            // {
            //     return;
            // }

            // 打断上一个动画
            self.StopCurrentAnimTask();

            self.CurrentAnimState = EBattleAnimState.Hit;

            // 创建EntityRef以便await后安全访问
            EntityRef<BattleCharacterViewComponent> selfRef = self;

            var tcs = ETTask.Create(true);
            self.CurrentAnimTask = tcs;

            //bool isCompleted = false;
            self.Animancer.PlayHit(() =>
            {
                // if (self.IsDisposed || self.CurrentAnimTask != tcs) return;
                // if (isCompleted) return;
                // isCompleted = true;
                tcs.SetResult();
            });

            await tcs;

            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                if (self.CurrentAnimTask == tcs)
                {
                    self.CurrentAnimTask = null;
                    self.CurrentAnimState = EBattleAnimState.Idle;
                }
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
            
            // 防重入
            if (self.CurrentAnimState == EBattleAnimState.Hit)
            {
                return;
            }

            // 死亡动画不防重入（确保能死透），但要打断之前的
            //self.StopCurrentAnimTask();

            self.CurrentAnimState = EBattleAnimState.Die;

            // 创建EntityRef以便await后安全访问
            EntityRef<BattleCharacterViewComponent> selfRef = self;

            var tcs = ETTask.Create(true);
            self.CurrentAnimTask = tcs;

            //bool isCompleted = false;
            self.Animancer.PlayDie(() =>
            {
                // if (self.IsDisposed || self.CurrentAnimTask != tcs) return;
                // if (isCompleted) return;
                // isCompleted = true;
                tcs.SetResult();
            });

            await tcs;

            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                // 死亡后保持死亡状态
                if (self.CurrentAnimTask == tcs)
                {
                    self.CurrentAnimTask = null;
                    self.CurrentAnimState = EBattleAnimState.Die;
                }
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

        #region 移动相关

        /// <summary>
        /// 移动到指定位置（用于近战攻击）
        /// </summary>
        /// <param name="self">视图组件</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="moveSpeed">移动速度（单位/秒）</param>
        public static async ETTask MoveToPosition(this BattleCharacterViewComponent self, Vector3 targetPosition, float moveSpeed = 5f)
        {
            if (self.CharacterGO == null)
                return;

            EntityRef<BattleCharacterViewComponent> selfRef = self;

            Vector3 startPos = self.CharacterGO.transform.position;
            float distance = Vector3.Distance(startPos, targetPosition);
            float duration = distance / moveSpeed;

            // 播放跑步动画
            self.PlayRun();

            // 移动过程
            float elapsed = 0f;
            while (elapsed < duration)
            {
                self = selfRef;
                if (self == null || self.IsDisposed || self.CharacterGO == null)
                    return;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                self.CharacterGO.transform.position = Vector3.Lerp(startPos, targetPosition, t);

                await self.Root().GetComponent<TimerComponent>().WaitFrameAsync();
            }

            // 确保到达目标位置
            self = selfRef;
            if (self != null && !self.IsDisposed && self.CharacterGO != null)
            {
                self.CharacterGO.transform.position = targetPosition;
            }
        }

        /// <summary>
        /// 返回原始位置（近战攻击后）
        /// </summary>
        /// <param name="self">视图组件</param>
        /// <param name="moveSpeed">移动速度（单位/秒）</param>
        public static async ETTask MoveBack(this BattleCharacterViewComponent self, float moveSpeed = 5f)
        {
            if (self.CharacterGO == null)
                return;

            await self.MoveToPosition(self.OriginalPosition, moveSpeed);

            // 返回后播放待机动画
            EntityRef<BattleCharacterViewComponent> selfRef = self;
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                self.PlayIdle();
            }
        }

        #endregion
    }
}
