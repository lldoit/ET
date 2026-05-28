using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.1.7
    /// Desc
    /// </summary>
    [FriendOf(typeof(LoadingPanelComponent))]
    public static partial class LoadingPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LoadingPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoadingPanelComponent self)
        {
            self.LoadingAnimationToken?.Cancel();
            self.LoadingAnimationToken = null;
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LoadingPanelComponent self)
        {
            self.LoadingAnimationToken?.Cancel();
            self.LoadingAnimationToken = new ETCancellationToken();
            ETCancellationToken cancellationToken = self.LoadingAnimationToken;
            EntityRef<LoadingPanelComponent> selfRef = self;

            await self.PlayLoadingAnimation(cancellationToken);
            if (cancellationToken.IsCancel())
            {
                return false;
            }

            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return false;
            }

            return true;
        }

        [EnableGetComponent(typeof(TimerComponent))]
        private static async ETTask PlayLoadingAnimation(this LoadingPanelComponent self, ETCancellationToken cancellationToken)
        {
            const float duration = 1.5f;
            float elapsedTime = 0f;
            EntityRef<LoadingPanelComponent> selfRef = self;

            self.u_DataProgress.SetValue(0f, true);

            while (elapsedTime < duration)
            {
                if (cancellationToken.IsCancel())
                {
                    return;
                }

                self = selfRef;
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                await self.Root().GetComponent<TimerComponent>().WaitFrameAsync();
                if (cancellationToken.IsCancel())
                {
                    return;
                }

                self = selfRef;
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration) * 100f;
                self.u_DataProgress.SetValue(progress);
            }

            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }

            self.u_DataProgress.SetValue(100f, true);
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}
