using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 特效池组件（客户端视图层）
    /// 管理所有爆炸特效的预制件引用
    /// </summary>
    [ComponentOf(typeof(Match3BoardComponent))]
    public class FxPoolComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 普通糖果爆炸特效（按颜色）
        /// </summary>
        public GameObject BlueCandyExplosion;
        public GameObject GreenCandyExplosion;
        public GameObject OrangeCandyExplosion;
        public GameObject PurpleCandyExplosion;
        public GameObject RedCandyExplosion;
        public GameObject YellowCandyExplosion;

        /// <summary>
        /// 条纹糖果爆炸特效
        /// </summary>
        public GameObject HorizontalStripedCandyExplosion;
        public GameObject VerticalStripedCandyExplosion;

        /// <summary>
        /// 包装糖果爆炸特效
        /// </summary>
        public GameObject WrappedCandyExplosion;

        /// <summary>
        /// 彩色炸弹爆炸特效
        /// </summary>
        public GameObject ColorBombExplosion;

        /// <summary>
        /// 元素爆炸特效
        /// </summary>
        public GameObject HoneyExplosion;
        public GameObject IceExplosion;
        public GameObject SyrupExplosion;

        /// <summary>
        /// 特殊方块爆炸特效
        /// </summary>
        public GameObject MarshmallowExplosion;
        public GameObject ChocolateExplosion;

        /// <summary>
        /// 收集物爆炸特效
        /// </summary>
        public GameObject CollectableExplosion;

        /// <summary>
        /// 生成特效（创建特殊糖果时的闪光效果）
        /// </summary>
        public GameObject SpawnParticles;

        /// <summary>
        /// 特效对象池字典
        /// </summary>
        public Dictionary<GameObject, Queue<GameObject>> EffectPools;

        /// <summary>
        /// 特效对象池根节点
        /// </summary>
        public Transform PoolRoot;

        /// <summary>
        /// 已创建的特效总数
        /// </summary>
        public int TotalCreated;
    }
}

