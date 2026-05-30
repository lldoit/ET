using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    [NinoType(false)]
    [Message(Opcode.G2Map_EnterMap)]
    [ResponseType(nameof(Map2G_EnterMap))]
    public partial class G2Map_EnterMap : MessageObject, IRequest
    {
        public static G2Map_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2Map_EnterMap>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public long PlayerId { get; set; }
        [NinoMember(2)]
        public ActorId GateActorId { get; set; }
        [NinoMember(3)]
        public string MapName { get; set; }
        [NinoMember(4)]
        public long MapId { get; set; }
        [NinoMember(5)]
        public int UnitConfigId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.Map2G_EnterMap)]
    public partial class Map2G_EnterMap : MessageObject, IResponse
    {
        public static Map2G_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Map2G_EnterMap>(isFromPool);
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

    [NinoType(false)]
    [Message(Opcode.M2M_UnitTransferRequest)]
    [ResponseType(nameof(M2M_UnitTransferResponse))]
    public partial class M2M_UnitTransferRequest : MessageObject, IRequest, IEntityMessage
    {
        public static M2M_UnitTransferRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2M_UnitTransferRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public ActorId OldActorId { get; set; }
        [NinoMember(2)]
        public byte[] UnitBytes { get; set; }
        [NinoMember(3)]
        public List<byte[]> EntityBytes { get; set; } = new();

        [NinoMember(4)]
        public bool ChangeScene { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2M_UnitTransferResponse)]
    public partial class M2M_UnitTransferResponse : MessageObject, IResponse
    {
        public static M2M_UnitTransferResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2M_UnitTransferResponse>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        [NinoMember(3)]
        public ActorId NewActorId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort G2Map_EnterMap = 21301;
        public const ushort Map2G_EnterMap = 21302;
        public const ushort M2M_UnitTransferRequest = 21303;
        public const ushort M2M_UnitTransferResponse = 21304;
    }
}