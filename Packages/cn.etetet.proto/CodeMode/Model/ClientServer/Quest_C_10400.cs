using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    // 接任务
    [NinoType(false)]
    [Message(Opcode.C2M_AcceptQuest)]
    [ResponseType(nameof(M2C_AcceptQuest))]
    public partial class C2M_AcceptQuest : MessageObject, ILocationRequest
    {
        public static C2M_AcceptQuest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_AcceptQuest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public long QuestId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_AcceptQuest)]
    public partial class M2C_AcceptQuest : MessageObject, ILocationResponse
    {
        public static M2C_AcceptQuest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_AcceptQuest>(isFromPool);
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

    // 交任务
    [NinoType(false)]
    [Message(Opcode.C2M_SubmitQuest)]
    [ResponseType(nameof(M2C_SubmitQuest))]
    public partial class C2M_SubmitQuest : MessageObject, ILocationRequest
    {
        public static C2M_SubmitQuest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_SubmitQuest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public long QuestId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_SubmitQuest)]
    public partial class M2C_SubmitQuest : MessageObject, ILocationResponse
    {
        public static M2C_SubmitQuest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_SubmitQuest>(isFromPool);
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
    [Message(Opcode.QuestObjectiveInfo)]
    public partial class QuestObjectiveInfo : MessageObject
    {
        public static QuestObjectiveInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<QuestObjectiveInfo>(isFromPool);
        }

        [NinoMember(0)]
        public long QuestObjectiveId { get; set; }
        [NinoMember(1)]
        public int Count { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // 更新任务信息
    [NinoType(false)]
    [Message(Opcode.M2C_CreateQuest)]
    public partial class M2C_CreateQuest : MessageObject, IMessage
    {
        public static M2C_CreateQuest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_CreateQuest>(isFromPool);
        }

        [NinoMember(0)]
        public long QuestId { get; set; }
        [NinoMember(1)]
        public List<QuestObjectiveInfo> QuestObjective { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // 更新任务目标
    [NinoType(false)]
    [Message(Opcode.M2C_UpdateQuestObjective)]
    public partial class M2C_UpdateQuestObjective : MessageObject, IMessage
    {
        public static M2C_UpdateQuestObjective Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_UpdateQuestObjective>(isFromPool);
        }

        /// <summary>
        /// 任务Id
        /// </summary>
        [NinoMember(0)]
        public long QuestId { get; set; }
        [NinoMember(1)]
        public long QuestObjectiveId { get; set; }
        [NinoMember(2)]
        public int Count { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // 更新任务信息
    [NinoType(false)]
    [Message(Opcode.M2C_UpdateQuest)]
    public partial class M2C_UpdateQuest : MessageObject, IMessage
    {
        public static M2C_UpdateQuest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_UpdateQuest>(isFromPool);
        }

        [NinoMember(0)]
        public long QuestId { get; set; }
        /// <summary>
        /// 1:进行中, 2:已完成
        /// </summary>
        [NinoMember(1)]
        public int State { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // 同步任务数据请求
    [NinoType(false)]
    [Message(Opcode.C2M_SyncQuestData)]
    [ResponseType(nameof(M2C_SyncQuestData))]
    public partial class C2M_SyncQuestData : MessageObject, ILocationRequest
    {
        public static C2M_SyncQuestData Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_SyncQuestData>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.QuestInfo)]
    public partial class QuestInfo : MessageObject
    {
        public static QuestInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<QuestInfo>(isFromPool);
        }

        [NinoMember(0)]
        public long QuestId { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        [NinoMember(1)]
        public int Status { get; set; }
        [NinoMember(2)]
        public List<QuestObjectiveInfo> Objectives { get; set; } = new();

        /// <summary>
        /// 接取时间
        /// </summary>
        [NinoMember(3)]
        public long AcceptTime { get; set; }
        /// <summary>
        /// 完成时间
        /// </summary>
        [NinoMember(4)]
        public long CompleteTime { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_SyncQuestData)]
    public partial class M2C_SyncQuestData : MessageObject, ILocationResponse
    {
        public static M2C_SyncQuestData Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_SyncQuestData>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        [NinoMember(3)]
        public List<QuestInfo> QuestList { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // 放弃任务
    [NinoType(false)]
    [Message(Opcode.C2M_AbandonQuest)]
    [ResponseType(nameof(M2C_AbandonQuest))]
    public partial class C2M_AbandonQuest : MessageObject, ILocationRequest
    {
        public static C2M_AbandonQuest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_AbandonQuest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public long QuestId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_AbandonQuest)]
    public partial class M2C_AbandonQuest : MessageObject, ILocationResponse
    {
        public static M2C_AbandonQuest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_AbandonQuest>(isFromPool);
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

    // 查询可接取任务
    [NinoType(false)]
    [Message(Opcode.C2M_QueryAvailableQuests)]
    [ResponseType(nameof(M2C_QueryAvailableQuests))]
    public partial class C2M_QueryAvailableQuests : MessageObject, ILocationRequest
    {
        public static C2M_QueryAvailableQuests Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_QueryAvailableQuests>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        /// <summary>
        /// NPC ID，为0时查询所有可接取任务
        /// </summary>
        [NinoMember(1)]
        public long NPCId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.AvailableQuestInfo)]
    public partial class AvailableQuestInfo : MessageObject
    {
        public static AvailableQuestInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<AvailableQuestInfo>(isFromPool);
        }

        [NinoMember(0)]
        public long QuestId { get; set; }
        [NinoMember(1)]
        public string QuestName { get; set; }
        [NinoMember(2)]
        public string QuestDesc { get; set; }
        [NinoMember(3)]
        public int QuestType { get; set; }
        [NinoMember(4)]
        public int RewardExp { get; set; }
        [NinoMember(5)]
        public int RewardGold { get; set; }
        /// <summary>
        /// 奖励道具ID列表
        /// </summary>
        [NinoMember(6)]
        public List<int> RewardItems { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_QueryAvailableQuests)]
    public partial class M2C_QueryAvailableQuests : MessageObject, ILocationResponse
    {
        public static M2C_QueryAvailableQuests Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_QueryAvailableQuests>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        [NinoMember(3)]
        public List<AvailableQuestInfo> AvailableQuests { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    // 获取任务详情
    [NinoType(false)]
    [Message(Opcode.C2M_GetQuestDetail)]
    [ResponseType(nameof(M2C_GetQuestDetail))]
    public partial class C2M_GetQuestDetail : MessageObject, ILocationRequest
    {
        public static C2M_GetQuestDetail Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_GetQuestDetail>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public long QuestId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.QuestDetailInfo)]
    public partial class QuestDetailInfo : MessageObject
    {
        public static QuestDetailInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<QuestDetailInfo>(isFromPool);
        }

        [NinoMember(0)]
        public long QuestId { get; set; }
        [NinoMember(1)]
        public string QuestName { get; set; }
        [NinoMember(2)]
        public string QuestDesc { get; set; }
        /// <summary>
        /// 任务背景故事
        /// </summary>
        [NinoMember(3)]
        public string QuestStory { get; set; }
        [NinoMember(4)]
        public int QuestType { get; set; }
        [NinoMember(5)]
        public long AcceptNPC { get; set; }
        [NinoMember(6)]
        public long SubmitNPC { get; set; }
        [NinoMember(7)]
        public int RewardExp { get; set; }
        [NinoMember(8)]
        public int RewardGold { get; set; }
        [NinoMember(9)]
        public List<int> RewardItems { get; set; } = new();

        /// <summary>
        /// 前置任务列表
        /// </summary>
        [NinoMember(10)]
        public List<int> PreQuests { get; set; } = new();

        /// <summary>
        /// 最低等级要求
        /// </summary>
        [NinoMember(11)]
        public int MinLevel { get; set; }
        /// <summary>
        /// 最高等级限制
        /// </summary>
        [NinoMember(12)]
        public int MaxLevel { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_GetQuestDetail)]
    public partial class M2C_GetQuestDetail : MessageObject, ILocationResponse
    {
        public static M2C_GetQuestDetail Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_GetQuestDetail>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        [NinoMember(3)]
        public QuestDetailInfo QuestDetail { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.Show_QuestInfo)]
    public partial class Show_QuestInfo : MessageObject
    {
        public static Show_QuestInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Show_QuestInfo>(isFromPool);
        }

        [NinoMember(0)]
        public int QuestId { get; set; }
        /// <summary>
        /// 0: 可接, 1: 已接, 2: 可提交，3: 已提交
        /// </summary>
        [NinoMember(1)]
        public int Status { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.C2M_ClickUnitRequest)]
    [ResponseType(nameof(M2C_ClickUnitResponse))]
    public partial class C2M_ClickUnitRequest : MessageObject, ILocationRequest
    {
        public static C2M_ClickUnitRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_ClickUnitRequest>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public long UnitId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_ClickUnitResponse)]
    public partial class M2C_ClickUnitResponse : MessageObject, ILocationResponse
    {
        public static M2C_ClickUnitResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_ClickUnitResponse>(isFromPool);
        }

        [NinoMember(0)]
        public int RpcId { get; set; }
        [NinoMember(1)]
        public int Error { get; set; }
        [NinoMember(2)]
        public string Message { get; set; }
        /// <summary>
        /// 任务信息
        /// </summary>
        [NinoMember(4)]
        public List<Show_QuestInfo> questInfo { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort C2M_AcceptQuest = 10401;
        public const ushort M2C_AcceptQuest = 10402;
        public const ushort C2M_SubmitQuest = 10403;
        public const ushort M2C_SubmitQuest = 10404;
        public const ushort QuestObjectiveInfo = 10405;
        public const ushort M2C_CreateQuest = 10406;
        public const ushort M2C_UpdateQuestObjective = 10407;
        public const ushort M2C_UpdateQuest = 10408;
        public const ushort C2M_SyncQuestData = 10409;
        public const ushort QuestInfo = 10410;
        public const ushort M2C_SyncQuestData = 10411;
        public const ushort C2M_AbandonQuest = 10412;
        public const ushort M2C_AbandonQuest = 10413;
        public const ushort C2M_QueryAvailableQuests = 10414;
        public const ushort AvailableQuestInfo = 10415;
        public const ushort M2C_QueryAvailableQuests = 10416;
        public const ushort C2M_GetQuestDetail = 10417;
        public const ushort QuestDetailInfo = 10418;
        public const ushort M2C_GetQuestDetail = 10419;
        public const ushort Show_QuestInfo = 10420;
        public const ushort C2M_ClickUnitRequest = 10421;
        public const ushort M2C_ClickUnitResponse = 10422;
    }
}