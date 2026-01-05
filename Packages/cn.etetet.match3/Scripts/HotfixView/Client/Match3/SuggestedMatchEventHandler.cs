using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 匹配提示事件处理器 - 播放/清除匹配提示动画
    /// </summary>
    [Event(SceneType.Current)]
    [FriendOf(typeof(Match3BoardComponent))]
    public class SuggestedMatchEventHandler : AEvent<Scene, SuggestedMatchEvent>
    {
        protected override async ETTask Run(Scene scene, SuggestedMatchEvent args)
        {
            var boardComponent = scene.GetComponent<Match3BoardComponent>();
            if (boardComponent == null) return;
            
            if (args.IsShow && args.TilesToHighlight != null)
            {
                // 播放匹配提示动画
                foreach (var tileDef in args.TilesToHighlight)
                {
                    var tile = boardComponent.GetTile(tileDef.x, tileDef.y);
                    if (tile == null) continue;
                    
                    // 获取瓦片的TileView组件
                    var tileView = tile.GetComponent<TileView>();
                    if (tileView == null || tileView.GameObject == null) continue;
                    
                    var animator = tileView.GameObject.GetComponent<Animator>();
                    if (animator != null && tileView.GameObject.activeSelf)
                    {
                        animator.SetTrigger("SuggestedMatch");
                    }
                }
            }
            else
            {
                // 清除匹配提示动画（重置所有之前提示的瓦片）
                foreach (var tileDef in boardComponent.SuggestedMatchTiles)
                {
                    var tile = boardComponent.GetTile(tileDef.x, tileDef.y);
                    if (tile == null) continue;
                    
                    var tileView = tile.GetComponent<TileView>();
                    if (tileView == null || tileView.GameObject == null) continue;
                    
                    var animator = tileView.GameObject.GetComponent<Animator>();
                    if (animator != null && tileView.GameObject.activeSelf)
                    {
                        animator.SetTrigger("Reset");
                    }
                }
            }
            
            await ETTask.CompletedTask;
        }
    }
}
