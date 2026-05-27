namespace ET.Server
{
    [Event(SceneType.Map)]
    public class UnitDieEvent_TriggerPetOwnerQuest: AEvent<Scene, UnitDie>
    {
        protected override async ETTask Run(Scene scene, UnitDie a)
        {
            Unit attacker = a.Unit;
            if (attacker == null || attacker.GetComponent<QuestComponent>() != null)
            {
                await ETTask.CompletedTask;
                return;
            }

            PetComponent petComponent = attacker.GetComponent<PetComponent>();
            Unit owner = petComponent?.GetOwner();
            if (owner == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            QuestEventHelper.OnMonsterKilled(owner, a.Target.Entity.Id, 1);
            await ETTask.CompletedTask;
        }
    }
}
