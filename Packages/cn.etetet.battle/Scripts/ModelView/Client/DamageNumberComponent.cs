using DamageNumbersPro;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 飘字管理组件 - 管理战斗中的伤害/治疗飘字
    /// 只包含数据，不包含方法
    /// 所有逻辑请使用 DamageNumberComponentSystem 扩展方法
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class DamageNumberComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 飘字容器（BattlePanel的RectTransform）
        /// </summary>
        public RectTransform Container;

        /// <summary>
        /// UI相机引用
        /// </summary>
        public Camera UICamera;

        /// <summary>
        /// 正常伤害飘字预制体
        /// </summary>
        public DamageNumberGUI NormalDamagePrefab;

        /// <summary>
        /// 暴击伤害飘字预制体
        /// </summary>
        public DamageNumberGUI CriticalDamagePrefab;

        /// <summary>
        /// 治疗飘字预制体
        /// </summary>
        public DamageNumberGUI HealPrefab;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized;

        /// <summary>
        /// 下一个飘字的显示时间（毫秒时间戳）
        /// 用于跨多个攻击统一管理飘字显示间隔
        /// </summary>
        public long NextShowTimeMs;
    }
}
