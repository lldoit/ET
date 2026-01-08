using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 棉花糖视图组件（Unity GameObject相关）
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class MarshmallowViewComponent : Entity, IAwake<GameObject>, IDestroy
    {
        public GameObject GameObject { get; set; }
        public Animator Animator { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
    }
}



