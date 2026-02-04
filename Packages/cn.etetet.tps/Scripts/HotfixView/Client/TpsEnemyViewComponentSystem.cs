using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS敌人视图系统
    /// 管理敌人的3D显示
    /// </summary>
    [FriendOf(typeof(TpsEnemyViewComponent))]
    [FriendOf(typeof(TpsEnemyComponent))]
    [EntitySystemOf(typeof(TpsEnemyViewComponent))]
    public static partial class TpsEnemyViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsEnemyViewComponent self)
        {
            // 创建简单的敌人显示对象（红色立方体）
            self.GameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            self.GameObject.name = "TpsEnemy";
            self.Transform = self.GameObject.transform;
            
            // 设置红色材质
            Renderer renderer = self.GameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
            
            // 设置默认位置（屏幕前方）
            self.WorldPosition = new Vector3(0, 1, 10);
            self.Transform.position = self.WorldPosition;
            self.Transform.localScale = new Vector3(1, 2, 1);
            
            Log.Info($"[TPS] 敌人视图创建完成");
        }

        [EntitySystem]
        private static void Destroy(this TpsEnemyViewComponent self)
        {
            if (self.GameObject != null)
            {
                UnityEngine.Object.Destroy(self.GameObject);
                self.GameObject = null;
            }
            self.Transform = null;
        }

        /// <summary>
        /// 设置敌人世界坐标位置
        /// </summary>
        public static void SetWorldPosition(this TpsEnemyViewComponent self, Vector3 position)
        {
            self.WorldPosition = position;
            if (self.Transform != null)
            {
                self.Transform.position = position;
            }
        }

        /// <summary>
        /// 播放受击效果
        /// </summary>
        public static void PlayHitEffect(this TpsEnemyViewComponent self)
        {
            // 简单的闪烁效果
            if (self.GameObject != null)
            {
                Renderer renderer = self.GameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // 暂时变白表示受击
                    renderer.material.color = Color.white;
                    self.ResetColor().NoContext();
                }
            }
        }

        /// <summary>
        /// 重置颜色
        /// </summary>
        private static async ETTask ResetColor(this TpsEnemyViewComponent self)
        {
            EntityRef<TpsEnemyViewComponent> selfRef = self;
            
            await self.Root().GetComponent<TimerComponent>().WaitAsync(100);
            
            self = selfRef;
            if (self == null || self.IsDisposed || self.GameObject == null)
            {
                return;
            }
            
            Renderer renderer = self.GameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }

        /// <summary>
        /// 播放死亡效果
        /// </summary>
        public static void PlayDeathEffect(this TpsEnemyViewComponent self)
        {
            if (self.GameObject != null)
            {
                // 简单效果：变灰并缩小
                Renderer renderer = self.GameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.gray;
                }
                self.Transform.localScale = Vector3.one * 0.5f;
            }
        }
    }
}
