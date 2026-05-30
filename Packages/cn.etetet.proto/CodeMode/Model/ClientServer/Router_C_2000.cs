using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    [NinoType(false)]
    [Message(Opcode.HttpGetRouterResponse)]
    public partial class HttpGetRouterResponse : MessageObject
    {
        public static HttpGetRouterResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<HttpGetRouterResponse>(isFromPool);
        }

        [NinoMember(0)]
        public List<string> Realms { get; set; } = new();

        [NinoMember(1)]
        public List<string> Routers { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort HttpGetRouterResponse = 2001;
    }
}