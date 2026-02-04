using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS输入系统
    /// 处理触摸/鼠标输入，转换为瞄准数据
    /// </summary>
    [FriendOf(typeof(TpsInputComponent))]
    [EntitySystemOf(typeof(TpsInputComponent))]
    public static partial class TpsInputComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TpsInputComponent self)
        {
            self.IsPressing = false;
            self.ScreenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
            self.NormalizedAimDirection = Vector2.zero;
            self.Sensitivity = 1.0f;
            self.IsInputEnabled = true;
        }

        [EntitySystem]
        private static void Update(this TpsInputComponent self)
        {
            if (!self.IsInputEnabled)
            {
                return;
            }

            self.ProcessInput();
        }

        [EntitySystem]
        private static void Destroy(this TpsInputComponent self)
        {
            self.IsInputEnabled = false;
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 处理输入
        /// </summary>
        private static void ProcessInput(this TpsInputComponent self)
        {
            // 检测触摸或鼠标输入
            bool wasPressed = self.IsPressing;

#if UNITY_EDITOR || UNITY_STANDALONE
            // 鼠标输入
            self.IsPressing = Input.GetMouseButton(0);
            if (self.IsPressing)
            {
                self.ScreenPosition = Input.mousePosition;
            }
#else
            // 触摸输入
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                self.IsPressing = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                if (self.IsPressing)
                {
                    self.ScreenPosition = touch.position;
                }
            }
            else
            {
                self.IsPressing = false;
            }
#endif

            // 计算归一化瞄准方向
            if (self.IsPressing)
            {
                float halfWidth = Screen.width / 2f;
                float halfHeight = Screen.height / 2f;

                self.NormalizedAimDirection = new Vector2(
                    Mathf.Clamp((self.ScreenPosition.x - halfWidth) / halfWidth, -1f, 1f) * self.Sensitivity,
                    Mathf.Clamp((self.ScreenPosition.y - halfHeight) / halfHeight, -1f, 1f) * self.Sensitivity
                );
            }

            // 检测状态切换
            if (self.IsPressing && !wasPressed)
            {
                self.OnPressDown();
            }
            else if (!self.IsPressing && wasPressed)
            {
                self.OnPressUp();
            }
        }

        /// <summary>
        /// 按下时触发
        /// </summary>
        private static void OnPressDown(this TpsInputComponent self)
        {
            Log.Info("[TPS] OnPressDown 触发");

            // 通知状态组件切换到瞄准状态
            Scene scene = self.Scene();

            TpsStateComponent stateComponent = scene.GetComponent<TpsStateComponent>();
            if (stateComponent == null)
            {
                Log.Error($"[TPS] TpsStateComponent 未找到! Scene={scene.Name}, SceneType={scene.SceneType}");
                return;
            }
            stateComponent.SwitchToAiming();
        }

        /// <summary>
        /// 松开时触发
        /// </summary>
        private static void OnPressUp(this TpsInputComponent self)
        {
            Log.Info("[TPS] OnPressUp 触发");

            // 通知状态组件切换到掩体状态
            TpsStateComponent stateComponent = self.Scene().GetComponent<TpsStateComponent>();
            stateComponent?.SwitchToCover();

            // 重置瞄准方向
            self.NormalizedAimDirection = Vector2.zero;
        }

        /// <summary>
        /// 启用/禁用输入
        /// </summary>
        public static void SetInputEnabled(this TpsInputComponent self, bool enabled)
        {
            self.IsInputEnabled = enabled;
            if (!enabled)
            {
                self.IsPressing = false;
                self.NormalizedAimDirection = Vector2.zero;
            }
        }

        #endregion
    }
}
