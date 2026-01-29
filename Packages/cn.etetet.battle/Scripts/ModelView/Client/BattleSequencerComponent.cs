using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 序列动作接口 - 所有可序列化执行的动作基础接口
    /// </summary>
    public interface ISequenceAction
    {
        /// <summary>
        /// 施法者ID，用于分组（相同施法者的动作串行执行）
        /// 返回0表示不参与分组，作为全局动作单独执行
        /// </summary>
        int CasterId { get; }
    }

    /// <summary>
    /// 技能动作 - 播放技能效果
    /// </summary>
    public struct SpellSequenceAction : ISequenceAction
    {
        public EntityCastSpell Data;
        public int CasterId => Data.CasterId;
        
        /// <summary>
        /// 近战攻击后是否返回原位
        /// 用于连续普攻时，只在最后一次攻击后返回原位
        /// </summary>
        public bool ShouldMoveBack;
    }

    /// <summary>
    /// 回合切换动作 - 显示回合提示
    /// </summary>
    public struct TurnSequenceAction : ISequenceAction
    {
        public bool IsPlayerTurn;
        public int CasterId => 0; // 回合动作不分组，作为全局动作
    }

    /// <summary>
    /// 回调动作 - 执行注册的回调函数
    /// </summary>
    public struct CallbackSequenceAction : ISequenceAction
    {
        public int CallbackId;
        public int CasterId => 0; // 回调动作不分组，作为全局动作
    }

    /// <summary>
    /// 动作批次 - 一批需要并行执行的动作
    /// </summary>
    public struct ActionBatch
    {
        public List<ISequenceAction> Actions;
    }

    /// <summary>
    /// 战斗序列器组件 - 管理战斗动作的序列化播放
    /// 支持多角色并行释放技能，每个角色的技能按顺序播放
    /// </summary>
    [ComponentOf(typeof(BattleSceneComponent))]
    public class BattleSequencerComponent : Entity, IAwake, IDestroy, IUpdate
    {
        /// <summary>
        /// 批次队列 - 批次间串行执行
        /// </summary>
        public Queue<ActionBatch> BatchQueue = new Queue<ActionBatch>();

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying;

        /// <summary>
        /// 回调注册表 - CallbackId -> Action
        /// </summary>
        public Dictionary<int, Action> CallbackRegistry = new Dictionary<int, Action>();

        /// <summary>
        /// 下一个可用的回调ID
        /// </summary>
        public int NextCallbackId;

        /// <summary>
        /// 是否正在批量收集模式
        /// </summary>
        public bool IsCollectingBatch;

        /// <summary>
        /// 临时动作列表 - 用于批量收集模式
        /// </summary>
        public List<ISequenceAction> PendingActions = new List<ISequenceAction>();
    }
}
