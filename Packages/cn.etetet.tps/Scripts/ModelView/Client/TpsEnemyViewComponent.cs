using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS敌人视图组件
    /// 管理敌人的3D显示对象
    /// </summary>
    [ComponentOf(typeof(TpsEnemyComponent))]
    public class TpsEnemyViewComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 敌人GameObject
        /// </summary>
        public GameObject GameObject;
        
        /// <summary>
        /// 敌人Transform
        /// </summary>
        public Transform Transform;
        
        /// <summary>
        /// 世界坐标位置
        /// </summary>
        public Vector3 WorldPosition;
    }
}
