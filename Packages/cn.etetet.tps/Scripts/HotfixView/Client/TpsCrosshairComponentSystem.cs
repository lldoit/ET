using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS准星系统
    /// 控制UI准星跟随和反馈
    /// </summary>
    [FriendOf(typeof(TpsCrosshairComponent))]
    [FriendOf(typeof(TpsInputComponent))]
    [EntitySystemOf(typeof(TpsCrosshairComponent))]
    public static partial class TpsCrosshairComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TpsCrosshairComponent self)
        {
            self.FollowSpeed = 15f;
            self.IsVisible = false;
            self.CurrentScale = 1f;
            self.DefaultScale = 1f;
        }

        [EntitySystem]
        private static void Update(this TpsCrosshairComponent self)
        {
            if (self.CrosshairRect == null)
            {
                return;
            }

            self.UpdateCrosshairPosition();
            self.UpdateCrosshairVisibility();
        }

        [EntitySystem]
        private static void Destroy(this TpsCrosshairComponent self)
        {
            if (self.CrosshairGO != null)
            {
                self.CrosshairGO.SetActive(false);
            }
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 更新准星位置
        /// </summary>
        private static void UpdateCrosshairPosition(this TpsCrosshairComponent self)
        {
            TpsInputComponent inputComponent = self.Scene().GetComponent<TpsInputComponent>();
            if (inputComponent == null || !inputComponent.IsPressing)
            {
                return;
            }

            // 平滑跟随输入位置
            Vector2 currentPos = self.CrosshairRect.anchoredPosition;
            Vector2 targetPos = new Vector2(
                inputComponent.ScreenPosition.x - Screen.width / 2f,
                inputComponent.ScreenPosition.y - Screen.height / 2f
            );

            self.CrosshairRect.anchoredPosition = Vector2.Lerp(
                currentPos,
                targetPos,
                Time.deltaTime * self.FollowSpeed
            );
        }

        /// <summary>
        /// 更新准星可见性
        /// </summary>
        private static void UpdateCrosshairVisibility(this TpsCrosshairComponent self)
        {
            TpsStateComponent stateComponent = self.Scene().GetComponent<TpsStateComponent>();
            if (stateComponent == null)
            {
                return;
            }

            bool shouldBeVisible = stateComponent.IsAiming();
            if (self.IsVisible != shouldBeVisible)
            {
                self.IsVisible = shouldBeVisible;
                if (self.CrosshairGO != null)
                {
                    self.CrosshairGO.SetActive(self.IsVisible);
                }
            }
        }

        /// <summary>
        /// 设置准星UI引用
        /// </summary>
        public static void SetCrosshairUI(this TpsCrosshairComponent self, RectTransform crosshairRect)
        {
            self.CrosshairRect = crosshairRect;
            self.CrosshairGO = crosshairRect?.gameObject;

            if (self.CrosshairGO != null)
            {
                self.CrosshairGO.SetActive(false);
            }
        }

        /// <summary>
        /// 射击反馈（准星放大）
        /// </summary>
        public static async ETTask PlayFireFeedback(this TpsCrosshairComponent self)
        {
            if (self.CrosshairRect == null)
            {
                return;
            }

            EntityRef<TpsCrosshairComponent> selfRef = self;

            // 放大
            self.CrosshairRect.localScale = Vector3.one * (self.DefaultScale * 1.3f);

            await self.Root().GetComponent<TimerComponent>().WaitAsync(50);

            self = selfRef;
            if (self == null || self.IsDisposed || self.CrosshairRect == null)
            {
                return;
            }

            // 恢复
            self.CrosshairRect.localScale = Vector3.one * self.DefaultScale;
        }

        #endregion
    }
}
