using System;

namespace ET
{
    public interface IExternalMessageCodec
    {
        byte[] Serialize<T>(T message);

        void Serialize<T>(T message, MemoryBuffer buffer);

        T Deserialize<T>(ReadOnlyMemory<byte> bytes);
    }
}
