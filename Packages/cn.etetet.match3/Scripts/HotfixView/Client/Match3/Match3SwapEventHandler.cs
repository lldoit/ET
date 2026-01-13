using PrimeTween;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Match3瓦片交换动画事件处理器
    /// 使用PrimeTween播放两个瓦片的交换动画
    /// 支持世界空间和UI空间双渲染模式
    /// </summary>
    [Event(SceneType.Battle)]
    [FriendOf(typeof(Match3BoardComponent))]
    public class Match3SwapEventHandler : AEvent<Scene, Match3SwapEvent>

    {
        protected override async ETTask Run(Scene scene, Match3SwapEvent args)
        {
            Tile tile1 = args.Tile1Ref;
            Tile tile2 = args.Tile2Ref;

            if (tile1 == null || tile1.IsDisposed || tile2 == null || tile2.IsDisposed)
            {
                return;
            }

            // 获取Match3棋盘组件
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            bool useUIRenderer = match3Board?.UseUIRenderer ?? false;

            if (useUIRenderer)
            {
                // UI渲染模式：使用TileView和RectTransform
                var uiTileView1 = tile1.GetComponent<TileView>();
                var uiTileView2 = tile2.GetComponent<TileView>();

                if (uiTileView1 == null || uiTileView1.RectTransform == null ||
                    uiTileView2 == null || uiTileView2.RectTransform == null)
                {
                    return;
                }

                Vector2 pos1 = uiTileView1.RectTransform.anchoredPosition;
                Vector2 pos2 = uiTileView2.RectTransform.anchoredPosition;

                // 调整层级确保显示正确
                uiTileView1.RectTransform.SetAsLastSibling();

                // 同时播放两个瓦片的移动动画
                _ = Tween.UIAnchoredPosition(uiTileView1.RectTransform, pos2, args.Duration, Ease.Linear);
                _ = Tween.UIAnchoredPosition(uiTileView2.RectTransform, pos1, args.Duration, Ease.Linear);
            }
            else
            {
                // 世界空间渲染模式：使用TileView和Transform
                var tileView1 = tile1.GetComponent<TileView>();
                var tileView2 = tile2.GetComponent<TileView>();

                if (tileView1 == null || tileView1.GameObject == null ||
                    tileView2 == null || tileView2.GameObject == null)
                {
                    return;
                }

                Vector3 pos1 = tileView1.GameObject.transform.position;
                Vector3 pos2 = tileView2.GameObject.transform.position;

                // 设置sortingOrder以确保显示正确
                var spriteRenderer1 = tileView1.GameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer1 != null)
                {
                    spriteRenderer1.sortingOrder = 1;
                }

                // 同时播放两个瓦片的移动动画
                _ = Tween.Position(tileView1.GameObject.transform, pos2, args.Duration, Ease.Linear);
                _ = Tween.Position(tileView2.GameObject.transform, pos1, args.Duration, Ease.Linear);

                // 等待动画完成后恢复sortingOrder
                await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(args.Duration * 1000));

                if (spriteRenderer1 != null)
                {
                    spriteRenderer1.sortingOrder = 0;
                }
                return;
            }

            // 等待动画完成
            await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(args.Duration * 1000));
        }
    }
}
