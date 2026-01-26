using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 批量技能释放事件处理器 - 将整批技能加入序列器
    /// 批次内不同角色的技能并行播放，同一角色的技能串行播放
    /// </summary>
    [Event(SceneType.Battle)]
    public class EntityCastSpellBatchEventHandler : AEvent<Scene, EntityCastSpellBatch>
    {
        protected override async ETTask Run(Scene scene, EntityCastSpellBatch args)
        {
            if (args.Spells == null || args.Spells.Count == 0)
            {
                await ETTask.CompletedTask;
                return;
            }

            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            BattleSequencerComponent sequencer = battleScene.GetComponent<BattleSequencerComponent>();
            if (sequencer == null)
            {
                // 如果没有序列器组件，逐个直接播放（兼容旧逻辑）
                foreach (var spell in args.Spells)
                {
                    await SpellEffectHelper.PlaySpellEffect(scene, spell);
                }
                return;
            }

            // 将批次内所有技能作为一个批次入队
            var actions = new List<ISequenceAction>();
            foreach (var spell in args.Spells)
            {
                actions.Add(new SpellSequenceAction { Data = spell });
            }
            sequencer.EnqueueBatch(actions);

            await ETTask.CompletedTask;
        }
    }
}
