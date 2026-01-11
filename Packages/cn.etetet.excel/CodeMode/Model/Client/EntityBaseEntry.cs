using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class EntityBaseEntryCategory : Singleton<EntityBaseEntryCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, EntityBaseEntry> dict = new();
		
        public void Merge(object o)
        {
            EntityBaseEntryCategory s = o as EntityBaseEntryCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public EntityBaseEntry Get(int id)
        {
            this.dict.TryGetValue(id, out EntityBaseEntry item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (EntityBaseEntry)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, EntityBaseEntry> GetAll()
        {
            return this.dict;
        }

        public EntityBaseEntry GetOne()
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

	public partial class EntityBaseEntry: ProtoObject, IConfig
	{
		/// <summary>编号</summary>
		public int Id { get; set; }
		/// <summary></summary>
		public string Name { get; set; }
		/// <summary>关联属性表Id</summary>
		public int EntityAttEntry { get; set; }
		/// <summary>等级</summary>
		public int Level { get; set; }
		/// <summary>星级</summary>
		public int Star { get; set; }
		/// <summary>评分</summary>
		public int Score { get; set; }
		/// <summary>兵种id</summary>
		public int ClassId { get; set; }
		/// <summary>初始被动buff和技能</summary>
		public int[] InitAurasAndSpell { get; set; }
		/// <summary>普通攻击</summary>
		public int MeleeSpell { get; set; }
		/// <summary>技能施放顺序</summary>
		public int SpellOrder { get; set; }
		/// <summary>小技能</summary>
		public int NormalSpell { get; set; }
		/// <summary>大技能</summary>
		public int SpecialSpell { get; set; }
		/// <summary>关联模型表Id</summary>
		public int ModelEntry { get; set; }
		/// <summary>模型缩放</summary>
		public float Scale { get; set; }

	}
}
