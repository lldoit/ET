using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    [NinoType(false)]
    [Message(Opcode.G2Map_Logout)]
    [ResponseType(nameof(Map2G_Logout))]
    public partial class G2Map_Logout : MessageObject, ILocationRequest
    {
        public static G2Map_Logout Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2Map_Logout>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.Map2G_Logout)]
    public partial class Map2G_Logout : MessageObject, ILocationResponse
    {
        public static Map2G_Logout Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Map2G_Logout>(isFromPool);
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
        public const ushort G2Map_Logout = 21201;
        public const ushort Map2G_Logout = 21202;
    }
}