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
