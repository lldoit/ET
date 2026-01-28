namespace ET.Client
{
    /// <summary>
    /// 技能糖果出手次数处理器 - 监听技能糖果消除，立即发布飘字事件
    /// 与 match3 包中的 PlaySkillCandyEffectEventHandler 同时监听同一事件
    /// </summary>
    [Event(SceneType.Battle)]
    [FriendOf(typeof(EntityHero))]
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(BattleSceneComponent))]
    public class SkillCandyActionCountHandler : AEvent<Scene, PlaySkillCandyEffectEvent>
    {
        protected override async ETTask Run(Scene scene, PlaySkillCandyEffectEvent args)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            EntityGroup playerGroup = battleScene.RedGroup;
            if (playerGroup == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 根据颜色找到对应的英雄
            EntityHero hero = FindHeroByColor(playerGroup, (int)args.Color);
            if (hero == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 立即发布飘字事件（每次消除显示 x1）
            var actionInfos = new System.Collections.Generic.List<HeroActionInfo>
            {
                new HeroActionInfo
                {
                    HeroId = hero.HeroId,
                    ActionCount = 1
                }
            };
            
            EventSystem.Instance.Publish(scene, new HeroActionCountEvent
            {
                ActionInfos = actionInfos
            });

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 根据颜色查找英雄
        /// </summary>
        private static EntityHero FindHeroByColor(EntityGroup group, int color)
        {
            if (group?.Entitys == null)
                return null;

            foreach (var heroRef in group.Entitys)
            {
                EntityHero hero = heroRef;
                if (hero != null && hero.HeroColor == color)
                    return hero;
            }

            return null;
        }
    }
}
