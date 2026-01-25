
using System;
using UnityEngine;
using ET;

namespace ET.Client
{
    [EntitySystemOf(typeof(BattleVisualQueueComponent))]
    [FriendOf(typeof(BattleVisualQueueComponent))]
    public static partial class BattleVisualQueueComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleVisualQueueComponent self)
        {
            self.Actions.Clear();
            self.IsPlaying = false;
            self.CurrentAction = null;
        }

        [EntitySystem]
        private static void Destroy(this BattleVisualQueueComponent self)
        {
            self.Actions.Clear();
            self.IsPlaying = false;
            self.CurrentAction = null;
        }

        [EntitySystem]
        private static void Update(this BattleVisualQueueComponent self)
        {
            if (self.IsPlaying)
                return;

            if (self.Actions.Count <= 0)
                return;

            self.ProcessNext().NoContext();
        }

        public static void Enqueue(this BattleVisualQueueComponent self, IVisualAction action)
        {
            self.Actions.Enqueue(action);
        }

        private static async ETTask ProcessNext(this BattleVisualQueueComponent self)
        {
            if (self.Actions.Count <= 0)
                return;

            self.IsPlaying = true;
            try
            {
                IVisualAction action = self.Actions.Dequeue();
                self.CurrentAction = action;

                // 执行动作并等待其完成
                await self.ExecuteAction(action);
            }
            catch (Exception e)
            {
                Log.Error($"[BattleVisualQueue] Error processing action: {e}");
            }
            finally
            {
                self.IsPlaying = false;
                self.CurrentAction = null;
            }
        }

        private static async ETTask ExecuteAction(this BattleVisualQueueComponent self, IVisualAction action)
        {
            switch (action)
            {
                case SpellAction spellAction:
                    await self.ExecuteSpell(spellAction.Data);
                    break;
                case TurnAction turnAction:
                    await self.ExecuteTurn(turnAction.IsPlayerTurn);
                    break;
                case CallbackAction callbackAction:
                    callbackAction.Callback?.Invoke();
                    await ETTask.CompletedTask;
                    break;
            }
        }

        private static async ETTask ExecuteSpell(this BattleVisualQueueComponent self, EntityCastSpell args)
        {
            BattleSceneComponent battleScene = self.GetParent<BattleSceneComponent>();
            Scene scene = battleScene.IScene as Scene;

            await SpellEffectHelper.PlaySpellEffect(scene, args);
        }

        private static async ETTask ExecuteTurn(this BattleVisualQueueComponent self, bool isPlayerTurn)
        {
            Log.Info($"[BattleVisualQueue] {(isPlayerTurn ? "Player" : "Enemy")} Turn Begin Visual");
            // 模拟回合切换动画
            await self.Root().GetComponent<TimerComponent>().WaitAsync(500);
        }
    }
}
