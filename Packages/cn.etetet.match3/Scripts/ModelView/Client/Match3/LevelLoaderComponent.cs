using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡加载器组件
    /// 使用YooAssets异步加载关卡JSON文件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class LevelLoaderComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 关卡缓存
        /// </summary>
        public Dictionary<int, Level> LevelCache;
        
        /// <summary>
        /// 关卡资源路径前缀
        /// </summary>
        public string LevelPathPrefix;
    }
}
