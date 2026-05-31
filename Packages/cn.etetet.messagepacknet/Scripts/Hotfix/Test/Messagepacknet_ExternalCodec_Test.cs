using System;

namespace ET.Test
{
    public class Messagepacknet_ExternalCodec_Test: ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            MessagePackExternalMessageCodec codec = new();
            ExternalMessagePackNetworkSmokeMessage message = new()
            {
                Id = 1001,
                Text = "messagepack"
            };

            byte[] bytes = codec.Serialize(message);
            ExternalMessagePackNetworkSmokeMessage fromBytes = codec.Deserialize<ExternalMessagePackNetworkSmokeMessage>(bytes);
            if (fromBytes.Id != message.Id || fromBytes.Text != message.Text)
            {
                throw new Exception("MessagePack byte[] roundtrip failed.");
            }

            using MemoryBuffer buffer = new();
            codec.Serialize(message, buffer);
            ReadOnlyMemory<byte> memory = buffer.GetBuffer().AsMemory(0, (int)buffer.Length);
            ExternalMessagePackNetworkSmokeMessage fromBuffer = codec.Deserialize<ExternalMessagePackNetworkSmokeMessage>(memory);
            if (fromBuffer.Id != message.Id || fromBuffer.Text != message.Text)
            {
                throw new Exception("MessagePack MemoryBuffer roundtrip failed.");
            }

            ExternalMessagePackNetworkSmokeMessage networkMessage = new()
            {
                Id = 2002,
                Text = "messagepack-network"
            };
            using MemoryBuffer networkBuffer = new();
            MessagePackNetworkMessageCodec.Instance.ToMessagePackBody(networkBuffer, networkMessage);
            (FiberInstanceId _, object decodedMessage) = MessagePackNetworkMessageCodec.Instance.ToMessagePackMessage(networkBuffer);
            ExternalMessagePackNetworkSmokeMessage decodedNetworkMessage = decodedMessage as ExternalMessagePackNetworkSmokeMessage;
            if (decodedNetworkMessage == null ||
                decodedNetworkMessage.Id != networkMessage.Id ||
                decodedNetworkMessage.Text != networkMessage.Text)
            {
                throw new Exception("MessagePack network codec roundtrip failed.");
            }

            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }
    }
}
