using DamageNumbersPro;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 飘字管理组件System - 处理战斗飘字显示逻辑
    /// </summary>
    [FriendOf(typeof(DamageNumberComponent))]
    [EntitySystemOf(typeof(DamageNumberComponent))]
    public static partial class DamageNumberComponentSystem
    {
        /// <summary>
        /// 正常伤害飘字资源名
        /// </summary>
        private const string NORMAL_DAMAGE_PREFAB = "DamageNumbers_Normal";

        /// <summary>
        /// 暴击伤害飘字资源名
        /// </summary>
        private const string CRITICAL_DAMAGE_PREFAB = "DamageNumbers_Critical";

        /// <summary>
        /// 治疗飘字资源名
        /// </summary>
        private const string HEAL_PREFAB = "DamageNumbers_Heal";

        /// <summary>
        /// 飘字显示间隔（毫秒）
        /// </summary>
        private const int DAMAGE_NUMBER_INTERVAL_MS = 200;

        [EntitySystem]
        private static void Awake(this DamageNumberComponent self)
        {
            self.IsInitialized = false;
        }

        [EntitySystem]
        private static void Destroy(this DamageNumberComponent self)
        {
            self.Container = null;
            self.UICamera = null;
            self.NormalDamagePrefab = null;
            self.CriticalDamagePrefab = null;
            self.HealPrefab = null;
            self.IsInitialized = false;
        }

        /// <summary>
        /// 初始化飘字组件
        /// </summary>
        /// <param name="self">飘字组件</param>
        /// <param name="container">飘字容器RectTransform</param>
        /// <param name="uiCamera">UI相机</param>
        public static async ETTask InitializeAsync(this DamageNumberComponent self, RectTransform container, Camera uiCamera)
        {
            self.Container = container;
            self.UICamera = uiCamera;

            // 通过YooAsset加载预制体
            var loader = self.Root().GetComponent<ResourcesLoaderComponent>();

            // 加载正常伤害预制体
            var normalPrefab = await loader.LoadAssetAsync<GameObject>(NORMAL_DAMAGE_PREFAB);
            if (normalPrefab != null)
            {
                self.NormalDamagePrefab = normalPrefab.GetComponent<DamageNumberGUI>();
            }

            // 加载暴击伤害预制体
            var criticalPrefab = await loader.LoadAssetAsync<GameObject>(CRITICAL_DAMAGE_PREFAB);
            if (criticalPrefab != null)
            {
                self.CriticalDamagePrefab = criticalPrefab.GetComponent<DamageNumberGUI>();
            }

            // 加载治疗预制体
            var healPrefab = await loader.LoadAssetAsync<GameObject>(HEAL_PREFAB);
            if (healPrefab != null)
            {
                self.HealPrefab = healPrefab.GetComponent<DamageNumberGUI>();
            }

            self.IsInitialized = true;
            Log.Info("[DamageNumber] 飘字组件初始化完成");
        }

        /// <summary>
        /// 显示正常伤害飘字
        /// </summary>
        /// <param name="self">飘字组件</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="damage">伤害值</param>
        public static void ShowNormalDamage(this DamageNumberComponent self, Vector3 worldPos, int damage)
        {
            if (!self.IsInitialized || self.NormalDamagePrefab == null)
            {
                Log.Warning("[DamageNumber] 正常伤害预制体未加载");
                return;
            }

            self.SpawnDamageNumber(self.NormalDamagePrefab, worldPos, damage);
        }

        /// <summary>
        /// 队列显示正常伤害飘字（全局统一管理延迟）
        /// </summary>
        /// <param name="self">飘字组件</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="damage">伤害值</param>
        public static void QueueNormalDamage(this DamageNumberComponent self, Vector3 worldPos, int damage)
        {
            long delayMs = self.CalculateQueueDelay();
            if (delayMs > 0)
            {
                self.ShowNormalDamageDelayedInternal(worldPos, damage, delayMs).NoContext();
            }
            else
            {
                self.ShowNormalDamage(worldPos, damage);
            }
        }

        /// <summary>
        /// 内部延迟显示正常伤害飘字
        /// </summary>
        private static async ETTask ShowNormalDamageDelayedInternal(this DamageNumberComponent self, Vector3 worldPos, int damage, long delayMs)
        {
            EntityRef<DamageNumberComponent> selfRef = self;
            await self.Root().GetComponent<TimerComponent>().WaitAsync(delayMs);
            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }
            self.ShowNormalDamage(worldPos, damage);
        }

        /// <summary>
        /// 显示暴击伤害飘字
        /// </summary>
        /// <param name="self">飘字组件</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="damage">伤害值</param>
        public static void ShowCriticalDamage(this DamageNumberComponent self, Vector3 worldPos, int damage)
        {
            if (!self.IsInitialized || self.CriticalDamagePrefab == null)
            {
                Log.Warning("[DamageNumber] 暴击伤害预制体未加载");
                return;
            }

            self.SpawnDamageNumber(self.CriticalDamagePrefab, worldPos, damage);
        }

        /// <summary>
        /// 队列显示暴击伤害飘字（全局统一管理延迟）
        /// </summary>
        /// <param name="self">飘字组件</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="damage">伤害值</param>
        public static void QueueCriticalDamage(this DamageNumberComponent self, Vector3 worldPos, int damage)
        {
            long delayMs = self.CalculateQueueDelay();
            if (delayMs > 0)
            {
                self.ShowCriticalDamageDelayedInternal(worldPos, damage, delayMs).NoContext();
            }
            else
            {
                self.ShowCriticalDamage(worldPos, damage);
            }
        }

        /// <summary>
        /// 内部延迟显示暴击伤害飘字
        /// </summary>
        private static async ETTask ShowCriticalDamageDelayedInternal(this DamageNumberComponent self, Vector3 worldPos, int damage, long delayMs)
        {
            EntityRef<DamageNumberComponent> selfRef = self;
            await self.Root().GetComponent<TimerComponent>().WaitAsync(delayMs);
            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }
            self.ShowCriticalDamage(worldPos, damage);
        }

        /// <summary>
        /// 显示治疗飘字
        /// </summary>
        /// <param name="self">飘字组件</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="healAmount">治疗量</param>
        public static void ShowHeal(this DamageNumberComponent self, Vector3 worldPos, int healAmount)
        {
            if (!self.IsInitialized || self.HealPrefab == null)
            {
                Log.Warning("[DamageNumber] 治疗预制体未加载");
                return;
            }

            self.SpawnDamageNumber(self.HealPrefab, worldPos, healAmount);
        }

        /// <summary>
        /// 队列显示治疗飘字（全局统一管理延迟）
        /// </summary>
        /// <param name="self">飘字组件</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="healAmount">治疗量</param>
        public static void QueueHeal(this DamageNumberComponent self, Vector3 worldPos, int healAmount)
        {
            long delayMs = self.CalculateQueueDelay();
            if (delayMs > 0)
            {
                self.ShowHealDelayedInternal(worldPos, healAmount, delayMs).NoContext();
            }
            else
            {
                self.ShowHeal(worldPos, healAmount);
            }
        }

        /// <summary>
        /// 内部延迟显示治疗飘字
        /// </summary>
        private static async ETTask ShowHealDelayedInternal(this DamageNumberComponent self, Vector3 worldPos, int healAmount, long delayMs)
        {
            EntityRef<DamageNumberComponent> selfRef = self;
            await self.Root().GetComponent<TimerComponent>().WaitAsync(delayMs);
            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }
            self.ShowHeal(worldPos, healAmount);
        }

        /// <summary>
        /// 计算队列延迟时间
        /// 基于全局时间戳，确保多个攻击的飘字不会重叠
        /// </summary>
        /// <returns>需要延迟的毫秒数</returns>
        private static long CalculateQueueDelay(this DamageNumberComponent self)
        {
            long nowMs = TimeInfo.Instance.ClientNow();

            // 如果上次显示时间已过期，可以立即显示
            if (nowMs >= self.NextShowTimeMs)
            {
                self.NextShowTimeMs = nowMs + DAMAGE_NUMBER_INTERVAL_MS;
                return 0;
            }

            // 计算需要延迟的时间
            long delayMs = self.NextShowTimeMs - nowMs;
            // 更新下次显示时间
            self.NextShowTimeMs += DAMAGE_NUMBER_INTERVAL_MS;
            return delayMs;
        }

        /// <summary>
        /// 生成飘字
        /// </summary>
        /// <param name="self">飘字组件</param>
        /// <param name="prefab">预制体</param>
        /// <param name="worldPos">UI空间世界坐标</param>
        /// <param name="value">数值</param>
        private static void SpawnDamageNumber(this DamageNumberComponent self, DamageNumberGUI prefab, Vector3 worldPos, int value)
        {
            if (self.Container == null || self.UICamera == null)
            {
                Log.Warning("[DamageNumber] 容器或相机未设置");
                return;
            }

            // 由于角色在UI空间，worldPos已经是UI空间的世界坐标
            // 直接使用Container的InverseTransformPoint转换为本地坐标
            Vector3 localPos = self.Container.InverseTransformPoint(worldPos);
            Vector2 anchoredPos = new Vector2(localPos.x, localPos.y);

            // 生成飘字
            DamageNumber dn = prefab.Spawn(anchoredPos, value);

            // 设置父级
            dn.SetAnchoredPosition(self.Container, anchoredPos);

            // 设置Canvas排序顺序，确保飘字显示在角色SpriteRenderer前面
            Canvas canvas = dn.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = dn.gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
        }
    }
}
