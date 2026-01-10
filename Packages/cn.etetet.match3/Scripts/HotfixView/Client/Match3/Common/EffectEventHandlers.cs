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

            Vector3 worldPosition = match3Board.GetTileWorldPosition(args.X, args.Y);
            match3Board.PlayTileExplosionEffect(tile, worldPosition);
            
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

            var fxPool = match3Board.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            Vector3 worldPosition = match3Board.GetTileWorldPosition(args.X, args.Y);
            
            // 确保创建瓦片视图（修复生成特殊糖果时的空白问题）
            // 因为在MatchSystem中我们销毁了旧瓦片，但没有为新瓦片创建视图
            var tile = match3Board.GetTile(args.X, args.Y);
            if (tile != null)
            {
                // 注意：CreateTileView 需要本地坐标，因为它是相对于 BoardRoot 的
                Vector3 localPosition = match3Board.GetTileLocalPosition(args.X, args.Y);
                match3Board.CreateTileView(tile, localPosition);
            }

            fxPool.PlaySpawnParticles(worldPosition);
            
            await ETTask.CompletedTask;
        }
    }

    /// <summary>
    /// 条纹特效事件处理器
    /// </summary>
    [Event(SceneType.All)]
    public class PlayStripedEffectEventHandler : AEvent<Scene, PlayStripedEffectEvent>
    {
        protected override async ETTask Run(Scene scene, PlayStripedEffectEvent args)
        {
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            if (match3Board == null)
            {
                return;
            }

            var fxPool = match3Board.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            Vector3 worldPosition = match3Board.GetTileWorldPosition(args.X, args.Y);
            fxPool.PlayStripedCandyExplosion(args.Direction, worldPosition);
            
            await ETTask.CompletedTask;
        }
    }

    /// <summary>
    /// 包装特效事件处理器
    /// </summary>
    [Event(SceneType.All)]
    public class PlayWrappedEffectEventHandler : AEvent<Scene, PlayWrappedEffectEvent>
    {
        protected override async ETTask Run(Scene scene, PlayWrappedEffectEvent args)
        {
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            if (match3Board == null)
            {
                return;
            }

            var fxPool = match3Board.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            Vector3 worldPosition = match3Board.GetTileWorldPosition(args.X, args.Y);
            fxPool.PlayWrappedCandyExplosion(worldPosition);
            
            await ETTask.CompletedTask;
        }
    }
}
