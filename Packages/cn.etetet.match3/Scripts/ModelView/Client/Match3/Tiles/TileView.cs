using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI瓦片视图基类
    /// 用于UI渲染模式下的瓦片显示
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class TileView : Entity, IAwake<RectTransform>, IDestroy
    {
        /// <summary>
        /// RectTransform引用
        /// </summary>
        public RectTransform RectTransform { get; set; }

        /// <summary>
        /// Image组件引用
        /// </summary>
        public UnityEngine.UI.Image Image { get; set; }

        /// <summary>
        /// GameObject引用
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// 源预制体引用（用于对象池回收）
        /// </summary>
        public GameObject Prefab { get; set; }
    }


}
