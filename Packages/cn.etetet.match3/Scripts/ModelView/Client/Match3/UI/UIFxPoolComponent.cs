using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI特效池组件
    /// 管理UI渲染模式下的粒子特效
    /// 使用ParticleSystem + Canvas排序方案
    /// </summary>
    [ComponentOf(typeof(Match3BoardComponent))]
    public class UIFxPoolComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 特效容器（需要设置Sorting Layer与Canvas配合）
        /// </summary>
        public RectTransform FxContainer;
        
        /// <summary>
        /// 特效的世界空间根节点
        /// 粒子特效仍在世界空间渲染，但位置跟随UI
        /// </summary>
        public Transform FxWorldRoot;
        
        #region 糖果爆炸特效Prefab
        public GameObject CandyExplosionBluePrefab;
        public GameObject CandyExplosionGreenPrefab;
        public GameObject CandyExplosionOrangePrefab;
        public GameObject CandyExplosionPurplePrefab;
        public GameObject CandyExplosionRedPrefab;
        public GameObject CandyExplosionYellowPrefab;
        #endregion
        
        #region 特殊糖果爆炸特效Prefab
        public GameObject SkillCandyExplosionPrefab;
        public GameObject ColorBombExplosionPrefab;
        #endregion
        
        /// <summary>
        /// 特效对象池
        /// </summary>
        public Dictionary<GameObject, Queue<GameObject>> FxPools;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized;
    }
}
