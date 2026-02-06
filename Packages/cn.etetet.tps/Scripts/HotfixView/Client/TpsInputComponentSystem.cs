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
            self.Sensitivity = 0.5f;
            self.IsInputEnabled = true;

            // 初始化新增字段
            self.AimScreenOffset = Vector2.zero;
            self.MaxAimScreenOffset = new Vector2(Screen.width * 0.45f, Screen.height * 0.45f);
            self.CrosshairScreenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);

            // 尝试读取场景配置
            TpsLevelConfig config = UnityEngine.Object.FindFirstObjectByType<TpsLevelConfig>();
            if (config != null)
            {
                self.MaxAimScreenOffset = config.MaxAimScreenOffset;
                Log.Info($"[TPS] 使用场景配置: MaxAimRange={config.MaxAimScreenOffset}");
            }
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

            // 计算相对于屏幕中心的偏移
            float halfWidth = Screen.width / 2f;
            float halfHeight = Screen.height / 2f;

            if (self.IsPressing)
            {
                // 按下的第一帧：只记录位置，不计算 delta
                if (!wasPressed)
                {
                    self.LastInputPosition = self.ScreenPosition;
                }
                else
                {
                    // 后续帧：计算 Delta 并累加
                    Vector2 delta = self.ScreenPosition - self.LastInputPosition;
                    self.LastInputPosition = self.ScreenPosition;

                    // 累加位移
                    self.AimScreenOffset += delta * self.Sensitivity;

                    // Clamp 限制边界
                    self.AimScreenOffset = new Vector2(
                        Mathf.Clamp(self.AimScreenOffset.x, -self.MaxAimScreenOffset.x, self.MaxAimScreenOffset.x),
                        Mathf.Clamp(self.AimScreenOffset.y, -self.MaxAimScreenOffset.y, self.MaxAimScreenOffset.y)
                    );
                }

                // 计算准星屏幕坐标（无论是否第一帧都要更新）
                self.CrosshairScreenPosition = new Vector2(
                    halfWidth + self.AimScreenOffset.x,
                    halfHeight + self.AimScreenOffset.y
                );

                // 保留归一化方向用于兼容
                self.NormalizedAimDirection = new Vector2(
                    self.AimScreenOffset.x / self.MaxAimScreenOffset.x,
                    self.AimScreenOffset.y / self.MaxAimScreenOffset.y
                ) * self.Sensitivity;
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

            // 记录当前位置作为初始 LastInputPosition，防止第一帧跳变
            self.LastInputPosition = self.ScreenPosition;

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

            // 不再重置瞄准方向和准星位置，保持最后的瞄准状态
            // self.NormalizedAimDirection = Vector2.zero;
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
