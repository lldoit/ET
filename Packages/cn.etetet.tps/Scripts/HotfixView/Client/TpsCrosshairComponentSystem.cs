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
        /// 使用 TpsInputComponent 中已经 Clamp 过的 CrosshairScreenPosition
        /// </summary>
        private static void UpdateCrosshairPosition(this TpsCrosshairComponent self)
        {
            TpsInputComponent inputComponent = self.Scene().GetComponent<TpsInputComponent>();
            if (inputComponent == null)
            {
                return;
            }

            // 使用经过 Clamp 的准星屏幕坐标（即使松开鼠标也保持位置）
            Vector2 targetPos = new Vector2(
                inputComponent.CrosshairScreenPosition.x - Screen.width / 2f,
                inputComponent.CrosshairScreenPosition.y - Screen.height / 2f
            );
            
            // 快速跟随准星位置（准星响应要快于相机）
            Vector2 currentPos = self.CrosshairRect.anchoredPosition;
            self.CrosshairRect.anchoredPosition = Vector2.Lerp(
                currentPos,
                targetPos,
                Time.deltaTime * self.FollowSpeed
            );
            // 直接设置准星位置
            //self.CrosshairRect.anchoredPosition = targetPos;
        }

        /// <summary>
        /// 更新准星可见性（准星始终可见）
        /// </summary>
        private static void UpdateCrosshairVisibility(this TpsCrosshairComponent self)
        {
            // 准星始终可见
            if (!self.IsVisible && self.CrosshairGO != null)
            {
                self.IsVisible = true;
                self.CrosshairGO.SetActive(true);
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
