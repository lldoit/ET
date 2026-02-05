using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS 环境系统
    /// 初始化和管理视差层
    /// </summary>
    [FriendOf(typeof(TpsEnvironmentComponent))]
    [EntitySystemOf(typeof(TpsEnvironmentComponent))]
    public static partial class TpsEnvironmentComponentSystem
    {
        #region 生命周期方法

        /// <summary>
        /// 初始化环境组件
        /// </summary>
        [EntitySystem]
        private static void Awake(this TpsEnvironmentComponent self)
        {
            // 初始化时不做任何事，等待 SetEnvironmentRoot 调用
        }

        /// <summary>
        /// 销毁时清理引用
        /// </summary>
        [EntitySystem]
        private static void Destroy(this TpsEnvironmentComponent self)
        {
            self.EnvironmentRoot = null;
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 设置环境根节点并自动创建视差层
        /// </summary>
        /// <param name="self">环境组件</param>
        /// <param name="environmentRoot">环境根节点</param>
        public static void SetEnvironmentRoot(this TpsEnvironmentComponent self, Transform environmentRoot)
        {
            self.EnvironmentRoot = environmentRoot;
            if (environmentRoot == null)
            {
                return;
            }

            // 遍历子节点，根据命名约定创建视差层
            foreach (Transform child in environmentRoot)
            {
                float parallaxFactor = self.GetParallaxFactorFromName(child.name);
                if (parallaxFactor >= 0)
                {
                    ParallaxLayerComponent layer = self.AddChild<ParallaxLayerComponent, float>(parallaxFactor);
                    layer.SetLayerTransform(child);
                    Log.Info($"[TPS] 创建视差层: {child.name}, Factor: {parallaxFactor}");
                }
            }
        }

        /// <summary>
        /// 根据命名约定解析视差系数
        /// 命名格式参考: Layer0_Sky, Layer1_City, Layer2_Mid, Layer3_Battle, Layer4_Fore
        /// </summary>
        /// <param name="self">环境组件</param>
        /// <param name="name">层级名称</param>
        /// <returns>视差系数，-1 表示不创建视差层</returns>
        private static float GetParallaxFactorFromName(this TpsEnvironmentComponent self, string name)
        {
            // 预定义的层级视差系数（可从配置表读取）
            if (name.Contains("Sky")) return 0.05f;
            if (name.Contains("City") || name.Contains("Far")) return 0.2f;
            if (name.Contains("Mid")) return 0.5f;
            if (name.Contains("Battle") || name.Contains("Base")) return 1.0f;
            if (name.Contains("Fore") || name.Contains("Front")) return 1.2f;
            
            // 未匹配的节点不创建视差层
            return -1f;
        }

        #endregion
    }
}
