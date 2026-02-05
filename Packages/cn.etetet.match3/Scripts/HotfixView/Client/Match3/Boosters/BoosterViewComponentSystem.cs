using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 道具视图系统 - 处理道具的视觉表现逻辑
    /// </summary>
    [FriendOf(typeof(BoosterViewComponent))]
    [EntitySystemOf(typeof(BoosterViewComponent))]
    public static partial class BoosterViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BoosterViewComponent self)
        {
            // 初始化默认值
            self.LollipopAnimDuration = 300;
            self.BombAnimDuration = 500;
            self.ColorBombAnimDuration = 600;
            self.SwitchAnimDuration = 250;
        }

        [EntitySystem]
        private static void Destroy(this BoosterViewComponent self)
        {
            // 清理资源
            self.LollipopEffectPrefab = null;
            self.BombEffectPrefab = null;
            self.ColorBombEffectPrefab = null;
            self.SwitchEffectPrefab = null;
            self.EffectPool = null;
        }

        /// <summary>
        /// 播放棒棒糖道具效果
        /// </summary>
        public static async ETTask PlayLollipopEffectAsync(this BoosterViewComponent self, Vector3 worldPosition)
        {
            // 播放音效
            self.PlayBoosterSound(self.LollipopSound);

            // 播放特效
            if (self.LollipopEffectPrefab != null)
            {
                var effect = UnityEngine.Object.Instantiate(self.LollipopEffectPrefab, worldPosition, Quaternion.identity);
                UnityEngine.Object.Destroy(effect, self.LollipopAnimDuration / 1000f);
            }

            // 等待动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(self.LollipopAnimDuration);
        }

        /// <summary>
        /// 播放炸弹道具效果
        /// </summary>
        public static async ETTask PlayBombEffectAsync(this BoosterViewComponent self, Vector3 worldPosition)
        {
            // 播放音效
            self.PlayBoosterSound(self.BombSound);

            // 播放特效
            if (self.BombEffectPrefab != null)
            {
                var effect = UnityEngine.Object.Instantiate(self.BombEffectPrefab, worldPosition, Quaternion.identity);

                // 炸弹特效可能需要扩散动画
                var particleSystem = effect.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }

                UnityEngine.Object.Destroy(effect, self.BombAnimDuration / 1000f);
            }

            // 等待动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(self.BombAnimDuration);
        }

        /// <summary>
        /// 播放彩色炸弹道具效果
        /// </summary>
        public static async ETTask PlayColorBombEffectAsync(this BoosterViewComponent self, Vector3 worldPosition)
        {
            // 播放音效
            self.PlayBoosterSound(self.ColorBombSound);

            // 播放特效
            if (self.ColorBombEffectPrefab != null)
            {
                var effect = UnityEngine.Object.Instantiate(self.ColorBombEffectPrefab, worldPosition, Quaternion.identity);

                // 彩色炸弹生成特效
                var particleSystem = effect.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }

                UnityEngine.Object.Destroy(effect, self.ColorBombAnimDuration / 1000f);
            }

            // 等待动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(self.ColorBombAnimDuration);
        }

        /// <summary>
        /// 播放交换道具效果
        /// </summary>
        public static async ETTask PlaySwitchEffectAsync(this BoosterViewComponent self, Vector3 worldPosition1, Vector3 worldPosition2)
        {
            // 播放音效
            self.PlayBoosterSound(self.SwitchSound);

            // 播放交换特效（在两个位置之间）
            if (self.SwitchEffectPrefab != null)
            {
                // 在两个瓦片位置之间的中点播放特效
                Vector3 midPoint = (worldPosition1 + worldPosition2) / 2f;
                var effect = UnityEngine.Object.Instantiate(self.SwitchEffectPrefab, midPoint, Quaternion.identity);

                // 可以添加连线效果
                var lineRenderer = effect.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, worldPosition1);
                    lineRenderer.SetPosition(1, worldPosition2);
                }

                UnityEngine.Object.Destroy(effect, self.SwitchAnimDuration / 1000f);
            }

            // 等待动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(self.SwitchAnimDuration);
        }

        /// <summary>
        /// 显示道具激活提示
        /// </summary>
        public static void ShowBoosterActivatedHint(this BoosterViewComponent self, BoosterType boosterType)
        {
            // 发布提示事件
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                string message = boosterType switch
                {
                    BoosterType.Lollipop => "点击要消除的瓦片",
                    BoosterType.Bomb => "点击要爆炸的位置",
                    BoosterType.ColorBomb => "点击要替换的瓦片",
                    BoosterType.Switch => "选择第一个瓦片",
                    _ => "选择目标瓦片"
                };

                EventSystem.Instance.Publish(scene, new ShowHintTextEvent
                {
                    Message = message,
                    Duration = 3f
                });
            }

            Log.Info($"道具 {boosterType} 已激活，请选择目标瓦片");
        }

        /// <summary>
        /// 隐藏道具激活提示
        /// </summary>
        public static void HideBoosterActivatedHint(this BoosterViewComponent self)
        {
            Log.Info("道具已取消激活");
        }

        /// <summary>
        /// 播放道具音效
        /// </summary>
        private static void PlayBoosterSound(this BoosterViewComponent self, string soundName)
        {
            // 使用Match3AudioHelper播放音效
            Scene clientScene = self.Root() as Scene;

            if (clientScene != null)
            {
                // 根据音效名称调用对应的方法
                switch (soundName)
                {
                    case "BoosterLollipop":
                        Match3AudioHelper.PlayBoosterLollipopSound(clientScene);
                        break;
                    case "BoosterBomb":
                        Match3AudioHelper.PlayBoosterBombSound(clientScene);
                        break;
                    case "BoosterColorBomb":
                        Match3AudioHelper.PlayBoosterColorBombSound(clientScene);
                        break;
                    case "BoosterSwitch":
                        Match3AudioHelper.PlayBoosterSwitchSound(clientScene);
                        break;
                    default:
                        Log.Warning($"未知的道具音效: {soundName}");
                        break;
                }
            }
        }

        /// <summary>
        /// 显示道具使用动画（瓦片消失前的特效）
        /// 根据瓦片类型播放对应的消除特效
        /// </summary>
        /// <param name="self">道具视图组件</param>
        /// <param name="boosterType">使用的道具类型</param>
        /// <param name="tile">被消除的瓦片</param>
        /// <param name="worldPosition">世界坐标位置（UI模式下忽略）</param>
        public static async ETTask ShowTileDestroyedByBoosterAsync(this BoosterViewComponent self, BoosterType boosterType, Tile tile, Vector3 worldPosition)
        {
            // 获取棋盘组件并播放瓦片对应的消除特效
            var match3Board = self.Scene().GetComponent<Match3BoardComponent>();
            if (match3Board != null && tile != null)
            {
                // UI模式：使用瓦片坐标播放特效
                int x = tile.GetX();
                int y = tile.GetY();
                match3Board.PlayUITileExplosionEffectAt(tile, x, y);
            }

            await ETTask.CompletedTask;
        }

    }
}

