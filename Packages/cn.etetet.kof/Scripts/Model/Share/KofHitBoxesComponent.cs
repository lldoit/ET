using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(KofFighterComponent))]
    public class KofHitBoxesComponent : Entity, IAwake
    {
        public List<KofHitBoxData> Boxes = new();
    }
}
