using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 匹配提示事件处理器 - 播放/清除匹配提示动画
    /// 支持世界空间和UI空间双渲染模式
    /// </summary>
    [Event(SceneType.Battle)]
    [FriendOf(typeof(Match3BoardComponent))]
    public class SuggestedMatchEventHandler : AEvent<Scene, SuggestedMatchEvent>
    {
        protected override async ETTask Run(Scene scene, SuggestedMatchEvent args)
        {
            var boardComponent = scene.GetComponent<Match3BoardComponent>();
            if (boardComponent == null) return;

            bool useUIRenderer = boardComponent.UseUIRenderer;

            if (args.IsShow && args.TilesToHighlight != null)
            {
                // 播放匹配提示动画
                foreach (var tileDef in args.TilesToHighlight)
                {
                    var tile = boardComponent.GetTile(tileDef.x, tileDef.y);
                    if (tile == null) continue;

                    if (useUIRenderer)
                    {
                        // UI渲染模式
                        var uiTileView = tile.GetComponent<TileView>();
                        if (uiTileView == null || uiTileView.GameObject == null) continue;

                        var animator = uiTileView.GameObject.GetComponent<Animator>();
                        if (animator != null && uiTileView.GameObject.activeSelf)
                        {
                            animator.SetTrigger("SuggestedMatch");
                        }
                    }
                    else
                    {
                        // 世界空间渲染模式
                        var tileView = tile.GetComponent<TileView>();
                        if (tileView == null || tileView.GameObject == null) continue;

                        var animator = tileView.GameObject.GetComponent<Animator>();
                        if (animator != null && tileView.GameObject.activeSelf)
                        {
                            animator.SetTrigger("SuggestedMatch");
                        }
                    }
                }
            }
            else
            {
                // 清除匹配提示动画
                foreach (var tileDef in boardComponent.SuggestedMatchTiles)
                {
                    var tile = boardComponent.GetTile(tileDef.x, tileDef.y);
                    if (tile == null) continue;

                    if (useUIRenderer)
                    {
                        var uiTileView = tile.GetComponent<TileView>();
                        if (uiTileView == null || uiTileView.GameObject == null) continue;

                        var animator = uiTileView.GameObject.GetComponent<Animator>();
                        if (animator != null && uiTileView.GameObject.activeSelf)
                        {
                            animator.SetTrigger("Reset");
                        }
                    }
                    else
                    {
                        var tileView = tile.GetComponent<TileView>();
                        if (tileView == null || tileView.GameObject == null) continue;

                        var animator = tileView.GameObject.GetComponent<Animator>();
                        if (animator != null && tileView.GameObject.activeSelf)
                        {
                            animator.SetTrigger("Reset");
                        }
                    }
                }
            }

            await ETTask.CompletedTask;
        }
    }
}
