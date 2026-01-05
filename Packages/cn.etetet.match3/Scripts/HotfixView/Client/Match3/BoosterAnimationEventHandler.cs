using PrimeTween;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 道具动画事件处理器
    /// 根据道具类型播放对应的动画效果
    /// </summary>
    [Event(SceneType.Current)]
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

            // 获取目标世界坐标
            Vector3 worldPosition = match3Board.GetTileWorldPosition(args.TargetX, args.TargetY);

            switch (args.BoosterType)
            {
                case BoosterType.Bomb:
                    await PlayBombAnimationAsync(scene, worldPosition, args.Duration);
                    break;
                    
                case BoosterType.ColorBomb:
                    await PlayColorBombUseAnimationAsync(scene, worldPosition, args.Duration);
                    break;
                    
                default:
                    break;
            }
        }

        /// <summary>
        /// 播放炸弹道具动画
        /// </summary>
        private static async ETTask PlayBombAnimationAsync(Scene scene, Vector3 position, float duration)
        {
            // 获取特效池
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            var fxPool = match3Board?.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            // 播放炸弹爆炸特效
            // 可以使用包装糖果爆炸特效（大范围爆炸）
            fxPool.PlayWrappedCandyExplosion(position);
            
            await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(duration * 1000));
        }

        /// <summary>
        /// 播放彩色炸弹道具使用动画
        /// </summary>
        private static async ETTask PlayColorBombUseAnimationAsync(Scene scene, Vector3 position, float duration)
        {
            // 获取特效池
            var match3Board = scene.GetComponent<Match3BoardComponent>();
            var fxPool = match3Board?.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            // 播放生成特效
            fxPool.PlaySpawnParticles(position);
            
            await scene.Root().GetComponent<TimerComponent>().WaitAsync((long)(duration * 1000));
        }
    }
}
