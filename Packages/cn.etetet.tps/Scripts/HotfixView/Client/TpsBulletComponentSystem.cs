using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS子弹组件系统
    /// 处理单个子弹的生命周期和判定逻辑
    /// </summary>
    [FriendOf(typeof(TpsBulletComponent))]
    [FriendOf(typeof(TpsBulletManagerComponent))]
    [EntitySystemOf(typeof(TpsBulletComponent))]
    public static partial class TpsBulletComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TpsBulletComponent self, TpsBulletConfig config, Vector3 origin, Vector3 direction)
        {
            self.Config = config;
            self.Origin = origin;
            self.Direction = direction.normalized;
            self.CurrentPosition = origin;
            self.State = TpsBulletState.Active;
            self.TraveledDistance = 0f;
            self.CreateTime = TimeInfo.Instance.ServerNow();
            self.OwnerId = 0;
            self.BulletGO = null;
            self.TracerGO = null;

            if (config.BulletType == TpsBulletType.Hitscan)
            {
                self.ProcessHitscan();
            }
            else
            {
                self.InitializeProjectile();
            }
        }

        [EntitySystem]
        private static void Update(this TpsBulletComponent self)
        {
            if (self.State != TpsBulletState.Active) return;
            if (self.Config.BulletType == TpsBulletType.Projectile)
            {
                self.UpdateProjectile();
            }
        }

        [EntitySystem]
        private static void Destroy(this TpsBulletComponent self)
        {
            if (self.BulletGO != null)
            {
                UnityEngine.Object.Destroy(self.BulletGO);
                self.BulletGO = null;
            }
            if (self.TracerGO != null)
            {
                UnityEngine.Object.Destroy(self.TracerGO);
                self.TracerGO = null;
            }
        }

        #endregion

        #region Hitscan 逻辑

        private static void ProcessHitscan(this TpsBulletComponent self)
        {
            // 仅用于确定 tracer 视觉终点，命中检测由 TpsFireEvent_HitDetectionView (Physics2D) 处理
            bool didHit = Physics.Raycast(self.Origin, self.Direction, out RaycastHit hitInfo, self.Config.MaxRange);
            Vector3 endPoint;

            if (didHit)
            {
                endPoint = hitInfo.point;
                Log.Debug($"[TPS] Hitscan tracer 终点（遮挡）: {hitInfo.collider.name} at {hitInfo.point}");
            }
            else
            {
                endPoint = self.Origin + self.Direction * self.Config.MaxRange;
            }

            self.SpawnTracer(self.Origin, endPoint);
            self.SpawnMuzzleFlash();
            self.State = TpsBulletState.Destroyed;
        }

        #endregion

        #region Projectile 逻辑

        private static void InitializeProjectile(this TpsBulletComponent self)
        {
            self.SpawnMuzzleFlash();

            // TODO: 异步加载子弹预制体，目前使用简单球体代替
            self.BulletGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            self.BulletGO.name = $"Bullet_{self.Id}";
            self.BulletGO.transform.position = self.Origin;
            self.BulletGO.transform.localScale = Vector3.one * 0.2f;

            Collider collider = self.BulletGO.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;

            Log.Debug($"[TPS] Projectile 初始化: Origin={self.Origin}");
        }

        private static void UpdateProjectile(this TpsBulletComponent self)
        {
            if (self.BulletGO == null)
            {
                self.State = TpsBulletState.Destroyed;
                return;
            }

            float deltaTime = Time.deltaTime;
            float moveDistance = self.Config.Speed * deltaTime;

            bool didHit = Physics.Raycast(self.CurrentPosition, self.Direction, out RaycastHit hitInfo, moveDistance);

            if (didHit)
            {
                self.CurrentPosition = hitInfo.point;
                self.OnHit(hitInfo.point, hitInfo.normal, hitInfo.collider.gameObject);
                self.State = TpsBulletState.Destroyed;
                return;
            }

            self.CurrentPosition += self.Direction * moveDistance;
            self.TraveledDistance += moveDistance;
            self.BulletGO.transform.position = self.CurrentPosition;

            if (self.TraveledDistance >= self.Config.MaxRange)
            {
                Log.Debug($"[TPS] Projectile 超过最大射程，销毁");
                self.State = TpsBulletState.Destroyed;
            }
        }

        #endregion

        #region 通用方法

        private static void OnHit(this TpsBulletComponent self, Vector3 hitPoint, Vector3 hitNormal, GameObject hitObject)
        {
            Log.Info($"[TPS] 子弹命中: Object={hitObject.name}, Point={hitPoint}, Damage={self.Config.Damage}");
            self.SpawnHitVfx(hitPoint, hitNormal);

            if (self.Config.BulletType == TpsBulletType.Projectile && self.Config.ExplosionRadius > 0)
            {
                Log.Debug($"[TPS] 爆炸范围伤害: Radius={self.Config.ExplosionRadius}");
                // TODO: 实现范围伤害检测
            }
            // TODO: 对目标造成伤害
        }

        /// <summary>
        /// 生成弹道轨迹效果
        /// 使用 LineRenderer 创建可见的弹道线 (3D)
        /// </summary>
        private static void SpawnTracer(this TpsBulletComponent self, Vector3 start, Vector3 end)
        {
            // 创建 Tracer GameObject
            GameObject tracerGO = new GameObject($"Tracer_{self.Id}");
            tracerGO.transform.position = start;

            // 添加 LineRenderer 组件
            LineRenderer lineRenderer = tracerGO.AddComponent<LineRenderer>();

            // 设置线条的起点和终点
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            // 设置线条宽度
            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.002f;

            // 使用 Sprites/Default 材质并设置颜色
            Material tracerMat = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material = tracerMat;

            // 设置明亮的纯色（亮黄色）确保可见
            lineRenderer.startColor = new Color(1f, 1f, 0f, 1f);  // 纯黄色
            lineRenderer.endColor = new Color(1f, 0.5f, 0f, 1f);  // 橙色

            // 设置为 World Space
            lineRenderer.useWorldSpace = true;

            // 设置渲染排序，确保在所有 Sprite 之上
            lineRenderer.sortingOrder = 999;
            lineRenderer.sortingLayerName = "UI";

            // 启动渐隐协程
            FadeOutTracer(self.Root(), tracerGO, lineRenderer, 0.3f).NoContext();
        }

        /// <summary>
        /// 弹道轨迹渐隐效果 (独立于 Bullet Entity 生命周期)
        /// </summary>
        private static async ETTask FadeOutTracer(Scene scene, GameObject tracerGO, LineRenderer lineRenderer, float duration)
        {
            float elapsed = 0f;
            float startWidth = lineRenderer.startWidth;
            float endWidth = 0f; // 最终变细到0
            Color initialStartColor = lineRenderer.startColor;
            Color initialEndColor = lineRenderer.endColor;

            TimerComponent timer = scene.GetComponent<TimerComponent>();

            while (elapsed < duration)
            {
                if (tracerGO == null || lineRenderer == null)
                {
                    break;
                }

                float t = elapsed / duration;
                float fadeMultiplier = 1f - t;

                // 渐变宽度
                lineRenderer.startWidth = startWidth * fadeMultiplier;
                lineRenderer.endWidth = endWidth * fadeMultiplier;

                // 渐变透明度
                Color newStartColor = initialStartColor;
                Color newEndColor = initialEndColor;
                newStartColor.a = fadeMultiplier;
                newEndColor.a = fadeMultiplier * 0.8f;
                lineRenderer.startColor = newStartColor;
                lineRenderer.endColor = newEndColor;

                elapsed += Time.deltaTime;

                await timer.WaitFrameAsync();
            }

            // 销毁 Tracer
            if (tracerGO != null)
            {
                UnityEngine.Object.Destroy(tracerGO);
            }
        }

        private static void SpawnMuzzleFlash(this TpsBulletComponent self)
        {
            if (string.IsNullOrEmpty(self.Config.MuzzleFlashAssetPath)) return;
            // TODO: 异步加载枪口火焰预制体
            Log.Debug($"[TPS] MuzzleFlash at {self.Origin}");
        }

        private static void SpawnHitVfx(this TpsBulletComponent self, Vector3 position, Vector3 normal)
        {
            if (string.IsNullOrEmpty(self.Config.HitVfxAssetPath)) return;
            // TODO: 异步加载命中特效预制体
            Log.Debug($"[TPS] HitVFX at {position}");
        }

        #endregion
    }
}
