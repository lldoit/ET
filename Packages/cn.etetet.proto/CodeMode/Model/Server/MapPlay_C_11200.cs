using Nino.Core;
using System.Collections.Generic;

namespace ET
{
    [NinoType(false)]
    [Message(Opcode.PetInfo)]
    public partial class PetInfo : MessageObject
    {
        public static PetInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<PetInfo>(isFromPool);
        }

        [NinoMember(0)]
        public long OwnerId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.UnitInfo)]
    public partial class UnitInfo : MessageObject
    {
        public static UnitInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<UnitInfo>(isFromPool);
        }

        [NinoMember(0)]
        public long UnitId { get; set; }
        [NinoMember(1)]
        public int ConfigId { get; set; }
        [NinoMember(2)]
        public int Type { get; set; }
        [NinoMember(3)]
        public Unity.Mathematics.float3 Position { get; set; }
        [NinoMember(4)]
        public Unity.Mathematics.float3 Forward { get; set; }
        [MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
        [NinoMember(5)]
        public Dictionary<int, long> KV { get; set; } = new();
        [NinoMember(6)]
        public MoveInfo MoveInfo { get; set; }
        [NinoMember(7)]
        public PetInfo PetInfo { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_CreateUnits)]
    public partial class M2C_CreateUnits : MessageObject, ICurrentMessage
    {
        public static M2C_CreateUnits Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_CreateUnits>(isFromPool);
        }

        [NinoMember(0)]
        public List<UnitInfo> Units { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_CreateMyUnit)]
    public partial class M2C_CreateMyUnit : MessageObject, ICurrentMessage
    {
        public static M2C_CreateMyUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_CreateMyUnit>(isFromPool);
        }

        [NinoMember(0)]
        public UnitInfo Unit { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_StartSceneChange)]
    public partial class M2C_StartSceneChange : MessageObject, IMessage
    {
        public static M2C_StartSceneChange Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_StartSceneChange>(isFromPool);
        }

        [NinoMember(0)]
        public long SceneId { get; set; }
        [NinoMember(1)]
        public string SceneName { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_RemoveUnits)]
    public partial class M2C_RemoveUnits : MessageObject, ICurrentMessage
    {
        public static M2C_RemoveUnits Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_RemoveUnits>(isFromPool);
        }

        [NinoMember(0)]
        public List<long> Units { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_Error)]
    public partial class M2C_Error : MessageObject, IMessage
    {
        public static M2C_Error Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_Error>(isFromPool);
        }

        [NinoMember(0)]
        public int Error { get; set; }
        [NinoMember(1)]
        public List<string> Values { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.M2C_NumericChange)]
    public partial class M2C_NumericChange : MessageObject, ICurrentMessage
    {
        public static M2C_NumericChange Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_NumericChange>(isFromPool);
        }

        [NinoMember(0)]
        public long UnitId { get; set; }
        [NinoMember(1)]
        public int NumericType { get; set; }
        [NinoMember(2)]
        public long Value { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [NinoType(false)]
    [Message(Opcode.C2M_PetAttack)]
    public partial class C2M_PetAttack : MessageObject, ILocationMessage
    {
        public static C2M_PetAttack Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_PetAttack>(isFromPool);
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

    public static partial class Opcode
    {
        public const ushort PetInfo = 11201;
        public const ushort UnitInfo = 11202;
        public const ushort M2C_CreateUnits = 11203;
        public const ushort M2C_CreateMyUnit = 11204;
        public const ushort M2C_StartSceneChange = 11205;
        public const ushort M2C_RemoveUnits = 11206;
        public const ushort M2C_Error = 11207;
        public const ushort M2C_NumericChange = 11208;
        public const ushort C2M_PetAttack = 11209;
    }
}