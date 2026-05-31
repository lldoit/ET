using System;
using System.IO;
using MessagePack;

namespace ET
{
    public sealed class MessagePackExternalMessageCodec: IExternalMessageCodec
    {
        public byte[] Serialize<T>(T message)
        {
            return MessagePackSerializer.Serialize(message);
        }

        public void Serialize<T>(T message, MemoryBuffer buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            MessagePackSerializer.Serialize((Stream)buffer, message);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }

    public sealed class MessagePackNetworkMessageCodec: INetworkMessageCodec
    {
        public static readonly MessagePackNetworkMessageCodec Instance = new();

        private MessagePackNetworkMessageCodec()
        {
        }

        public (ushort Opcode, MemoryBuffer MemoryBuffer) ToMemoryBuffer(AService service, FiberInstanceId fiberInstanceId, object message)
        {
            MemoryBuffer memoryBuffer = service.Fetch();
            ushort opcode = 0;
            switch (service.ServiceType)
            {
                case ServiceType.Inner:
                {
                    opcode = this.MessageToStream(memoryBuffer, message, Packet.FiberInstanceIdLength);
                    memoryBuffer.GetBuffer().WriteTo(0, fiberInstanceId);
                    break;
                }
                case ServiceType.Outer:
                {
                    opcode = this.MessageToStream(memoryBuffer, message);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(service.ServiceType), service.ServiceType, "Unsupported service type.");
                }
            }

            return (opcode, memoryBuffer);
        }

        public (FiberInstanceId FiberInstanceId, object Message) ToMessage(AService service, MemoryBuffer memoryBuffer)
        {
            switch (service.ServiceType)
            {
                case ServiceType.Outer:
                {
                    return this.ToMessagePackMessage(memoryBuffer);
                }
                case ServiceType.Inner:
                {
                    byte[] buffer = memoryBuffer.GetBuffer();
                    FiberInstanceId fiberInstanceId = default;
                    fiberInstanceId.Fiber = BitConverter.ToInt64(buffer, Packet.FiberInstanceIdIndex);
                    fiberInstanceId.InstanceId = BitConverter.ToInt64(buffer, Packet.FiberInstanceIdIndex + 8);
                    return this.ToMessagePackMessage(memoryBuffer, Packet.FiberInstanceIdLength, fiberInstanceId);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(service.ServiceType), service.ServiceType, "Unsupported service type.");
                }
            }
        }

        public ushort ToMessagePackBody(MemoryBuffer memoryBuffer, object message)
        {
            return this.MessageToStream(memoryBuffer, message);
        }

        public (FiberInstanceId FiberInstanceId, object Message) ToMessagePackMessage(MemoryBuffer memoryBuffer)
        {
            return this.ToMessagePackMessage(memoryBuffer, 0, default);
        }

        private (FiberInstanceId FiberInstanceId, object Message) ToMessagePackMessage(MemoryBuffer memoryBuffer, int headOffset, FiberInstanceId fiberInstanceId)
        {
            ushort opcode = BitConverter.ToUInt16(memoryBuffer.GetBuffer(), headOffset);
            Type type = OpcodeType.Instance.GetType(opcode);
            memoryBuffer.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            object message = MessagePackSerializer.Deserialize(type, memoryBuffer);
            return (fiberInstanceId, message);
        }

        private ushort MessageToStream(MemoryBuffer memoryBuffer, object message, int headOffset = 0)
        {
            ushort opcode = OpcodeType.Instance.GetOpcode(message.GetType());
            memoryBuffer.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            memoryBuffer.SetLength(headOffset + Packet.OpcodeLength);
            memoryBuffer.GetBuffer().WriteTo(headOffset, opcode);
            MessagePackSerializer.Serialize(message.GetType(), (Stream)memoryBuffer, message);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            return opcode;
        }
    }
}
