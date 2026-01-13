using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 三消游戏输入处理组件
    /// 负责检测玩家点击/拖拽输入，分发到道具使用或普通交换逻辑
    /// 支持世界空间和UI空间双渲染模式
    /// </summary>
    [ComponentOf(typeof(Match3BoardComponent))]
    public class Match3InputComponent : Entity, IAwake, IUpdate
    {
        /// <summary>
        /// 是否正在拖拽
        /// </summary>
        public bool IsDragging;

        /// <summary>
        /// 拖拽起始瓦片坐标
        /// </summary>
        public int DragStartX;
        public int DragStartY;

        /// <summary>
        /// 拖拽起始世界坐标/UI坐标
        /// </summary>
        public Vector3 DragStartWorldPos;

        /// <summary>
        /// 游戏相机（用于屏幕坐标转世界坐标，世界空间模式）
        /// </summary>
        public Camera GameCamera;

        /// <summary>
        /// 棋盘根节点Transform（用于坐标转换）
        /// </summary>
        public Transform BoardTransform;

        /// <summary>
        /// 瓦片大小（世界单位）
        /// </summary>
        public float TileSize = 1.0f;

        /// <summary>
        /// 棋盘偏移量（世界坐标）
        /// </summary>
        public Vector2 BoardOffset;

        /// <summary>
        /// 最小拖拽距离（判断是否触发交换）
        /// </summary>
        public float MinDragDistance = 0.3f;

        /// <summary>
        /// 输入是否启用
        /// </summary>
        public bool InputEnabled = true;

        /// <summary>
        /// Switch道具模式下选中的瓦片坐标
        /// </summary>
        public int SwitchSelectedX = -1;
        public int SwitchSelectedY = -1;

        #region UI模式专用字段

        /// <summary>
        /// UI棋盘根节点RectTransform（UI模式）
        /// </summary>
        public RectTransform UIBoardRoot;

        /// <summary>
        /// 瓦片尺寸（像素，UI模式）
        /// </summary>
        public Vector2 UITileSize = new Vector2(100f, 100f);

        /// <summary>
        /// 瓦片间距（像素，UI模式）
        /// </summary>
        public Vector2 UITileSpacing = new Vector2(2f, 2f);

        /// <summary>
        /// UI Canvas（UI模式）
        /// </summary>
        public Canvas UICanvas;

        #endregion
    }
}
