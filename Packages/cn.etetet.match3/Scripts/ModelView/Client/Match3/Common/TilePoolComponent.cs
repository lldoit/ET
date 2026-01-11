using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 瓦片池组件
    /// 管理所有类型瓦片的Prefab加载和对象池
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TilePoolComponent : Entity, IAwake, IDestroy
    {
        public int PreFillCount = 20;

        /// <summary>
        /// 瓦片对象池根节点
        /// </summary>
        public Transform PoolRoot;

        /// <summary>
        /// 棋盘根节点（用于放置活动瓦片）
        /// </summary>
        public Transform BoardRoot;

        #region 普通糖果Prefab

        public GameObject BlueCandyPrefab;
        public GameObject GreenCandyPrefab;
        public GameObject OrangeCandyPrefab;
        public GameObject PurpleCandyPrefab;
        public GameObject RedCandyPrefab;
        public GameObject YellowCandyPrefab;

        #endregion

        #region 技能糖果Prefab

        public GameObject BlueSkillCandyPrefab;
        public GameObject GreenSkillCandyPrefab;
        public GameObject OrangeSkillCandyPrefab;
        public GameObject PurpleSkillCandyPrefab;
        public GameObject RedSkillCandyPrefab;
        public GameObject YellowSkillCandyPrefab;

        #endregion


        #region 特殊糖果Prefab

        public GameObject ColorBombPrefab;

        #endregion

        #region 背景瓦片Prefab

        public GameObject LightBgTilePrefab;
        public GameObject DarkBgTilePrefab;

        #endregion

        #region 特殊方块Prefab

        public GameObject MarshmallowPrefab;
        public GameObject ChocolatePrefab;
        public GameObject UnbreakablePrefab;

        #endregion

        #region 元素Prefab

        public GameObject HoneyPrefab;
        public GameObject IcePrefab;
        public GameObject Syrup1Prefab;
        public GameObject Syrup2Prefab;

        #endregion

        #region 收集物Prefab

        public GameObject Collectable1Prefab;
        public GameObject Collectable2Prefab;
        public GameObject Collectable3Prefab;

        #endregion

        /// <summary>
        /// 对象池字典
        /// </summary>
        public Dictionary<GameObject, Queue<GameObject>> TilePools;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized;
    }
}
