using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Match3填充事件处理器
    /// 接收填充事件，使用PrimeTween播放瓦片填充动画
    /// UI空间渲染模式
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

                    // UI渲染模式：使用TileView和RectTransform
                    var uiTileView = tile.GetComponent<TileView>();
                    if (uiTileView == null || uiTileView.RectTransform == null)
                    {
                        continue;
                    }

                    Vector2 targetPosition = match3Board.GetUITilePosition(moveInfo.ToX, moveInfo.ToY);
                    var tween = Tween.UIAnchoredPosition(
                        uiTileView.RectTransform,
                        targetPosition,
                        args.Duration,
                        Ease.InQuad
                    );
                    tweens.Add(tween);
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

                    // UI渲染模式
                    var uiTileView = tile.GetComponent<TileView>();
                    if (uiTileView == null)
                    {
                        // 创建UI视图
                        Vector2 tempPos = match3Board.GetUITilePosition(createInfo.TargetX, createInfo.TargetY);
                        match3Board.CreateTileView(tile, tempPos);
                        uiTileView = tile.GetComponent<TileView>();
                    }

                    if (uiTileView == null || uiTileView.RectTransform == null)
                    {
                        continue;
                    }

                    Vector2 targetPosition = match3Board.GetUITilePosition(createInfo.TargetX, createInfo.TargetY);
                    Vector2 initialPosition = match3Board.GetUITilePosition(createInfo.InitialX, createInfo.InitialY);

                    uiTileView.RectTransform.anchoredPosition = initialPosition;

                    var tween = Tween.UIAnchoredPosition(
                        uiTileView.RectTransform,
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
                await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(maxDuration * 1000));
            }
        }
    }
}
