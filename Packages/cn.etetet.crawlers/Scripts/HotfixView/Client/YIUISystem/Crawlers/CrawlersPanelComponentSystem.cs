using UnityEngine;
using UnityEngine.UI;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.5.17
    /// Desc
    /// </summary>
    [FriendOf(typeof(CrawlersPanelComponent))]
    [FriendOf(typeof(CrawlerBattleComponent))]
    [FriendOf(typeof(CrawlerDeckComponent))]
    [FriendOf(typeof(CrawlerComboComponent))]
    [FriendOf(typeof(CrawlerEnemyFormationComponent))]
    [FriendOf(typeof(CrawlerChantComponent))]
    public static partial class CrawlersPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this CrawlersPanelComponent self)
        {
            self.BattleId = 1;
            self.UIWindow.WindowOption |= EWindowOption.BanOpenTween;
        }

        [EntitySystem]
        private static void Destroy(this CrawlersPanelComponent self)
        {
            self.ClearUiListeners();
            self.BattleRef = default;
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this CrawlersPanelComponent self)
        {
            self.BindBackButton();
            self.BindEndTurnButton();
            self.BindHandTuningControls();
            self.EnsureManaWidget();
            self.BindBattle();
            self.SetCrawlersViewVisible(true);
            await ETTask.CompletedTask;
            return true;
        }

        private static void BindBackButton(this CrawlersPanelComponent self)
        {
            Button button = self.FindComponent<Button>(BackButtonPath);
            if (button == null)
            {
                Log.Warning("[CrawlersPanel] 未找到返回按钮");
                return;
            }

            EntityRef<CrawlersPanelComponent> selfRef = self;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnBackClickedAsync(selfRef).Coroutine());
        }

        private static void BindEndTurnButton(this CrawlersPanelComponent self)
        {
            Button button = self.FindComponent<Button>(EndTurnButtonPath);
            if (button == null)
            {
                Log.Warning("[CrawlersPanel] 未找到结束回合按钮");
                return;
            }

            EntityRef<CrawlersPanelComponent> selfRef = self;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnEndTurnClicked(selfRef));
        }

        private static void BindBattle(this CrawlersPanelComponent self)
        {
            CrawlerBattleComponent battle = self.GetOrStartBattle();

            if (self.u_ComHandView != null)
            {
                self.UnbindCardInteractions();
                self.ConfigureHandPiles();
                EntityRef<CrawlersPanelComponent> selfRef = self;
                self.u_ComHandView.CardClicked += card => OnCardClicked(selfRef, card);
            }

            self.RefreshBattleView();
        }

        private static void ConfigureHandPiles(this CrawlersPanelComponent self)
        {
            CrawlerHandView handView = self.u_ComHandView;
            if (handView == null)
            {
                return;
            }

            RectTransform playedPile = self.GetOrCreateRectTransform(PlayedPilePath, "PlayedPile");
            RectTransform discardPile = self.FindRectTransform(DiscardPilePath);
            RectTransform drawPile = self.FindRectTransform(DrawPilePath);
            handView.ConfigureBattlePiles(playedPile, discardPile, drawPile);
            handView.ClearBattlePileVisuals();
        }

        private static void OnCardClicked(EntityRef<CrawlersPanelComponent> selfRef, CrawlerCardView cardView)
        {
            CrawlersPanelComponent self = selfRef;
            if (self == null)
            {
                return;
            }

            CrawlerPlayCardResult result = self.TryPlayCard(cardView);
            if (!result.Success)
            {
                return;
            }

            self.u_ComHandView?.PlayCardToPlayedPile(cardView, result.ComboBroken);
            self.RefreshStatusOnly();
        }

        private static CrawlerPlayCardResult TryPlayCard(this CrawlersPanelComponent self, CrawlerCardView cardView)
        {
            CrawlerBattleComponent battle = self.GetBattle();
            if (battle == null || cardView?.Definition == null)
            {
                return CrawlerPlayCardResult.Fail(CrawlerPlayFailReason.BattleMissing);
            }

            if (!long.TryParse(cardView.Definition.Id, out long cardInstanceId))
            {
                Log.Warning($"[CrawlersPanel] 无效卡牌实例ID: {cardView.Definition.Id}");
                return CrawlerPlayCardResult.Fail(CrawlerPlayFailReason.CardNotInHand);
            }

            CrawlerPlayCardResult result = battle.TryPlayCard(cardInstanceId);
            if (!result.Success)
            {
                Log.Warning($"[CrawlersPanel] 出牌失败: {result.FailReason}");
            }

            return result;
        }

        private static void RefreshBattleView(this CrawlersPanelComponent self)
        {
            CrawlerBattleComponent battle = self.GetBattle();
            if (battle == null)
            {
                return;
            }

            self.RefreshHandView(battle);
            self.RefreshStatusView(battle);
            Log.Info(battle.BuildStateLog("[CrawlersPanel] 刷新战斗"));
        }

        private static void RefreshStatusOnly(this CrawlersPanelComponent self)
        {
            CrawlerBattleComponent battle = self.GetBattle();
            if (battle == null)
            {
                return;
            }

            self.RefreshStatusView(battle);
            Log.Info(battle.BuildStateLog("[CrawlersPanel] 刷新战斗状态"));
        }

        private static CrawlerBattleComponent GetOrStartBattle(this CrawlersPanelComponent self)
        {
            Scene scene = self.GetBattleScene();
            CrawlerBattleComponent battle = scene.GetComponent<CrawlerBattleComponent>();
            if (battle == null)
            {
                battle = scene.AddComponent<CrawlerBattleComponent>();
            }

            if (!battle.Started)
            {
                battle.StartBattle(self.BattleId);
            }

            self.BattleRef = battle;
            return battle;
        }

        private static CrawlerBattleComponent GetBattle(this CrawlersPanelComponent self)
        {
            CrawlerBattleComponent battle = self.BattleRef;
            if (battle != null)
            {
                return battle;
            }

            battle = self.GetBattleScene().GetComponent<CrawlerBattleComponent>();
            if (battle != null)
            {
                self.BattleRef = battle;
            }

            return battle;
        }

        private static Scene GetBattleScene(this CrawlersPanelComponent self)
        {
            Scene currentScene = self.Root().CurrentScene();
            return currentScene != null && !currentScene.IsDisposed ? currentScene : self.Scene();
        }

        private static async ETTask OnBackClickedAsync(EntityRef<CrawlersPanelComponent> selfRef)
        {
            CrawlersPanelComponent self = selfRef;
            if (self == null)
            {
                return;
            }

            await CrawlersBattleSceneHelper.ExitBattleAsync(self.Root());
        }

        private static void OnEndTurnClicked(EntityRef<CrawlersPanelComponent> selfRef)
        {
            CrawlersPanelComponent self = selfRef;
            if (self == null)
            {
                return;
            }

            CrawlerBattleComponent battle = self.GetBattle();
            if (battle == null)
            {
                return;
            }

            CrawlerTurnResult result = battle.EndPlayerTurn();
            if (!result.Success)
            {
                Log.Warning($"[CrawlersPanel] 结束回合失败: {result.FailReason}");
                return;
            }

            self.u_ComHandView?.PlayEndTurnPileCycle();
            self.RefreshBattleViewFromDraw();
        }

        private static void ClearUiListeners(this CrawlersPanelComponent self)
        {
            self.FindComponent<Button>(BackButtonPath)?.onClick.RemoveAllListeners();
            self.FindComponent<Button>(EndTurnButtonPath)?.onClick.RemoveAllListeners();
            self.ClearHandTuningListeners();
            self.UnbindCardInteractions();
        }

        private static void UnbindCardInteractions(this CrawlersPanelComponent self)
        {
            if (self.u_ComHandView != null)
            {
                self.u_ComHandView.ClearCardInteractionListeners();
            }
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
