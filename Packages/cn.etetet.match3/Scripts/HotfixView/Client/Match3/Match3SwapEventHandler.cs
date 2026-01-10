using PrimeTween;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Match3瓦片交换动画事件处理器
    /// 使用PrimeTween播放两个瓦片的交换动画
    /// </summary>
    [Event(SceneType.Battle)]
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

            var tileView1 = tile1.GetComponent<TileView>();
            var tileView2 = tile2.GetComponent<TileView>();
            
            if (tileView1 == null || tileView1.GameObject == null ||
                tileView2 == null || tileView2.GameObject == null)
            {
                return;
            }

            // 获取两个瓦片的当前位置
            Vector3 pos1 = tileView1.GameObject.transform.position;
            Vector3 pos2 = tileView2.GameObject.transform.position;
            
            // 参考CandyMatch3Kit：交换时设置sortingOrder以确保显示正确
            var spriteRenderer1 = tileView1.GameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer1 != null)
            {
                spriteRenderer1.sortingOrder = 1;
            }

            // 同时播放两个瓦片的移动动画
            var tween1 = Tween.Position(
                tileView1.GameObject.transform,
                pos2,
                args.Duration,
                Ease.Linear // 交换使用线性动画
            );
            
            var tween2 = Tween.Position(
                tileView2.GameObject.transform,
                pos1,
                args.Duration,
                Ease.Linear
            );

            // 等待动画完成
            await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(args.Duration * 1000));
            
            // 恢复sortingOrder
            if (spriteRenderer1 != null)
            {
                spriteRenderer1.sortingOrder = 0;
            }
        }
    }
}
