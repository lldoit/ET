using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 彩色炸弹视图组件（Unity GameObject相关）
    /// </summary>
    [ComponentOf(typeof(ColorBombComponent))]
    public class ColorBombViewComponent : Entity, IAwake<GameObject>, IDestroy
    {
        public GameObject GameObject { get; set; }
        public Animator Animator { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
    }
}

