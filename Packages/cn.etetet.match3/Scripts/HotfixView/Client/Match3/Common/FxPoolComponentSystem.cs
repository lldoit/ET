using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET.Client
{
    /// <summary>
    /// 特效池组件系统
    /// </summary>
    [FriendOf(typeof(FxPoolComponent))]
    [EntitySystemOf(typeof(FxPoolComponent))]
    public static partial class FxPoolComponentSystem
    {
        [EntitySystem]
        private static void Awake(this FxPoolComponent self)
        {
            // 初始化对象池字典
            self.EffectPools = new Dictionary<GameObject, Queue<GameObject>>();

            // 创建特效对象池根节点
            var poolRootObj = new GameObject("FxPoolRoot");
            poolRootObj.transform.SetParent(null);
            self.PoolRoot = poolRootObj.transform;
        }

        [EntitySystem]
        private static void Destroy(this FxPoolComponent self)
        {
            // 清理所有对象池
            if (self.PoolRoot != null)
            {
                UnityEngine.Object.Destroy(self.PoolRoot.gameObject);
                self.PoolRoot = null;
            }

            // 清理对象池字典
            if (self.EffectPools != null)
            {
                foreach (var pool in self.EffectPools.Values)
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
                self.EffectPools.Clear();
            }
        }

        /// <summary>
        /// 初始化特效池（加载所有特效预制件）
        /// </summary>
        public static async ETTask InitializeAsync(this FxPoolComponent self)
        {
            self.TotalCreated = 0;
            var resourcePackage = YooAssets.GetPackage("DefaultPackage");

            // 辅助方法：加载GameObject资源
            async ETTask<GameObject> LoadPrefabAsync(string location)
            {
                var handle = resourcePackage.LoadAssetAsync<GameObject>(location);
                await handle.Task;
                var prefab = handle.AssetObject as GameObject;
                return prefab;
            }

            // 加载普通糖果爆炸特效
            self.BlueCandyExplosion = await LoadPrefabAsync("BlueCandyMatchParticles");
            self.GreenCandyExplosion = await LoadPrefabAsync("GreenCandyMatchParticles");
            self.OrangeCandyExplosion = await LoadPrefabAsync("OrangeCandyMatchParticles");
            self.PurpleCandyExplosion = await LoadPrefabAsync("PurpleCandyMatchParticles");
            self.RedCandyExplosion = await LoadPrefabAsync("RedCandyMatchParticles");
            self.YellowCandyExplosion = await LoadPrefabAsync("YellowCandyMatchParticles");

            // 加载技能糖果爆炸特效
            self.SkillCandyExplosion = await LoadPrefabAsync("SkillCandyParticles");

            // 加载彩色炸弹爆炸特效
            self.ColorBombExplosion = await LoadPrefabAsync("ColorBombParticles");

            // 加载元素爆炸特效
            self.HoneyExplosion = await LoadPrefabAsync("HoneyParticles");
            self.IceExplosion = await LoadPrefabAsync("IceParticles");
            self.SyrupExplosion = await LoadPrefabAsync("SyrupParticles");

            // 加载特殊方块爆炸特效
            self.MarshmallowExplosion = await LoadPrefabAsync("MarshmallowParticles");
            self.ChocolateExplosion = await LoadPrefabAsync("ChocolateParticles");

            // 加载收集物爆炸特效
            self.CollectableExplosion = await LoadPrefabAsync("CollectablesParticles");

            // 加载生成特效（创建特殊糖果时的闪光效果）
            try
            {
                self.SpawnParticles = await LoadPrefabAsync("Spawn");
            }
            catch
            {
                Log.Warning("未找到 Spawn 特效资源，生成特效将不可用");
            }

            // 注意：ComplimentText 建议使用 YIUI Tips 系统实现，而非粒子特效

            // 预热常用特效
            self.WarmUp(self.RedCandyExplosion, 5);
            self.WarmUp(self.BlueCandyExplosion, 5);
            self.WarmUp(self.GreenCandyExplosion, 5);
            self.WarmUp(self.YellowCandyExplosion, 5);
            self.WarmUp(self.OrangeCandyExplosion, 5);
            self.WarmUp(self.PurpleCandyExplosion, 5);
            self.WarmUp(self.SpawnParticles, 5);
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        public static void WarmUp(this FxPoolComponent self, GameObject prefab, int count)
        {
            if (prefab == null || count <= 0) return;

            if (!self.EffectPools.ContainsKey(prefab))
            {
                self.EffectPools[prefab] = new Queue<GameObject>();
            }

            var pool = self.EffectPools[prefab];
            for (int i = 0; i < count; i++)
            {
                GameObject obj = UnityEngine.Object.Instantiate(prefab, self.PoolRoot);
                obj.SetActive(false);
                pool.Enqueue(obj);
                self.TotalCreated++;
            }
        }

        /// <summary>
        /// 获取普通糖果爆炸特效预制件
        /// </summary>
        public static GameObject GetCandyExplosionPrefab(this FxPoolComponent self, CandyColor color)
        {
            return color switch
            {
                CandyColor.Blue => self.BlueCandyExplosion,
                CandyColor.Green => self.GreenCandyExplosion,
                CandyColor.Orange => self.OrangeCandyExplosion,
                CandyColor.Purple => self.PurpleCandyExplosion,
                CandyColor.Red => self.RedCandyExplosion,
                CandyColor.Yellow => self.YellowCandyExplosion,
                _ => self.RedCandyExplosion
            };
        }

        /// <summary>
        /// 获取技能糖果爆炸特效预制件
        /// </summary>
        public static GameObject GetSkillCandyExplosionPrefab(this FxPoolComponent self)
        {
            return self.SkillCandyExplosion;
        }

        /// <summary>
        /// 播放特效（从对象池获取或创建新实例）
        /// </summary>
        public static GameObject PlayEffect(this FxPoolComponent self, GameObject prefab, Vector3 position)
        {
            if (prefab == null)
            {
                Log.Warning("特效预制件为空，无法播放特效");
                return null;
            }

            GameObject effectObj = null;

            // 尝试从对象池获取
            if (self.EffectPools.TryGetValue(prefab, out var pool) && pool.Count > 0)
            {
                effectObj = pool.Dequeue();
                if (effectObj != null)
                {
                    effectObj.transform.position = position;
                    effectObj.SetActive(true);
                }
            }

            // 如果对象池中没有，创建新实例
            if (effectObj == null)
            {
                effectObj = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
                self.TotalCreated++;
                if (self.PoolRoot != null)
                {
                    effectObj.transform.SetParent(self.PoolRoot);
                }
            }

            // 播放所有粒子系统
            var particleSystems = effectObj.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }

            // 智能计算回收时间
            float duration = self.GetParticleDuration(effectObj);
            // 异步回收（保留一点缓冲时间）
            self.ScheduleReturnEffect(effectObj, prefab, duration + 0.2f).NoContext();

            return effectObj;
        }

        /// <summary>
        /// 异步回收特效
        /// </summary>
        private static async ETTask ScheduleReturnEffect(this FxPoolComponent self, GameObject effectObj, GameObject prefab, float delay)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync((long)(delay * 1000));

            if (effectObj != null && effectObj.activeSelf)
            {
                // 停止粒子系统
                var particleSystems = effectObj.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    ps.Stop();
                    ps.Clear();
                }

                self.ReturnEffect(effectObj, prefab);
            }
        }

        /// <summary>
        /// 回收特效到对象池
        /// </summary>
        public static void ReturnEffect(this FxPoolComponent self, GameObject effectObj, GameObject prefab)
        {
            if (effectObj == null || prefab == null)
            {
                return;
            }

            effectObj.SetActive(false);

            if (!self.EffectPools.ContainsKey(prefab))
            {
                self.EffectPools[prefab] = new Queue<GameObject>();
            }

            self.EffectPools[prefab].Enqueue(effectObj);
        }

        /// <summary>
        /// 播放普通糖果爆炸特效
        /// </summary>
        public static GameObject PlayCandyExplosion(this FxPoolComponent self, CandyColor color, Vector3 position)
        {
            var prefab = self.GetCandyExplosionPrefab(color);
            return self.PlayEffect(prefab, position);
        }

        /// <summary>
        /// 播放技能糖果爆炸特效
        /// </summary>
        public static GameObject PlaySkillCandyExplosion(this FxPoolComponent self, Vector3 position)
        {
            var prefab = self.GetSkillCandyExplosionPrefab();
            return self.PlayEffect(prefab, position);
        }

        /// <summary>
        /// 播放彩色炸弹爆炸特效
        /// </summary>
        public static GameObject PlayColorBombExplosion(this FxPoolComponent self, Vector3 position)
        {
            return self.PlayEffect(self.ColorBombExplosion, position);
        }

        /// <summary>
        /// 播放元素爆炸特效
        /// </summary>
        public static GameObject PlayElementExplosion(this FxPoolComponent self, ElementType elementType, Vector3 position)
        {
            GameObject prefab = elementType switch
            {
                ElementType.Honey => self.HoneyExplosion,
                ElementType.Ice => self.IceExplosion,
                ElementType.Syrup1 => self.SyrupExplosion,
                ElementType.Syrup2 => self.SyrupExplosion,
                _ => null
            };

            return self.PlayEffect(prefab, position);
        }

        /// <summary>
        /// 播放特殊方块爆炸特效
        /// </summary>
        public static GameObject PlaySpecialBlockExplosion(this FxPoolComponent self, SpecialBlockType blockType, Vector3 position)
        {
            GameObject prefab = blockType switch
            {
                SpecialBlockType.Marshmallow => self.MarshmallowExplosion,
                SpecialBlockType.Chocolate => self.ChocolateExplosion,
                _ => null
            };

            return self.PlayEffect(prefab, position);
        }

        /// <summary>
        /// 播放收集物爆炸特效
        /// </summary>
        public static GameObject PlayCollectableExplosion(this FxPoolComponent self, Vector3 position)
        {
            return self.PlayEffect(self.CollectableExplosion, position);
        }

        /// <summary>
        /// 播放生成特效（创建特殊糖果时显示）
        /// </summary>
        public static GameObject PlaySpawnParticles(this FxPoolComponent self, Vector3 position)
        {
            if (self.SpawnParticles == null)
            {
                return null;
            }

            var effectObj = self.PlayEffect(self.SpawnParticles, position);

            // 播放所有子粒子系统
            if (effectObj != null)
            {
                var particleSystems = effectObj.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    ps.Play();
                }
            }

            return effectObj;
        }

        /// <summary>
        /// 根据连续消除次数获取表扬类型
        /// 2次=Good，4次=Super，6次=Yummy
        /// </summary>
        /// <param name="cascadeCount">连续消除次数</param>
        /// <returns>表扬类型，如果次数不足则返回null</returns>
        public static ComplimentType? GetComplimentType(int cascadeCount)
        {
            if (cascadeCount >= 6)
            {
                return ComplimentType.Yummy;
            }
            else if (cascadeCount >= 4)
            {
                return ComplimentType.Super;
            }
            else if (cascadeCount >= 2)
            {
                return ComplimentType.Good;
            }

            return null;
        }

        /// <summary>
        /// 判断是否应该显示表扬（仅在达到2/4/6时显示）
        /// </summary>
        public static bool ShouldShowCompliment(int cascadeCount)
        {
            return cascadeCount == 2 || cascadeCount == 4 || cascadeCount == 6;
        }

        /// <summary>
        /// 获取粒子系统最大时长
        /// </summary>
        private static float GetParticleDuration(this FxPoolComponent self, GameObject effectObj)
        {
            if (effectObj == null) return 2.0f;

            float maxDuration = 0f;
            var particleSystems = effectObj.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                if (ps.main.loop) continue; // 忽略循环粒子
                float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                if (duration > maxDuration)
                {
                    maxDuration = duration;
                }
            }

            return maxDuration > 0 ? maxDuration : 2.0f; // 默认2秒
        }

        /// <summary>
        /// 获取当前池状态信息
        /// </summary>
        public static string GetPoolStats(this FxPoolComponent self)
        {
            int pooledCount = 0;
            if (self.EffectPools != null)
            {
                foreach (var pool in self.EffectPools.Values)
                {
                    pooledCount += pool.Count;
                }
            }

            return $"Total Created: {self.TotalCreated}, Pooled: {pooledCount}, Active: {self.TotalCreated - pooledCount}";
        }
    }
}

