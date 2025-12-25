using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2025.12.25
    /// Desc
    /// </summary>
    public partial class WeeklyViewComponent : Entity
    {
        public EntityRef<YIUILoopScrollChild> m_Loop;
        public YIUILoopScrollChild Loop => m_Loop;
    }
}
