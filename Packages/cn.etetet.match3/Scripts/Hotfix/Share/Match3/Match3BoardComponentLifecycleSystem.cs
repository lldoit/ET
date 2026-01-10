using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件生命周期系统
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [EntitySystemOf(typeof(Match3BoardComponent))]
    public static partial class Match3BoardComponentLifecycleSystem
    {
        [EntitySystem]
        private static void Awake(this Match3BoardComponent self)
        {
            GameStateSystem.Reset(ref self.GameState);
            self.LastMoveTime = TimeInfo.Instance.ClientNow();
        }

        [EntitySystem]
        private static void Update(this Match3BoardComponent self)
        {
            if (!self.HasLevel) return;

            // 检查是否忙碌
            bool isBusy = self.InputLocked || self.CurrentlySwapping || self.CurrentlyAwarding;
            
            if (isBusy)
            {
                self.LastMoveTime = TimeInfo.Instance.ClientNow();
                // 如果正在显示提示，清除它
                if (self.SuggestedMatchTiles.Count > 0) 
                {
                    self.ClearSuggestedMatch();
                }
                return;
            }

            // 检查是否需要显示提示
            // Convert seconds to milliseconds
            long timeThreshold = (long)(Match3Constants.TimeBetweenRandomMatchSuggestions * 1000);
            
            if (TimeInfo.Instance.ClientNow() - self.LastMoveTime >= timeThreshold)
            {
                // 如果没有显示提示才显示
                if (self.SuggestedMatchTiles.Count == 0)
                {
                    self.ShowSuggestedMatchAsync().NoContext();
                }
            }
        }
    }
}
