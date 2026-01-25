using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 英雄实体 - 只包含数据，不包含方法
    /// 所有逻辑请使用 EntityHeroSystem 扩展方法
    /// </summary>
    [ChildOf(typeof(EntityGroup))]
    public class EntityHero : Entity, IAwake<int>, IDestroy
    {
        /// <summary>
        /// 英雄唯一运行时Id (全局自增)
        /// </summary>
        public int HeroId;


        /// <summary>
        /// 静态属性
        /// </summary>
        public DREntityBaseEntry Entry;

        /// <summary>
        /// 等级
        /// </summary>
        public int Level;

        /// <summary>
        /// 觉醒等级
        /// </summary>
        public int WakeUpLv;

        /// <summary>
        /// 评分
        /// </summary>
        public int Score;

        /// <summary>
        /// 站位 (1到15)
        /// </summary>
        public int Station;

        /// <summary>
        /// 删除标记
        /// </summary>
        public bool Delete;

        /// <summary>
        /// 能量值
        /// </summary>
        public int Energy;

        /// <summary>
        /// 满能量值
        /// </summary>
        public int MaxEnergy;

        /// <summary>
        /// 英雄颜色（对应糖果颜色）
        /// </summary>
        public int HeroColor => Entry.Color;

        /// 所属队伍引用
        /// </summary>
        public EntityRef<EntityGroup> GroupRef;

        /// <summary>
        /// 实体配置Id
        /// </summary>
        public int EntryId;

        /// <summary>
        /// 属性控制器
        /// </summary>
        public EntityRef<AttComponent> AttCom;

        /// <summary>
        /// 状态控制器
        /// </summary>
        public EntityRef<StateComponent> StateCom;

    }
}