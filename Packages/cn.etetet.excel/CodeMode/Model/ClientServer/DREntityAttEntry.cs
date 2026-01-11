using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class DREntityAttEntryCategory : Singleton<DREntityAttEntryCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, DREntityAttEntry> dict = new();
		
        public void Merge(object o)
        {
            DREntityAttEntryCategory s = o as DREntityAttEntryCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public DREntityAttEntry Get(int id)
        {
            this.dict.TryGetValue(id, out DREntityAttEntry item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (DREntityAttEntry)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, DREntityAttEntry> GetAll()
        {
            return this.dict;
        }

        public DREntityAttEntry GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
    }

	public partial class DREntityAttEntry: ProtoObject, IConfig
	{
		/// <summary>编号</summary>
		public int Id { get; set; }
		/// <summary>物理攻击</summary>
		public int MeleeAttack { get; set; }
		/// <summary>法术攻击</summary>
		public int MagicAttack { get; set; }
		/// <summary>物理防御</summary>
		public int MeleeDefence { get; set; }
		/// <summary>法术防御</summary>
		public int MagicDefence { get; set; }
		/// <summary>生命值上限</summary>
		public int MaxHP { get; set; }
		/// <summary>速度</summary>
		public int Speed { get; set; }
		/// <summary>暴击</summary>
		public int Crit { get; set; }
		/// <summary>韧性</summary>
		public int Resilience { get; set; }
		/// <summary>格挡</summary>
		public int Block { get; set; }
		/// <summary>破击</summary>
		public int Broken { get; set; }
		/// <summary>血条数量</summary>
		public int NumLives { get; set; }
		/// <summary>闪避</summary>
		public int Dodge { get; set; }
		/// <summary>攻击百分比伤害改变</summary>
		public int PctDmgInc { get; set; }
		/// <summary>被攻击百分比伤害改变</summary>
		public int PctDmgDec { get; set; }
		/// <summary>治疗百分比加成</summary>
		public int PctHealInc { get; set; }
		/// <summary>反击率</summary>
		public int StrikeBack { get; set; }
		/// <summary>合击率</summary>
		public int JoinAttack { get; set; }
		/// <summary>水伤害减免</summary>
		public int SchoolTaken1 { get; set; }
		/// <summary>火伤害减免</summary>
		public int SchoolTaken2 { get; set; }
		/// <summary>毒伤害减免</summary>
		public int SchoolTaken3 { get; set; }
		/// <summary>风伤害减免</summary>
		public int SchoolTaken4 { get; set; }
		/// <summary>雷伤害减免</summary>
		public int SchoolTaken5 { get; set; }
		/// <summary>无属性伤害减免</summary>
		public int SchoolTaken6 { get; set; }
		/// <summary>武力</summary>
		public int HeroAtts0 { get; set; }
		/// <summary>智力</summary>
		public int HeroAtts1 { get; set; }
		/// <summary>统率</summary>
		public int HeroAtts2 { get; set; }
		/// <summary>政治</summary>
		public int HeroAtts3 { get; set; }

	}
}
