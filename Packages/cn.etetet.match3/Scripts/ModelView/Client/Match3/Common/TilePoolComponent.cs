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

        #region UI相关字段

        /// <summary>
        /// 棋盘UI根节点
        /// </summary>
        public RectTransform BoardRoot;

        /// <summary>
        /// 背景格子容器
        /// </summary>
        public RectTransform CellContainer;

        /// <summary>
        /// 瓦片容器（用于可移动的瓦片）
        /// </summary>
        public RectTransform TileContainer;

        /// <summary>
        /// 瓦片遮罩容器（用于裁剪超出棋盘区域的瓦片）
        /// </summary>
        public RectTransform TileMaskContainer;

        /// <summary>
        /// UI对象池根节点（隐藏）
        /// </summary>
        public RectTransform UIPoolRoot;

        /// <summary>
        /// 瓦片尺寸（像素）
        /// </summary>
        public Vector2 TileSize;

        /// <summary>
        /// 瓦片间距（像素）
        /// </summary>
        public Vector2 TileSpacing;

        /// <summary>
        /// UI对象池字典
        /// Key: 源Prefab, Value: 可用对象队列
        /// </summary>
        public Dictionary<GameObject, Queue<GameObject>> UIPrefabPools;

        #endregion



        #region 普通糖果Prefab

        public GameObject BlueCandyPrefab;
        public GameObject GreenCandyPrefab;
        public GameObject RedCandyPrefab;
        public GameObject YellowCandyPrefab;

        #endregion

        #region 技能糖果Prefab

        public GameObject BlueSkillCandyPrefab;
        public GameObject GreenSkillCandyPrefab;
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
