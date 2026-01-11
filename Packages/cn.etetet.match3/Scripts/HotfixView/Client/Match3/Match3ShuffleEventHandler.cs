using PrimeTween;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Match3洗牌事件处理器
    /// 使用PrimeTween播放瓦片洗牌动画
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

                var tileView = tile.GetComponent<TileView>();
                if (tileView == null || tileView.GameObject == null)
                {
                    continue;
                }

                var transform = tileView.GameObject.transform;
                
                // 获取目标世界坐标
                Vector3 targetPosition = match3Board.GetTileWorldPosition(moveInfo.ToX, moveInfo.ToY);
                
                // 设置位置
                transform.position = targetPosition;
                
                // 创建缩放动画序列：先缩小到0.5，再恢复到1
                var sequence = Sequence.Create();
                _ = sequence.Chain(Tween.Scale(transform, 0.5f, args.Duration * 0.5f, Ease.InQuad));
                _ = sequence.Chain(Tween.Scale(transform, 1f, args.Duration * 0.5f, Ease.OutQuad));
            }

            // 等待动画完成
            await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(args.Duration * 1000));
        }
    }
}
