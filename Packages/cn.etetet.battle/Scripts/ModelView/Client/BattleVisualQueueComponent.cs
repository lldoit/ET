
using System.Collections.Generic;
using ET;

namespace ET.Client
{
    public interface IVisualAction
    {
    }

    [EnableClass]
    public class SpellAction : IVisualAction
    {
        public EntityCastSpell Data;
    }

    [EnableClass]
    public class TurnAction : IVisualAction
    {
        public bool IsPlayerTurn;
    }

    [EnableClass]
    public class CallbackAction : IVisualAction
    {
        public System.Action Callback;
    }

    [ComponentOf(typeof(BattleSceneComponent))]
    public class BattleVisualQueueComponent : Entity, IAwake, IDestroy, IUpdate
    {
        public Queue<IVisualAction> Actions = new Queue<IVisualAction>();
        public bool IsPlaying;
        public IVisualAction CurrentAction;
    }
}
