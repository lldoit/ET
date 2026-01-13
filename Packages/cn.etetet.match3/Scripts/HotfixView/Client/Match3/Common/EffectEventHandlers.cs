using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 瓦片爆炸特效事件处理器
    /// </summary>
    [Event(SceneType.All)]
    public class PlayTileExplosionEventHandler : AEvent<Scene, PlayTileExplosionEvent>
    {
        protected override async ETTask Run(Scene scene, PlayTileExplosionEvent args)
        {
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            if (match3Board == null)
            {
                return;
            }

            // 获取瓦片（通过ID查找）
            var tile = match3Board.GetChild<Tile>(args.TileId);
            if (tile == null)
            {
                return;
            }

            // UI渲染模式
            match3Board.PlayUITileExplosionEffectAt(tile, args.X, args.Y);

            await ETTask.CompletedTask;
        }
    }

    /// <summary>
    /// 生成特效事件处理器
    /// </summary>
    [Event(SceneType.All)]
    public class PlaySpawnEffectEventHandler : AEvent<Scene, PlaySpawnEffectEvent>
    {
        protected override async ETTask Run(Scene scene, PlaySpawnEffectEvent args)
        {
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            if (match3Board == null)
            {
                return;
            }

            // 确保创建瓦片视图（修复生成特殊糖果时的空白问题）
            var tile = match3Board.GetTile(args.X, args.Y);
            if (tile != null)
            {
                Vector2 uiPosition = match3Board.GetUITilePosition(args.X, args.Y);
                match3Board.CreateTileView(tile, uiPosition);
            }

            await ETTask.CompletedTask;
        }
    }

    /// <summary>
    /// 技能糖果特效事件处理器
    /// </summary>
    [Event(SceneType.All)]
    [FriendOf(typeof(TilePoolComponent))]
    public class PlaySkillCandyEffectEventHandler : AEvent<Scene, PlaySkillCandyEffectEvent>

    {
        protected override async ETTask Run(Scene scene, PlaySkillCandyEffectEvent args)
        {
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            if (match3Board == null)
            {
                return;
            }

            var uiFxPool = match3Board.GetComponent<FxPoolComponent>();
            if (uiFxPool == null)
            {
                return;
            }

            // 获取瓦片的世界位置
            var tilePool = scene.GetComponent<TilePoolComponent>();
            if (tilePool != null && tilePool.TileContainer != null)
            {
                var tile = match3Board.GetTile(args.X, args.Y);
                Vector3 worldPos = Vector3.zero;
                if (tile != null)
                {
                    var uiTileView = tile.GetComponent<TileView>();
                    if (uiTileView != null && uiTileView.RectTransform != null)
                    {
                        worldPos = uiTileView.RectTransform.position;
                    }
                }
                uiFxPool.PlaySkillCandyExplosion(worldPos);
            }

            await ETTask.CompletedTask;
        }
    }
}
