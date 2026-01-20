using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Battle)]
    public class AfterEntityHeroCreate_CreateHeroView : AEvent<Scene, AfterEntityHeroCreate>
    {
        protected override async ETTask Run(Scene scene, AfterEntityHeroCreate args)
        {
            EntityHero hero = args.Hero;
            // hero View层
            string assetsName = "Hero_gong";
            //hero.Entry.ModelEntry
            GameObject prefab = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            
            //go.transform.position = unit.Position;
            
            var viewCom = hero.AddComponent<BattleCharacterViewComponent>();
            viewCom.Initialize(go);
            
            await ETTask.CompletedTask;
        }
    }
}