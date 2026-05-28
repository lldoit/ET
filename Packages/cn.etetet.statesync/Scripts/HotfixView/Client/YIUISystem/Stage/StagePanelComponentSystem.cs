using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.12.23
    /// Desc
    /// </summary>
    [FriendOf(typeof(StagePanelComponent))]
    public static partial class StagePanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this StagePanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this StagePanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this StagePanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始

        [YIUIInvoke(StagePanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this StagePanelComponent self)
        {
            self.OnEventBackInvokeAsync().Coroutine();
        }

        private static async ETTask OnEventBackInvokeAsync(this StagePanelComponent self)
        {
            await self.YIUIMgr().HomePanel<HomePanelComponent>();
        }

        [YIUIInvoke(StagePanelComponent.OnEventEnterMapInvoke)]
        private static async ETTask OnEventEnterMapInvoke(this StagePanelComponent self)
        {
            Scene root = self.Root();
            CrawlerBattleStageConfig stageConfig = GetFirstCrawlerBattleStageConfig(self);
            if (stageConfig == null)
            {
                Log.Error("Crawlers 关卡配置为空，无法进入战斗");
                return;
            }

            await EventSystem.Instance.PublishAsync(root, new EnterStageBattle
            {
                StageId = stageConfig.Id,
                BattleType = StageBattleType.Crawlers
            });
        }

        private static CrawlerBattleStageConfig GetFirstCrawlerBattleStageConfig(this StagePanelComponent self)
        {
            CrawlerBattleStageConfig firstConfig = null;
            foreach (CrawlerBattleStageConfig config in self.Fiber().GetSingleton<CrawlerBattleStageConfigCategory>().GetAll().Values)
            {
                if (firstConfig == null || config.Id < firstConfig.Id)
                {
                    firstConfig = config;
                }
            }

            return firstConfig;
        }
        #endregion YIUIEvent结束
    }
}
