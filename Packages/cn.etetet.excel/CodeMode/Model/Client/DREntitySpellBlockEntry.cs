using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class DREntitySpellBlockEntryCategory : Singleton<DREntitySpellBlockEntryCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, DREntitySpellBlockEntry> dict = new();
		
        public void Merge(object o)
        {
            DREntitySpellBlockEntryCategory s = o as DREntitySpellBlockEntryCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public DREntitySpellBlockEntry Get(int id)
        {
            this.dict.TryGetValue(id, out DREntitySpellBlockEntry item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (DREntitySpellBlockEntry)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, DREntitySpellBlockEntry> GetAll()
        {
            return this.dict;
        }

        public DREntitySpellBlockEntry GetOne()
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

	public partial class DREntitySpellBlockEntry: ProtoObject, IConfig
	{
		/// <summary>编号</summary>
		public int Id { get; set; }
		/// <summary>效果类型</summary>
		public int Effect { get; set; }
		/// <summary>效果参数</summary>
		public int[] Param { get; set; }
		/// <summary>效果作用概率(默认不填概率为100%)</summary>
		public int Probability { get; set; }
		/// <summary>效果限制条件</summary>
		public int[] Condition { get; set; }
		/// <summary>作用条件结果</summary>
		public bool ConditionResult { get; set; }
		/// <summary>条件作用目标</summary>
		public int ConditionTarget { get; set; }
		/// <summary>效果限制条件</summary>
		public int[] ConditionEx { get; set; }
		/// <summary>作用条件结果</summary>
		public bool ConditionResultEx { get; set; }
		/// <summary>条件作用目标</summary>
		public int ConditionTargetEx { get; set; }

	}
}
