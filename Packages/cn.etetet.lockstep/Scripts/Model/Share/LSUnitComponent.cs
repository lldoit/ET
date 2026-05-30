using Nino.Core;

namespace ET
{
	[ComponentOf(typeof(LSWorld))]
	[NinoType]
	public partial class LSUnitComponent: LSEntity, IAwake, ISerializeToEntity
	{
	}
}