using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    // Spell相关消息定义
    [NinoType(false)]
    [Message(Opcode.C2M_SpellCast)]
    public partial class C2M_SpellCast : MessageObject, ILocationMessage
    {
        public static C2M_SpellCast Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_SpellCast>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int SpellConfigId { get; set; }
        [NinoMember(2)]
        public Unity.Mathematics.float3 TargetPosition { get; set; }
        [NinoMember(3)]
        public long TargetUnitId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_SpellAdd)]
    public partial class M2C_SpellAdd : MessageObject, IMessage
    {
        public static M2C_SpellAdd Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_SpellAdd>(isFromPool);
        }

        [NinoMember(0)]
        public long UnitId { get; set; }
        [NinoMember(1)]
        public long SpellId { get; set; }
        [NinoMember(2)]
        public int SpellConfigId { get; set; }
        [NinoMember(3)]
        public List<long> TargetUnitId { get; set; } = new();

        [NinoMember(4)]
        public Unity.Mathematics.float3 TargetPosition { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_SpellRemove)]
    public partial class M2C_SpellRemove : MessageObject, IMessage
    {
        public static M2C_SpellRemove Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_SpellRemove>(isFromPool);
        }

        [NinoMember(0)]
        public long UnitId { get; set; }
        [NinoMember(1)]
        public long SpellId { get; set; }
        [NinoMember(2)]
        public int RemoveType { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.SpellTarget)]
    public partial class SpellTarget : MessageObject
    {
        public static SpellTarget Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<SpellTarget>(isFromPool);
        }

        [NinoMember(2)]
        public List<long> TargetUnitId { get; set; } = new();

        [NinoMember(3)]
        public Unity.Mathematics.float3 TargetPosition { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // Buff相关消息定义
    [NinoType(false)]
    [Message(Opcode.M2C_BuffAdd)]
    public partial class M2C_BuffAdd : MessageObject, ICurrentMessage
    {
        public static M2C_BuffAdd Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_BuffAdd>(isFromPool);
        }

        [NinoMember(0)]
        public long UnitId { get; set; }
        [NinoMember(1)]
        public long BuffId { get; set; }
        [NinoMember(2)]
        public int BuffConfigId { get; set; }
        [NinoMember(3)]
        public long CreateTime { get; set; }
        [NinoMember(4)]
        public int TickTime { get; set; }
        [NinoMember(5)]
        public long ExpireTime { get; set; }
        [NinoMember(6)]
        public long CasterId { get; set; }
        [NinoMember(7)]
        public int Stack { get; set; }
        [NinoMember(8)]
        public SpellTarget SpellTarget { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_BuffUpdate)]
    public partial class M2C_BuffUpdate : MessageObject, ICurrentMessage
    {
        public static M2C_BuffUpdate Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_BuffUpdate>(isFromPool);
        }

        [NinoMember(0)]
        public long UnitId { get; set; }
        [NinoMember(1)]
        public long BuffId { get; set; }
        [NinoMember(4)]
        public int TickTime { get; set; }
        [NinoMember(5)]
        public long ExpireTime { get; set; }
        [NinoMember(6)]
        public int Stack { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_BuffRemove)]
    public partial class M2C_BuffRemove : MessageObject, ICurrentMessage
    {
        public static M2C_BuffRemove Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_BuffRemove>(isFromPool);
        }

        [NinoMember(0)]
        public long UnitId { get; set; }
        [NinoMember(1)]
        public long BuffId { get; set; }
        [NinoMember(2)]
        public int BuffConfigId { get; set; }
        [NinoMember(3)]
        public int RemoveType { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // CD相关消息定义
    [NinoType(false)]
    [Message(Opcode.M2C_UpdateCD)]
    public partial class M2C_UpdateCD : MessageObject, ICurrentMessage
    {
        public static M2C_UpdateCD Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_UpdateCD>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public long UnitId { get; set; }
        /// <summary>
        /// 0表示公共CD
        /// </summary>
        [NinoMember(2)]
        public int SpellConfigId { get; set; }
        [NinoMember(3)]
        public long Time { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort C2M_SpellCast = 10201;
        public const ushort M2C_SpellAdd = 10202;
        public const ushort M2C_SpellRemove = 10203;
        public const ushort SpellTarget = 10204;
        public const ushort M2C_BuffAdd = 10205;
        public const ushort M2C_BuffUpdate = 10206;
        public const ushort M2C_BuffRemove = 10207;
        public const ushort M2C_UpdateCD = 10208;
    }
}