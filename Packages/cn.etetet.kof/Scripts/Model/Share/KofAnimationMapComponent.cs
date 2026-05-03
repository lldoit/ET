using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 动画查表对应的组件
    /// </summary>
    [ComponentOf(typeof(KofFighterComponent))]
    public class KofAnimationMapComponent : Entity, IAwake
    {
        public Dictionary<int, KofAnimationMapConfig> MoveMaps = new();
    }
}
