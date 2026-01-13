using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 道具动画事件处理器
    /// 根据道具类型播放对应的动画效果（UI模式）
    /// </summary>
    [Event(SceneType.Battle)]
    public class BoosterAnimationEventHandler : AEvent<Scene, BoosterAnimationEvent>
    {
        protected override async ETTask Run(Scene scene, BoosterAnimationEvent args)
        {
            // 获取棋盘组件
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            if (match3Board == null)
            {
                return;
            }

            // 获取UI特效池
            var uiFxPool = match3Board.GetComponent<FxPoolComponent>();
            if (uiFxPool == null)
            {
                return;
            }

            // 获取瓦片的世界坐标
            Vector3 worldPos = Vector3.zero;
            var tile = match3Board.GetTile(args.TargetX, args.TargetY);
            if (tile != null)
            {
                var uiTileView = tile.GetComponent<TileView>();
                if (uiTileView != null && uiTileView.RectTransform != null)
                {
                    worldPos = uiTileView.RectTransform.position;
                }
            }

            switch (args.BoosterType)
            {
                case BoosterType.Bomb:
                    uiFxPool.PlaySkillCandyExplosion(worldPos);
                    break;

                case BoosterType.ColorBomb:
                    uiFxPool.PlayColorBombExplosion(worldPos);
                    break;

                default:
                    break;
            }

            await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(args.Duration * 1000));
        }
    }
}
