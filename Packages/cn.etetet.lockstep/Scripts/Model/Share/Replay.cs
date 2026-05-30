using System.Collections.Generic;
using Nino.Core;

namespace ET
{
    [NinoType(false)]
    public partial class Replay: Object
    {
        [NinoMember(1)]
        public List<LockStepUnitInfo> UnitInfos;
        
        [NinoMember(2)]
        public List<OneFrameInputs> FrameInputs = new();
        
        [NinoMember(3)]
        public List<byte[]> Snapshots = new();
    }
}