using Nino.Core;

namespace ET
{
    [ComponentOf(typeof(LSUnit))]
    [NinoType]
    public partial class LSInputComponent: LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        public LSInput LSInput { get; set; }
    }
}