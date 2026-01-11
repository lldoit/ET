using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI特效池组件系统
    /// 使用ParticleSystem方案
    /// </summary>
    [FriendOf(typeof(UIFxPoolComponent))]
    [EntitySystemOf(typeof(UIFxPoolComponent))]
    public static partial class UIFxPoolComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIFxPoolComponent self)
        {
            self.FxPools = new Dictionary<GameObject, Queue<GameObject>>();
        }

        [EntitySystem]
        private static void Destroy(this UIFxPoolComponent self)
        {
            // 清理特效池
            if (self.FxPools != null)
            {
                foreach (var pool in self.FxPools.Values)
                {
                    while (pool.Count > 0)
                    {
                        var obj = pool.Dequeue();
                        if (obj != null)
                        {
                            UnityEngine.Object.Destroy(obj);
                        }
                    }
                }
                self.FxPools.Clear();
            }
            
            if (self.FxWorldRoot != null)
            {
                UnityEngine.Object.Destroy(self.FxWorldRoot.gameObject);
            }
            
            self.FxContainer = null;
            self.FxWorldRoot = null;
            self.IsInitialized = false;
        }

        /// <summary>
        /// 初始化特效池
        /// </summary>
        public static async ETTask InitializeAsync(this UIFxPoolComponent self)
        {
            if (self.IsInitialized) return;
            
            Log.Info("[UIFxPool] 开始初始化UI特效池");
            
            // 创建世界空间特效根节点
            if (self.FxWorldRoot == null)
            {
                var fxRootGo = new GameObject("UIFxWorldRoot");
                self.FxWorldRoot = fxRootGo.transform;
            }
            
            // TODO: 加载特效Prefab资源
            
            self.IsInitialized = true;
            
            Log.Info("[UIFxPool] UI特效池初始化完成");
            
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 从对象池获取特效
        /// </summary>
        private static GameObject GetFx(this UIFxPoolComponent self, GameObject prefab)
        {
            if (prefab == null) return null;
            
            if (!self.FxPools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                self.FxPools[prefab] = pool;
            }
            
            GameObject fxObj;
            if (pool.Count > 0)
            {
                fxObj = pool.Dequeue();
                fxObj.SetActive(true);
            }
            else
            {
                fxObj = UnityEngine.Object.Instantiate(prefab);
            }
            
            if (self.FxWorldRoot != null)
            {
                fxObj.transform.SetParent(self.FxWorldRoot, false);
            }
            
            return fxObj;
        }

        /// <summary>
        /// 回收特效到对象池
        /// </summary>
        private static void ReturnFx(this UIFxPoolComponent self, GameObject fxObj, GameObject prefab)
        {
            if (fxObj == null || prefab == null) return;
            
            if (!self.FxPools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                self.FxPools[prefab] = pool;
            }
            
            fxObj.SetActive(false);
            pool.Enqueue(fxObj);
        }

        /// <summary>
        /// 获取糖果爆炸特效Prefab
        /// </summary>
        private static GameObject GetCandyExplosionPrefab(this UIFxPoolComponent self, CandyColor color)
        {
            return color switch
            {
                CandyColor.Blue => self.CandyExplosionBluePrefab,
                CandyColor.Green => self.CandyExplosionGreenPrefab,
                CandyColor.Orange => self.CandyExplosionOrangePrefab,
                CandyColor.Purple => self.CandyExplosionPurplePrefab,
                CandyColor.Red => self.CandyExplosionRedPrefab,
                CandyColor.Yellow => self.CandyExplosionYellowPrefab,
                _ => null
            };
        }

        /// <summary>
        /// 播放糖果爆炸特效
        /// </summary>
        public static void PlayCandyExplosion(this UIFxPoolComponent self, CandyColor color, Vector2 uiPosition)
        {
            var prefab = self.GetCandyExplosionPrefab(color);
            if (prefab == null) return;
            
            var fxObj = self.GetFx(prefab);
            if (fxObj == null) return;
            
            // 将UI坐标转换为世界坐标（需要通过Canvas计算）
            // 这里简化处理，直接使用UI坐标作为本地坐标
            fxObj.transform.localPosition = new Vector3(uiPosition.x, uiPosition.y, 0);
            
            // 播放粒子
            var ps = fxObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
            
            // 延迟回收
            self.DelayReturnFx(fxObj, prefab, 2f).NoContext();
        }

        /// <summary>
        /// 播放技能糖果爆炸特效
        /// </summary>
        public static void PlaySkillCandyExplosion(this UIFxPoolComponent self, Vector2 uiPosition)
        {
            var prefab = self.SkillCandyExplosionPrefab;
            if (prefab == null) return;
            
            var fxObj = self.GetFx(prefab);
            if (fxObj == null) return;
            
            fxObj.transform.localPosition = new Vector3(uiPosition.x, uiPosition.y, 0);
            
            var ps = fxObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
            
            self.DelayReturnFx(fxObj, prefab, 2f).NoContext();
        }

        /// <summary>
        /// 播放彩色炸弹爆炸特效
        /// </summary>
        public static void PlayColorBombExplosion(this UIFxPoolComponent self, Vector2 uiPosition)
        {
            var prefab = self.ColorBombExplosionPrefab;
            if (prefab == null) return;
            
            var fxObj = self.GetFx(prefab);
            if (fxObj == null) return;
            
            fxObj.transform.localPosition = new Vector3(uiPosition.x, uiPosition.y, 0);
            
            var ps = fxObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
            
            self.DelayReturnFx(fxObj, prefab, 2f).NoContext();
        }

        /// <summary>
        /// 延迟回收特效
        /// </summary>
        private static async ETTask DelayReturnFx(this UIFxPoolComponent self, GameObject fxObj, GameObject prefab, float delay)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync((long)(delay * 1000));
            
            if (self.IsDisposed) return;
            
            self.ReturnFx(fxObj, prefab);
        }
    }
}
