using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 不可破坏视图组件（Unity GameObject相关）
    /// </summary>
    [ComponentOf(typeof(UnbreakableComponent))]
    public class UnbreakableViewComponent : Entity, IAwake<GameObject>, IDestroy
    {
        public GameObject GameObject { get; set; }
        public Animator Animator { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
    }
}



