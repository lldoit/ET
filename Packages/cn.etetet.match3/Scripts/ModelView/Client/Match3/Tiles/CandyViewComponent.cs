using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 糖果视图组件（Unity GameObject相关）
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class CandyViewComponent : Entity, IAwake<GameObject>, IDestroy
    {
        public GameObject GameObject { get; set; }
        public Animator Animator { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
    }
}

