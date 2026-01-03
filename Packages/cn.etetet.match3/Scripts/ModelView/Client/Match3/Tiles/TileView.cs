using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 瓦片视图基类（Unity GameObject相关）
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class TileView : Entity, IAwake<GameObject>, IDestroy
    {
        public GameObject GameObject { get; set; }
    }
}

