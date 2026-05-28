using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.5.17
    /// Desc
    /// </summary>
    public partial class CrawlersPanelComponent : Entity, IUpdate
    {
        public int BattleId;
        public EntityRef<CrawlerBattleComponent> BattleRef;
        public KeyCode ToggleHandTuningKey = KeyCode.H;
    }
}
