using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS 环境组件
    /// 管理环境根节点和所有视差层
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsEnvironmentComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 环境根节点 Transform
        /// </summary>
        public Transform EnvironmentRoot;
    }
}
