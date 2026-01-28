using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 英雄出手次数事件处理器
    /// 在角色身上显示出手次数飘字（累加模式）
    /// </summary>
    [Event(SceneType.Battle)]
    [FriendOf(typeof(BattleCharacterViewComponent))]
    [FriendOf(typeof(DamageNumberComponent))]
    public class HeroActionCountEventHandler : AEvent<Scene, HeroActionCountEvent>
    {
        protected override async ETTask Run(Scene scene, HeroActionCountEvent args)
        {
            if (args.ActionInfos == null || args.ActionInfos.Count == 0)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 获取飘字组件
            DamageNumberComponent dnComponent = scene.GetComponent<DamageNumberComponent>();
            if (dnComponent == null || !dnComponent.IsInitialized)
            {
                Log.Warning("[HeroActionCountEventHandler] 飘字组件未初始化");
                await ETTask.CompletedTask;
                return;
            }

            // 遍历所有英雄的出手次数信息
            foreach (var actionInfo in args.ActionInfos)
            {
                // 根据HeroId查找英雄
                EntityHero hero = SpellEffectHelper.FindHeroByHeroId(scene, actionInfo.HeroId);
                if (hero == null)
                {
                    Log.Warning($"[HeroActionCountEventHandler] 未找到英雄 HeroId={actionInfo.HeroId}");
                    continue;
                }

                // 获取视图组件
                BattleCharacterViewComponent viewComponent = SpellEffectHelper.GetViewComponent(hero);
                if (viewComponent == null || viewComponent.CharacterGO == null)
                {
                    Log.Warning($"[HeroActionCountEventHandler] 英雄没有视图组件 HeroId={actionInfo.HeroId}");
                    continue;
                }

                // 获取角色世界坐标（身体中间）
                Vector3 worldPos = viewComponent.CharacterGO.transform.position;

                // 使用累加模式显示出手次数飘字
                dnComponent.AddActionCount(actionInfo.HeroId, worldPos, actionInfo.ActionCount);
            }

            await ETTask.CompletedTask;
        }
    }
}
