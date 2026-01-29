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
        /// <summary>
        /// 检查当前位置后面是否还有同一施法者的普攻
        /// </summary>
        private static bool HasFollowingNormalAttack(List<EntityCastSpell> spells, int currentIndex)
        {
            if (currentIndex >= spells.Count - 1)
                return false;

            var currentSpell = spells[currentIndex];
            int casterId = currentSpell.CasterId;
            
            // 检查当前技能是否为普攻
            if (!IsNormalAttackSpell(currentSpell.SpellId))
                return false;

            // 检查后面是否有同一施法者的普攻
            for (int i = currentIndex + 1; i < spells.Count; i++)
            {
                var spell = spells[i];
                if (spell.CasterId == casterId && IsNormalAttackSpell(spell.SpellId))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断是否是近战普攻类型的技能
        /// </summary>
        private static bool IsNormalAttackSpell(int spellId)
        {
            if (spellId <= 0)
                return false;

            DREntitySpellEntry spellEntry = DREntitySpellEntryCategory.Instance.Get(spellId);
            if (spellEntry == null)
                return true; // 默认为近战

            // Melee或Normal类型都视为普攻
            return spellEntry.SpellType == (int)EEntitySpellType.Melee ||
                   spellEntry.SpellType == (int)EEntitySpellType.Normal;
        }

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
                // 需要处理连续普攻的返回原位逻辑
                for (int i = 0; i < args.Spells.Count; i++)
                {
                    var spell = args.Spells[i];
                    // 检查后面是否还有同一施法者的普攻
                    bool shouldMoveBack = !HasFollowingNormalAttack(args.Spells, i);
                    await SpellEffectHelper.PlaySpellEffect(scene, spell, shouldMoveBack);
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
