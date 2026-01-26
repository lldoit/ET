using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 战斗序列器系统 - 管理战斗动作的序列化播放逻辑
    /// </summary>
    [EntitySystemOf(typeof(BattleSequencerComponent))]
    [FriendOf(typeof(BattleSequencerComponent))]
    public static partial class BattleSequencerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleSequencerComponent self)
        {
            self.BatchQueue.Clear();
            self.CallbackRegistry.Clear();
            self.IsPlaying = false;
            self.NextCallbackId = 1;
        }

        [EntitySystem]
        private static void Destroy(this BattleSequencerComponent self)
        {
            self.BatchQueue.Clear();
            self.CallbackRegistry.Clear();
            self.IsPlaying = false;
        }

        [EntitySystem]
        private static void Update(this BattleSequencerComponent self)
        {
            if (self.IsPlaying)
                return;

            if (self.BatchQueue.Count <= 0)
                return;

            self.ProcessNextBatch().NoContext();
        }

        #region 公开API

        /// <summary>
        /// 入队单个动作
        /// 如果在批量收集模式，加入临时列表；否则自动包装为单批次
        /// </summary>
        public static void Enqueue(this BattleSequencerComponent self, ISequenceAction action)
        {
            if (self.IsCollectingBatch)
            {
                // 批量收集模式：加入临时列表
                Log.Info($"[BattleSequencer] Enqueue (收集模式): {action.GetType().Name} CasterId={action.CasterId}");
                self.PendingActions.Add(action);
            }
            else
            {
                // 普通模式：包装为单批次
                Log.Info($"[BattleSequencer] Enqueue (普通模式): {action.GetType().Name} CasterId={action.CasterId}, 队列长度={self.BatchQueue.Count + 1}");
                var batch = new ActionBatch
                {
                    Actions = new List<ISequenceAction> { action }
                };
                self.BatchQueue.Enqueue(batch);
            }
        }

        /// <summary>
        /// 入队一批动作（批次内按CasterId分组并行执行）
        /// </summary>
        public static void EnqueueBatch(this BattleSequencerComponent self, List<ISequenceAction> actions)
        {
            if (actions == null || actions.Count == 0)
                return;

            Log.Info($"[BattleSequencer] EnqueueBatch: {actions.Count} 个动作, 队列长度={self.BatchQueue.Count + 1}");
            var batch = new ActionBatch
            {
                Actions = actions
            };
            self.BatchQueue.Enqueue(batch);
        }

        /// <summary>
        /// 开始批量收集模式
        /// 在此之后调用 Enqueue 的动作会被收集到临时列表
        /// </summary>
        public static void BeginBatch(this BattleSequencerComponent self)
        {
            self.IsCollectingBatch = true;
            self.PendingActions.Clear();
            Log.Info("[BattleSequencer] BeginBatch - 开始批量收集");
        }

        /// <summary>
        /// 结束批量收集模式，将收集的动作作为一个批次入队
        /// </summary>
        public static void EndBatch(this BattleSequencerComponent self)
        {
            if (!self.IsCollectingBatch)
                return;

            self.IsCollectingBatch = false;

            if (self.PendingActions.Count > 0)
            {
                Log.Info($"[BattleSequencer] EndBatch - 提交 {self.PendingActions.Count} 个动作");
                var batch = new ActionBatch
                {
                    Actions = new List<ISequenceAction>(self.PendingActions)
                };
                self.BatchQueue.Enqueue(batch);
            }

            self.PendingActions.Clear();
        }

        /// <summary>
        /// 注册回调函数，返回CallbackId
        /// </summary>
        public static int RegisterCallback(this BattleSequencerComponent self, Action callback)
        {
            int id = self.NextCallbackId++;
            self.CallbackRegistry[id] = callback;
            return id;
        }

        #endregion

        #region 内部逻辑

        /// <summary>
        /// 处理下一个批次
        /// </summary>
        private static async ETTask ProcessNextBatch(this BattleSequencerComponent self)
        {
            if (self.BatchQueue.Count <= 0)
                return;

            self.IsPlaying = true;
            EntityRef<BattleSequencerComponent> selfRef = self;

            try
            {
                ActionBatch batch = self.BatchQueue.Dequeue();

                if (batch.Actions == null || batch.Actions.Count == 0)
                {
                    Log.Warning("[BattleSequencer] ProcessNextBatch: 批次为空");
                    return;
                }

                Log.Info($"[BattleSequencer] ProcessNextBatch: 处理 {batch.Actions.Count} 个动作");

                // 按CasterId分组
                var casterGroups = new Dictionary<int, List<ISequenceAction>>();
                var globalActions = new List<ISequenceAction>(); // CasterId == 0 的全局动作

                foreach (var action in batch.Actions)
                {
                    int casterId = action.CasterId;
                    if (casterId == 0)
                    {
                        globalActions.Add(action);
                    }
                    else
                    {
                        if (!casterGroups.ContainsKey(casterId))
                        {
                            casterGroups[casterId] = new List<ISequenceAction>();
                        }
                        casterGroups[casterId].Add(action);
                    }
                }

                Log.Info($"[BattleSequencer] 全局动作: {globalActions.Count}, 角色组: {casterGroups.Count}");

                // 先执行全局动作（串行）
                foreach (var action in globalActions)
                {
                    self = selfRef;
                    if (self == null || self.IsDisposed)
                        return;

                    await self.ExecuteAction(action);
                }

                self = selfRef;
                if (self == null || self.IsDisposed)
                    return;

                // 并行执行各角色的动作序列
                if (casterGroups.Count > 0)
                {
                    var tasks = new List<ETTask>();
                    foreach (var kvp in casterGroups)
                    {
                        int casterId = kvp.Key;
                        var actions = kvp.Value;
                        Log.Info($"[BattleSequencer] 启动角色任务: CasterId={casterId}, 动作数={actions.Count}");
                        tasks.Add(self.ExecuteCasterActions(casterId, actions));
                    }

                    Log.Info($"[BattleSequencer] 等待 {tasks.Count} 个角色任务完成...");
                    await ETTaskHelper.WaitAll(tasks);
                    Log.Info("[BattleSequencer] 所有角色任务完成");
                }

                Log.Info($"[BattleSequencer] 批次处理完成，队列剩余={self.BatchQueue.Count}");
            }
            catch (Exception e)
            {
                Log.Error($"[BattleSequencer] Error processing batch: {e}");
            }
            finally
            {
                self = selfRef;
                if (self != null && !self.IsDisposed)
                {
                    self.IsPlaying = false;
                }
            }
        }

        /// <summary>
        /// 执行某个角色的动作序列（串行）
        /// </summary>
        private static async ETTask ExecuteCasterActions(this BattleSequencerComponent self, int casterId, List<ISequenceAction> actions)
        {
            EntityRef<BattleSequencerComponent> selfRef = self;

            Log.Info($"[BattleSequencer] ExecuteCasterActions: CasterId={casterId}, 动作数={actions.Count}");

            foreach (var action in actions)
            {
                self = selfRef;
                if (self == null || self.IsDisposed)
                    return;

                Log.Info($"[BattleSequencer] 执行动作: {action.GetType().Name} CasterId={action.CasterId}");
                await self.ExecuteAction(action);
            }
        }

        /// <summary>
        /// 运行角色动作并在完成后调用回调（用于并行执行）
        /// </summary>
        private static async ETTask RunCasterActionsAsync(BattleSequencerComponent self, int casterId, List<ISequenceAction> actions, Action onComplete)
        {
            try
            {
                Log.Info($"[BattleSequencer] RunCasterActionsAsync 开始: CasterId={casterId}");
                await self.ExecuteCasterActions(casterId, actions);
                Log.Info($"[BattleSequencer] RunCasterActionsAsync 结束: CasterId={casterId}");
            }
            catch (Exception e)
            {
                Log.Error($"[BattleSequencer] RunCasterActionsAsync 异常: CasterId={casterId}, Error={e}");
            }
            finally
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 执行单个动作
        /// </summary>
        private static async ETTask ExecuteAction(this BattleSequencerComponent self, ISequenceAction action)
        {
            switch (action)
            {
                case SpellSequenceAction spellAction:
                    await self.ExecuteSpell(spellAction.Data);
                    break;
                case TurnSequenceAction turnAction:
                    await self.ExecuteTurn(turnAction.IsPlayerTurn);
                    break;
                case CallbackSequenceAction callbackAction:
                    self.InvokeCallback(callbackAction.CallbackId);
                    await ETTask.CompletedTask;
                    break;
            }
        }

        /// <summary>
        /// 执行技能效果
        /// </summary>
        private static async ETTask ExecuteSpell(this BattleSequencerComponent self, EntityCastSpell args)
        {
            Log.Info($"[BattleSequencer] ExecuteSpell 开始: CasterId={args.CasterId}, SpellId={args.SpellId}");

            BattleSceneComponent battleScene = self.GetParent<BattleSceneComponent>();
            Scene scene = battleScene.IScene as Scene;

            await SpellEffectHelper.PlaySpellEffect(scene, args);

            Log.Info($"[BattleSequencer] ExecuteSpell 结束: CasterId={args.CasterId}, SpellId={args.SpellId}");
        }

        /// <summary>
        /// 执行回合切换效果
        /// </summary>
        private static async ETTask ExecuteTurn(this BattleSequencerComponent self, bool isPlayerTurn)
        {
            Log.Info($"[BattleSequencer] {(isPlayerTurn ? "Player" : "Enemy")} Turn Begin Visual");
            // 模拟回合切换动画
            await self.Root().GetComponent<TimerComponent>().WaitAsync(500);
        }

        /// <summary>
        /// 执行回调
        /// </summary>
        private static void InvokeCallback(this BattleSequencerComponent self, int callbackId)
        {
            if (self.CallbackRegistry.TryGetValue(callbackId, out var callback))
            {
                self.CallbackRegistry.Remove(callbackId);
                try
                {
                    callback?.Invoke();
                }
                catch (Exception e)
                {
                    Log.Error($"[BattleSequencer] Callback {callbackId} error: {e}");
                }
            }
        }

        #endregion
    }
}
