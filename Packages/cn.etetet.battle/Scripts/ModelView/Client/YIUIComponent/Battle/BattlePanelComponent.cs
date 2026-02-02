using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 战斗主界面面板
    /// 对应设计文档中的 "BattleMainPanel"
    /// 负责管理战斗场景的三个主要区域：
    /// 1. Top: 战斗表现区 (Formation, Characters, DamageNumbers)
    /// 2. Middle: 信息提示区 (TurnCounter, Intent)
    /// 3. Bottom: 三消盘面区 (Match3Board)
    /// </summary>
    public partial class BattlePanelComponent : Entity
    {
        // 运行时状态
        public int CurrentTurn;
    }
}
