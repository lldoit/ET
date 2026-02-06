using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.2.4
    /// Desc
    /// </summary>
    [FriendOf(typeof(TpsBattlePanelComponent))]
    public static partial class TpsBattlePanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this TpsBattlePanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TpsBattlePanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TpsBattlePanelComponent self)
        {
            var currentSceneComponent = self.Root().GetComponent<CurrentScenesComponent>();
            var crosshairComponent = currentSceneComponent.Scene.GetComponent<TpsCrosshairComponent>();
            if (crosshairComponent != null)
            {
                crosshairComponent.SetCrosshairUI(self.u_ComU_CrosshairRectTransform);
                Log.Info("[TPS] UI Crosshair 绑定成功!");
            }

            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始

        [YIUIInvoke(TpsBattlePanelComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this TpsBattlePanelComponent self)
        {
            TpsSceneHelper.ExitTpsAsync(self.Root()).NoContext();
        }
        #endregion YIUIEvent结束
    }
}
