using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    [NinoType(false)]
    [Message(Opcode.MoveInfo)]
    public partial class MoveInfo : MessageObject
    {
        public static MoveInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<MoveInfo>(isFromPool);
        }

        [NinoMember(0)]
        public List<Unity.Mathematics.float3> Points { get; set; } = new();

        [NinoMember(1)]
        public Unity.Mathematics.quaternion Rotation { get; set; }
        [NinoMember(2)]
        public int TurnSpeed { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.C2M_PathfindingResult)]
    public partial class C2M_PathfindingResult : MessageObject, ILocationMessage
    {
        public static C2M_PathfindingResult Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_PathfindingResult>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public Unity.Mathematics.float3 Position { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.C2M_Stop)]
    public partial class C2M_Stop : MessageObject, ILocationMessage
    {
        public static C2M_Stop Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_Stop>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_PathfindingResult)]
    public partial class M2C_PathfindingResult : MessageObject, ICurrentMessage
    {
        public static M2C_PathfindingResult Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_PathfindingResult>(isFromPool);
        }

        [NinoMember(0)]
        public long Id { get; set; }
        [NinoMember(1)]
        public Unity.Mathematics.float3 Position { get; set; }
        [NinoMember(2)]
        public List<Unity.Mathematics.float3> Points { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_Stop)]
    public partial class M2C_Stop : MessageObject, ICurrentMessage
    {
        public static M2C_Stop Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_Stop>(isFromPool);
        }

        [NinoMember(0)]
        public int Error { get; set; }
        [NinoMember(1)]
        public long Id { get; set; }
        [NinoMember(2)]
        public Unity.Mathematics.float3 Position { get; set; }
        [NinoMember(3)]
        public Unity.Mathematics.quaternion Rotation { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_Turn)]
    public partial class M2C_Turn : MessageObject, ICurrentMessage
    {
        public static M2C_Turn Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_Turn>(isFromPool);
        }

        [NinoMember(0)]
        public long UnitId { get; set; }
        [NinoMember(1)]
        public Unity.Mathematics.quaternion Rotation { get; set; }
        [NinoMember(2)]
        public int TurnTime { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort MoveInfo = 10301;
        public const ushort C2M_PathfindingResult = 10302;
        public const ushort C2M_Stop = 10303;
        public const ushort M2C_PathfindingResult = 10304;
        public const ushort M2C_Stop = 10305;
        public const ushort M2C_Turn = 10306;
    }
}