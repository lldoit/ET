using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Battle)]
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(FormationComponent))]
    public class AfterEntityHeroCreate_CreateHeroView : AEvent<Scene, AfterEntityHeroCreate>
    {
        protected override async ETTask Run(Scene scene, AfterEntityHeroCreate args)
        {
            EntityHero hero = args.Hero;

            // 加载角色预制体
            string assetsName = "Hero_gong";
            //hero.Entry.ModelEntry
            GameObject prefab = await scene.Root().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, false);

            // 获取站位组件
            var formationComponent = scene.GetComponent<FormationComponent>();

            // 获取英雄所属阵营
            EntityGroup group = hero.GroupRef;
            ECamp camp = group?.Camp ?? ECamp.Red;

            // 计算站位索引（根据英雄在队伍中的位置）
            int slotIndex = 0;
            if (group != null && group.Entitys != null)
            {
                for (int i = 0; i < group.Entitys.Count; i++)
                {
                    EntityHero e = group.Entitys[i];
                    if (e != null && e.Id == hero.Id)
                    {
                        slotIndex = i;
                        break;
                    }
                }
            }

            // 设置角色位置和朝向
            if (formationComponent != null)
            {
                // 获取队伍总人数，用于敌方选择正确的站位配置
                int totalCount = group?.Entitys?.Count ?? 4;

                // 获取站位的世界坐标
                Vector3 worldPosition = formationComponent.GetSlotWorldPosition(camp, slotIndex, totalCount);
                bool facingLeft = formationComponent.GetSlotFacing(camp, slotIndex, totalCount);

                // 设置世界坐标位置
                go.transform.position = worldPosition;

                // 初始化视图组件并设置朝向
                var viewCom = hero.AddComponent<BattleCharacterViewComponent>();
                viewCom.Initialize(go, slotIndex);
                viewCom.SetFacing(facingLeft);

                Log.Info($"[Formation] 角色 {hero.HeroId} 放置到站位 {slotIndex}/{totalCount}, 世界坐标: {worldPosition}, 面向左: {facingLeft}");
            }
            else
            {
                // 如果没有站位组件，使用默认初始化
                var viewCom = hero.AddComponent<BattleCharacterViewComponent>();
                viewCom.Initialize(go, slotIndex);
                Log.Warning("[Formation] 站位组件未找到，使用默认位置");
            }

            await ETTask.CompletedTask;
        }
    }
}