using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class DREntitySpellEntryCategory : Singleton<DREntitySpellEntryCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, DREntitySpellEntry> dict = new();
		
        public void Merge(object o)
        {
            DREntitySpellEntryCategory s = o as DREntitySpellEntryCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public DREntitySpellEntry Get(int id)
        {
            this.dict.TryGetValue(id, out DREntitySpellEntry item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (DREntitySpellEntry)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, DREntitySpellEntry> GetAll()
        {
            return this.dict;
        }

        public DREntitySpellEntry GetOne()
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

	public partial class DREntitySpellEntry: ProtoObject, IConfig
	{
		/// <summary>编号</summary>
		public int Id { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>技能类型</summary>
		public int SpellType { get; set; }
		/// <summary>技能消耗类型</summary>
		public int CostType { get; set; }
		/// <summary>消耗值</summary>
		public int CostValue { get; set; }
		/// <summary>回复觉醒点百分比( 0 - 100)</summary>
		public int AddAwakePointPct { get; set; }
		/// <summary>施放者状态限制</summary>
		public uint CasterStateLimit { get; set; }
		/// <summary>目标状态限制</summary>
		public uint TargetStateLimit { get; set; }
		/// <summary>初始CD</summary>
		public int InitCD { get; set; }
		/// <summary>技能CD(回合数)</summary>
		public int CD { get; set; }
		/// <summary>技能攻击范围显示</summary>
		public int RangeDisplayType { get; set; }
		/// <summary>目标选取方式</summary>
		public int SelectType { get; set; }
		/// <summary>最大目标数量</summary>
		public int MaxTargetNum { get; set; }
		/// <summary>对目标效果块</summary>
		public int[] EffectBlocks { get; set; }
		/// <summary>生成AuraID组 Tips用</summary>
		public int[] Auras { get; set; }
		/// <summary>伤害类型</summary>
		public int DamageSchool { get; set; }
		/// <summary>子技能</summary>
		public int SubSpell { get; set; }
		/// <summary>图标</summary>
		public string Icon { get; set; }
		/// <summary>描述</summary>
		public string Desc { get; set; }
		/// <summary>该技能是否必命中</summary>
		public bool HitCertainly { get; set; }
		/// <summary>命中是否计算等级压制</summary>
		public bool LevelSuppress { get; set; }
		/// <summary>技能不被格挡</summary>
		public bool NotBlock { get; set; }
		/// <summary>技能不暴击</summary>
		public bool NotCrit { get; set; }
		/// <summary>是否能被援护</summary>
		public bool CanIntervene { get; set; }
		/// <summary>是否能被反击</summary>
		public bool CanStrikeBack { get; set; }
		/// <summary>该技能不会触发任何触发器</summary>
		public bool NotTrigger { get; set; }
		/// <summary>资源路径</summary>
		public string AssetPath { get; set; }
		/// <summary>被动技能施放掩码</summary>
		public int PassiveType { get; set; }
		/// <summary>AI技能选择条件</summary>
		public int AISelectSpell { get; set; }
		/// <summary>AI目标选择条件</summary>
		public int AISelectTarget { get; set; }
		/// <summary>默认AI目标</summary>
		public int DefaultAITarget { get; set; }
		/// <summary>额外类型</summary>
		public int AdditionalType { get; set; }
		/// <summary>吟唱回合</summary>
		public int DelayRound { get; set; }
		/// <summary>动作时间</summary>
		public float ActionTime { get; set; }
		/// <summary>触发类型</summary>
		public int[] TriggerType { get; set; }
		/// <summary>合击类型</summary>
		public int JoinAttack { get; set; }
		/// <summary>额外触发类型</summary>
		public int AddtionTriggerType { get; set; }
		/// <summary>合击触发类型</summary>
		public int JoinAttackTriggerType { get; set; }
		/// <summary>法宝效果资源路径</summary>
		public string ArtifactPath { get; set; }
		/// <summary>场景ID</summary>
		public int[] SceneIds { get; set; }

	}
}
