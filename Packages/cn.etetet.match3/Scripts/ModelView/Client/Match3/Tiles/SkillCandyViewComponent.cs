using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 技能糖果视图组件
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class SkillCandyViewComponent : Entity, IAwake<GameObject>, IDestroy
    {
        public GameObject GameObject { get; set; }
        public Animator Animator { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
    }
}
