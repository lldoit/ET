using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    [NinoType(false)]
    [Message(Opcode.RouterSync)]
    public partial class RouterSync : MessageObject
    {
        public static RouterSync Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<RouterSync>(isFromPool);
        }

        [NinoMember(0)]
        public uint ConnectId { get; set; }
        [NinoMember(1)]
        public string Address { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.C2M_Reload)]
    [ResponseType(nameof(M2C_Reload))]
    public partial class C2M_Reload : MessageObject, ISessionRequest
    {
        public static C2M_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_Reload>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public string Account { get; set; }
        [NinoMember(2)]
        public string Password { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_Reload)]
    public partial class M2C_Reload : MessageObject, ISessionResponse
    {
        public static M2C_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_Reload>(isFromPool);
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
    [Message(Opcode.G2C_TestHotfixMessage)]
    public partial class G2C_TestHotfixMessage : MessageObject, ISessionMessage
    {
        public static G2C_TestHotfixMessage Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2C_TestHotfixMessage>(isFromPool);
        }

        [NinoMember(0)]
        public string Info { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.C2G_Benchmark)]
    [ResponseType(nameof(G2C_Benchmark))]
    public partial class C2G_Benchmark : MessageObject, ISessionRequest
    {
        public static C2G_Benchmark Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2G_Benchmark>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.G2C_Benchmark)]
    public partial class G2C_Benchmark : MessageObject, ISessionResponse
    {
        public static G2C_Benchmark Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2C_Benchmark>(isFromPool);
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
        public const ushort RouterSync = 10701;
        public const ushort C2M_Reload = 10702;
        public const ushort M2C_Reload = 10703;
        public const ushort G2C_TestHotfixMessage = 10704;
        public const ushort C2G_Benchmark = 10705;
        public const ushort G2C_Benchmark = 10706;
    }
}