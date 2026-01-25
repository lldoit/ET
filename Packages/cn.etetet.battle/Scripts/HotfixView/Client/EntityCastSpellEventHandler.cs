
using System.Collections.Generic;
using UnityEngine;
using ET;

namespace ET.Client
{
    /// <summary>
    /// 技能释放事件处理器 - 将事件加入视觉队列
    /// </summary>
    [Event(SceneType.Battle)]
    public class EntityCastSpellEventHandler : AEvent<Scene, EntityCastSpell>
    {
        protected override async ETTask Run(Scene scene, EntityCastSpell args)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null) return;

            BattleVisualQueueComponent queue = battleScene.GetComponent<BattleVisualQueueComponent>();
            if (queue == null)
            {
                // 如果没有队列组件，直接播放（兼容旧逻辑或异常情况）
                await SpellEffectHelper.PlaySpellEffect(scene, args);
                return;
            }

            // 加入队列
            BattleVisualQueueComponentSystem.Enqueue(queue, new SpellAction { Data = args });

            await ETTask.CompletedTask;
        }
    }
}
