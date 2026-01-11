using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI瓦片池组件
    /// 管理所有UI瓦片的Prefab和对象池
    /// 用于UI渲染模式
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class UITilePoolComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 预填充数量
        /// </summary>
        public int PreFillCount = 10;
        
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
        /// 对象池根节点（隐藏）
        /// </summary>
        public RectTransform PoolRoot;
        
        /// <summary>
        /// 瓦片尺寸（像素）
        /// </summary>
        public Vector2 TileSize;
        
        /// <summary>
        /// 瓦片间距（像素）
        /// </summary>
        public Vector2 TileSpacing;
        
        #region 普通糖果Prefab (UI版本)
        public GameObject UICandyBluePrefab;
        public GameObject UICandyGreenPrefab;
        public GameObject UICandyOrangePrefab;
        public GameObject UICandyPurplePrefab;
        public GameObject UICandyRedPrefab;
        public GameObject UICandyYellowPrefab;
        #endregion
        
        #region 技能糖果Prefab (UI版本)
        public GameObject UISkillCandyBluePrefab;
        public GameObject UISkillCandyGreenPrefab;
        public GameObject UISkillCandyOrangePrefab;
        public GameObject UISkillCandyPurplePrefab;
        public GameObject UISkillCandyRedPrefab;
        public GameObject UISkillCandyYellowPrefab;
        #endregion
        
        #region 其他特殊瓦片Prefab
        public GameObject UIColorBombPrefab;
        public GameObject UIMarshmallowPrefab;
        public GameObject UIChocolatePrefab;
        public GameObject UIUnbreakablePrefab;
        #endregion
        
        #region 收集物Prefab
        public GameObject UICollectable1Prefab;
        public GameObject UICollectable2Prefab;
        public GameObject UICollectable3Prefab;
        #endregion
        
        #region 背景格子Prefab
        public GameObject UIBgCellLightPrefab;
        public GameObject UIBgCellDarkPrefab;
        #endregion
        
        /// <summary>
        /// UI对象池字典
        /// Key: 源Prefab, Value: 可用对象队列
        /// </summary>
        public Dictionary<GameObject, Queue<GameObject>> TilePools;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized;
    }
}
