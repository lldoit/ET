using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    [NinoType(false)]
    [Message(Opcode.ServiceRegisterRequest)]
    [ResponseType(nameof(ServiceRegisterResponse))]
    public partial class ServiceRegisterRequest : MessageObject, IRequest
    {
        public static ServiceRegisterRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceRegisterRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public string SceneName { get; set; }
        [NinoMember(2)]
        public ActorId ActorId { get; set; }
        /// <summary>
        /// new()
        /// </summary>
        [NinoMember(3)]
        public StringKV Metadata { get; set; } = new();
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceRegisterResponse)]
    public partial class ServiceRegisterResponse : MessageObject, IResponse
    {
        public static ServiceRegisterResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceRegisterResponse>(isFromPool);
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
    [Message(Opcode.ServiceUnregisterRequest)]
    [ResponseType(nameof(ServiceUnregisterResponse))]
    public partial class ServiceUnregisterRequest : MessageObject, IRequest
    {
        public static ServiceUnregisterRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceUnregisterRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public string SceneName { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceUnregisterResponse)]
    public partial class ServiceUnregisterResponse : MessageObject, IResponse
    {
        public static ServiceUnregisterResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceUnregisterResponse>(isFromPool);
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
    [Message(Opcode.ServiceHeartbeatRequest)]
    [ResponseType(nameof(ServiceHeartbeatResponse))]
    public partial class ServiceHeartbeatRequest : MessageObject, IRequest
    {
        public static ServiceHeartbeatRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceHeartbeatRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public string SceneName { get; set; }
        [NinoMember(2)]
        public ActorId AgentActorId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceHeartbeatResponse)]
    public partial class ServiceHeartbeatResponse : MessageObject, IResponse
    {
        public static ServiceHeartbeatResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceHeartbeatResponse>(isFromPool);
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
    [Message(Opcode.ServiceAgentRegisterRequest)]
    [ResponseType(nameof(ServiceAgentRegisterResponse))]
    public partial class ServiceAgentRegisterRequest : MessageObject, IRequest
    {
        public static ServiceAgentRegisterRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceAgentRegisterRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public ActorId AgentActorId { get; set; }
        [NinoMember(2)]
        public List<ServiceInfoProto> LocalServices { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceAgentRegisterResponse)]
    public partial class ServiceAgentRegisterResponse : MessageObject, IResponse
    {
        public static ServiceAgentRegisterResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceAgentRegisterResponse>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        [NinoMember(3)]
        public List<ServiceInfoProto> Services { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceQueryRequest)]
    [ResponseType(nameof(ServiceQueryResponse))]
    public partial class ServiceQueryRequest : MessageObject, IRequest
    {
        public static ServiceQueryRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceQueryRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        /// <summary>
        /// new()
        /// </summary>
        [NinoMember(1)]
        public StringKV Filter { get; set; } = new();
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceQueryResponse)]
    public partial class ServiceQueryResponse : MessageObject, IResponse
    {
        public static ServiceQueryResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceQueryResponse>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        [NinoMember(3)]
        public List<ServiceInfoProto> Services { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceSubscribeRequest)]
    [ResponseType(nameof(ServiceSubscribeResponse))]
    public partial class ServiceSubscribeRequest : MessageObject, IRequest
    {
        public static ServiceSubscribeRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceSubscribeRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public string SceneName { get; set; }
        [NinoMember(2)]
        public string FilterName { get; set; }
        /// <summary>
        /// new()
        /// </summary>
        [NinoMember(3)]
        public StringKV FilterMetadata { get; set; } = new();
        [NinoMember(4)]
        public ActorId SubscriberActorId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceSubscribeResponse)]
    public partial class ServiceSubscribeResponse : MessageObject, IResponse
    {
        public static ServiceSubscribeResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceSubscribeResponse>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        [NinoMember(3)]
        public List<ServiceInfoProto> Services { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceUnsubscribeRequest)]
    [ResponseType(nameof(ServiceUnsubscribeResponse))]
    public partial class ServiceUnsubscribeRequest : MessageObject, IRequest
    {
        public static ServiceUnsubscribeRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceUnsubscribeRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public string SceneName { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.ServiceUnsubscribeResponse)]
    public partial class ServiceUnsubscribeResponse : MessageObject, IResponse
    {
        public static ServiceUnsubscribeResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceUnsubscribeResponse>(isFromPool);
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

    // 服务变更通知消息
    [NinoType(false)]
    [Message(Opcode.ServiceChangeNotification)]
    public partial class ServiceChangeNotification : MessageObject, IMessage
    {
        public static ServiceChangeNotification Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceChangeNotification>(isFromPool);
        }

        /// <summary>
        /// 1=添加, 2=删除, 3=主机切换
        /// </summary>
        [NinoMember(0)]
        public int ChangeType { get; set; }
        [NinoMember(1)]
        public long Epoch { get; set; }
        [NinoMember(2)]
        public ActorId MasterActorId { get; set; }
        [NinoMember(3)]
        public List<ServiceInfoProto> ServiceInfo { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // Proxy销毁时单向通知Agent执行注销
    [NinoType(false)]
    [Message(Opcode.ServiceProxyDestroyUnregisterMessage)]
    public partial class ServiceProxyDestroyUnregisterMessage : MessageObject, IMessage
    {
        public static ServiceProxyDestroyUnregisterMessage Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceProxyDestroyUnregisterMessage>(isFromPool);
        }

        [NinoMember(0)]
        public string SceneName { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // 服务信息Proto定义
    [NinoType(false)]
    [Message(Opcode.ServiceInfoProto)]
    public partial class ServiceInfoProto : MessageObject
    {
        public static ServiceInfoProto Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ServiceInfoProto>(isFromPool);
        }

        [NinoMember(0)]
        public string SceneName { get; set; }
        [NinoMember(2)]
        public ActorId ActorId { get; set; }
        /// <summary>
        /// new()
        /// </summary>
        [NinoMember(4)]
        public StringKV Metadata { get; set; } = new();
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort ServiceRegisterRequest = 20501;
        public const ushort ServiceRegisterResponse = 20502;
        public const ushort ServiceUnregisterRequest = 20503;
        public const ushort ServiceUnregisterResponse = 20504;
        public const ushort ServiceHeartbeatRequest = 20505;
        public const ushort ServiceHeartbeatResponse = 20506;
        public const ushort ServiceAgentRegisterRequest = 20507;
        public const ushort ServiceAgentRegisterResponse = 20508;
        public const ushort ServiceQueryRequest = 20509;
        public const ushort ServiceQueryResponse = 20510;
        public const ushort ServiceSubscribeRequest = 20511;
        public const ushort ServiceSubscribeResponse = 20512;
        public const ushort ServiceUnsubscribeRequest = 20513;
        public const ushort ServiceUnsubscribeResponse = 20514;
        public const ushort ServiceChangeNotification = 20515;
        public const ushort ServiceProxyDestroyUnregisterMessage = 20516;
        public const ushort ServiceInfoProto = 20517;
    }
}