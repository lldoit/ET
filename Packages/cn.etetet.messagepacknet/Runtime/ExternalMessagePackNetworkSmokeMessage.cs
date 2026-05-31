using MessagePack;

namespace ET
{
    [Message(19999)]
    [MessagePackObject]
    public sealed class ExternalMessagePackNetworkSmokeMessage: MessageObject
    {
        [Key(0)]
        public int Id { get; set; }

        [Key(1)]
        public string Text { get; set; }
    }
}
