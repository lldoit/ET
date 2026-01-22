namespace ET.Client
{
    /// <summary>
    /// 战斗场景退出开始事件处理器
    /// 此事件在退出战斗场景前发布，用于关闭战斗界面
    /// </summary>
    [Event(SceneType.Battle)]
    [FriendOf(typeof(BattleSceneComponent))]
    [FriendOf(typeof(EntityGroup))]
    public class BattleSceneExitStartHandler : AEvent<Scene, BattleSceneExitStart>
    {
        protected override async ETTask Run(Scene scene, BattleSceneExitStart args)
        {
            Log.Info("战斗场景退出开始，清理角色视图并关闭战斗面板");

            // 先清理所有角色的视图组件（在UI关闭前同步清理，避免视觉残留）
            CleanupHeroViews(scene);

            // 关闭战斗面板
            await scene.YIUIMgr().ClosePanelAsync<BattlePanelComponent>();
        }

        /// <summary>
        /// 清理所有英雄的视图组件
        /// </summary>
        private void CleanupHeroViews(Scene scene)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
            {
                return;
            }

            // 清理红方角色视图
            EntityGroup redGroup = battleScene.RedGroup;
            if (redGroup != null)
            {
                CleanupGroupHeroViews(redGroup);
            }

            // 清理蓝方角色视图
            EntityGroup blueGroup = battleScene.BlueGroup;
            if (blueGroup != null)
            {
                CleanupGroupHeroViews(blueGroup);
            }
        }

        /// <summary>
        /// 清理指定队伍中所有英雄的视图组件
        /// </summary>
        private void CleanupGroupHeroViews(EntityGroup group)
        {
            if (group.Entitys == null)
            {
                return;
            }

            foreach (var heroRef in group.Entitys)
            {
                EntityHero hero = heroRef;
                if (hero == null || hero.IsDisposed)
                {
                    continue;
                }

                // 移除视图组件，触发其Destroy方法清理GameObject
                BattleCharacterViewComponent viewComponent = hero.GetComponent<BattleCharacterViewComponent>();
                if (viewComponent != null)
                {
                    viewComponent.Dispose();
                }
            }

            Log.Info($"[BattleSceneExitStartHandler] 清理{group.Camp}方角色视图完成");
        }
    }
}
