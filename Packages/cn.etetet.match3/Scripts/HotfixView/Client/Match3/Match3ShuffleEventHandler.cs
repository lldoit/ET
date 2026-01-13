using PrimeTween;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Match3洗牌事件处理器
    /// 使用PrimeTween播放瓦片洗牌动画（UI模式）
    /// </summary>
    [Event(SceneType.Battle)]
    public class Match3ShuffleEventHandler : AEvent<Scene, Match3ShuffleEvent>
    {
        protected override async ETTask Run(Scene scene, Match3ShuffleEvent args)
        {
            if (args.Moves == null || args.Moves.Count == 0)
            {
                return;
            }

            // 获取Match3棋盘组件
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            if (match3Board == null)
            {
                Log.Warning("Match3BoardComponent not found in scene");
                return;
            }

            // 洗牌动画：先缩小，再放大
            foreach (var moveInfo in args.Moves)
            {
                Tile tile = moveInfo.TileRef;
                if (tile == null || tile.IsDisposed)
                {
                    continue;
                }

                // UI渲染模式
                var uiTileView = tile.GetComponent<TileView>();
                if (uiTileView == null || uiTileView.RectTransform == null)
                {
                    continue;
                }

                Vector2 targetPosition = match3Board.GetUITilePosition(moveInfo.ToX, moveInfo.ToY);
                uiTileView.RectTransform.anchoredPosition = targetPosition;

                // 创建缩放动画序列
                var sequence = Sequence.Create();
                _ = sequence.Chain(Tween.Scale(uiTileView.RectTransform, 0.5f, args.Duration * 0.5f, Ease.InQuad));
                _ = sequence.Chain(Tween.Scale(uiTileView.RectTransform, 1f, args.Duration * 0.5f, Ease.OutQuad));
            }

            // 等待动画完成
            await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(args.Duration * 1000));
        }
    }
}
