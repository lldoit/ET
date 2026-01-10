using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Match3填充事件处理器
    /// 接收填充事件，使用PrimeTween播放瓦片填充动画
    /// </summary>
    [Event(SceneType.Battle)]
    public class Match3FillEventHandler : AEvent<Scene, Match3FillEvent>
    {
        protected override async ETTask Run(Scene scene, Match3FillEvent args)
        {
            if (args.Moves == null && args.NewTiles == null)
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

            // 收集所有Tween以便等待它们完成
            var tweens = new List<Tween>();

            // 处理现有瓦片的移动
            float maxDuration = args.Duration;
            if (args.Moves != null)
            {
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

                    // 检查是否有路径信息（滑动填充使用路径动画）
                    if (moveInfo.Path != null && moveInfo.Path.Count > 0)
                    {
                        // 参考CandyMatch3Kit：路径动画时长 = 0.5秒 * 路径长度
                        float pathDuration = 0.5f * moveInfo.Path.Count;
                        if (pathDuration > maxDuration)
                        {
                            maxDuration = pathDuration;
                        }
                        
                        // 构建路径点数组
                        var pathPositions = new Vector3[moveInfo.Path.Count];
                        for (int k = 0; k < moveInfo.Path.Count; k++)
                        {
                            var pathPos = moveInfo.Path[k];
                            pathPositions[k] = match3Board.GetTileWorldPosition(pathPos.x, pathPos.y);
                        }
                        
                        // 参考CandyMatch3Kit：LeanTween路径动画需要至少4个点
                        // PrimeTween不支持直接路径动画，所以使用分段动画
                        if (pathPositions.Length >= 2)
                        {
                            // 使用序列动画沿路径移动
                            float segmentDuration = pathDuration / pathPositions.Length;
                            var transform = tileView.GameObject.transform;
                            
                            // 创建序列动画 - PrimeTween Sequence会自动执行
                            var sequence = Sequence.Create();
                            for (int k = 0; k < pathPositions.Length; k++)
                            {
                                var targetPos = pathPositions[k];
                                _ = sequence.Chain(Tween.Position(transform, targetPos, segmentDuration, Ease.OutQuad));
                            }
                            // Sequence创建后会自动开始播放，不需要添加到tweens列表
                        }
                        else
                        {
                            // 只有一个目标点，直接移动
                            Vector3 targetPosition = match3Board.GetTileWorldPosition(moveInfo.ToX, moveInfo.ToY);
                            var tween = Tween.Position(
                                tileView.GameObject.transform,
                                targetPosition,
                                args.Duration,
                                Ease.OutQuad // 滑动填充使用OutQuad
                            );
                            tweens.Add(tween);
                        }
                    }
                    else
                    {
                        // 没有路径信息，直接移动到目标位置（重力填充）
                        Vector3 targetPosition = match3Board.GetTileWorldPosition(moveInfo.ToX, moveInfo.ToY);
                        
                        // 使用PrimeTween播放移动动画
                        var tween = Tween.Position(
                            tileView.GameObject.transform,
                            targetPosition,
                            args.Duration,
                            Ease.InQuad
                        );
                        
                        tweens.Add(tween);
                    }
                }
            }

            // 处理新创建瓦片的下落
            if (args.NewTiles != null)
            {
                foreach (var createInfo in args.NewTiles)
                {
                    Tile tile = createInfo.TileRef;
                    if (tile == null || tile.IsDisposed)
                    {
                        continue;
                    }

                    var tileView = tile.GetComponent<TileView>();
                    
                    // 如果TileView不存在，尝试创建它
                    if (tileView == null)
                    {
                        // 计算初始位置（逻辑位置），稍后会覆盖为动画起始位置
                        // 这里我们只需要一个位置来初始化GameObject，具体位置会被下落动画覆盖
                        Vector3 tempPos = match3Board.GetTileWorldPosition(createInfo.TargetX, createInfo.TargetY);
                        match3Board.CreateTileView(tile, tempPos);
                        tileView = tile.GetComponent<TileView>();
                    }

                    if (tileView == null || tileView.GameObject == null)
                    {
                        continue;
                    }

                    // 获取目标世界坐标
                    Vector3 targetPosition = match3Board.GetTileWorldPosition(createInfo.TargetX, createInfo.TargetY);

                    // 计算初始位置（在屏幕上方）
                    // 直接使用 InitialX 和 InitialY 获取世界坐标
                    // InitialY 是负数，Match3BoardViewHelper.GetTileWorldPosition 会正确处理负数坐标，返回上方的位置
                    Vector3 initialPosition = match3Board.GetTileWorldPosition(createInfo.InitialX, createInfo.InitialY);
                    
                    // 设置初始位置
                    tileView.GameObject.transform.position = initialPosition;
                    
                    // 使用PrimeTween播放下落动画
                    var tween = Tween.Position(
                        tileView.GameObject.transform,
                        targetPosition,
                        args.Duration,
                        Ease.InQuad
                    );
                    
                    tweens.Add(tween);
                }
            }

            // 等待所有动画完成
            if (tweens.Count > 0)
            {
                // 等待最长的动画时间（路径动画可能比普通动画更长）
                await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(maxDuration * 1000));
            }
        }
    }
}
