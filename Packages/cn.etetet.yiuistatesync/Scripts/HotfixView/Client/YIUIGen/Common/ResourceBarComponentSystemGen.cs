using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIChild))]
    [EntitySystemOf(typeof(ResourceBarComponent))]
    public static partial class ResourceBarComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ResourceBarComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this ResourceBarComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this ResourceBarComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();

            self.u_EventAddEnergy = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventAddEnergy");
            self.u_EventAddEnergyHandle = self.u_EventAddEnergy.Add(self,ResourceBarComponent.OnEventAddEnergyInvoke);
            self.u_EventAddGem = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventAddGem");
            self.u_EventAddGemHandle = self.u_EventAddGem.Add(self,ResourceBarComponent.OnEventAddGemInvoke);
            self.u_EventAddCoin = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventAddCoin");
            self.u_EventAddCoinHandle = self.u_EventAddCoin.Add(self,ResourceBarComponent.OnEventAddCoinInvoke);
            self.u_EventAddDiamond = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventAddDiamond");
            self.u_EventAddDiamondHandle = self.u_EventAddDiamond.Add(self,ResourceBarComponent.OnEventAddDiamondInvoke);

        }
    }
}
