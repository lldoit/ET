using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS角色动画控制器（MonoBehaviour）
    /// 挂载到敌人预制体上，用于关联 Unity GameObject 与 ET Entity
    /// 同时作为 Physics2D Raycast 的命中目标标识
    /// </summary>
    public class TpsCharacterAnimancer : MonoBehaviour
    {
        /// <summary>
        /// 关联的敌人 Entity ID
        /// </summary>
        public long EnemyId;
    }
}
