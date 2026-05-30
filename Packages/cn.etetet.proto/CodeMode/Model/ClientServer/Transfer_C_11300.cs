using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    [NinoType(false)]
    [Message(Opcode.C2M_TransferMap)]
    [ResponseType(nameof(M2C_TransferMap))]
    public partial class C2M_TransferMap : MessageObject, ILocationRequest
    {
        public static C2M_TransferMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_TransferMap>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_TransferMap)]
    public partial class M2C_TransferMap : MessageObject, ILocationResponse
    {
        public static M2C_TransferMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_TransferMap>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort C2M_TransferMap = 11301;
        public const ushort M2C_TransferMap = 11302;
    }
}