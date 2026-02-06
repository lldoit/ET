# 战斗场景技术规格说明书

## 1. 概述 (Overview)
基于 ET 9.0 ECS 实现类似 **Nikke 风格的准星驱动视差 (Aim-Driven Parallax)** 战斗场景，采用 **World Space (SpriteRenderer)** 技术方案。
该系统利用玩家的瞄准输入，优先移动 UI 准星，并带动摄像机延迟跟随，同时通过多层视差产生深度感。此外，系统通过**偏移量叠加 (Offset Composition)** 完美解决射击震动与瞄准移动的冲突。

## 2. 架构 (Architecture)

### 2.1. 核心组件 (Core Components)

#### `TpsSceneControllerComponent` (客户端 - Client)
- **作用域**: 场景 (Client)
- **数据**:
    - `Vector2 AimScreenOffset`: **虚拟瞄准点**相对于屏幕中心的偏移量 (像素)。
    - `Vector2 MaxAimScreenOffset`: 准星允许移动的最大屏幕范围 (例如: Screen.width * 0.45)。
    - `float CameraFollowRatio`: 摄像机跟随准星的移动比例 (例如 0.5，表示准星移100像素，相机移相当于50像素的单位)。
    - `Vector2 CrosshairScreenPos`: 最终计算出的准星屏幕坐标 (用于 UI 同步)。

#### `TpsCameraComponent` (客户端 - Client)
- **作用域**: 场景 (Client) / 摄像机 Entity
- **数据**:
    - `Vector3 OriginPos`: 摄像机初始位置。
    - `EntityRef<Camera> UnityCamera`: Unity 摄像机的引用。
    - `float SmoothTime`: 摄像机移动的阻尼平滑时间。
    - `Vector3 CurrentAimOffset`: 当前因瞄准产生的摄像机位移 (经过平滑处理)。
    - `Vector3 ShakeOffset`: 当前因震动产生的临时位移。

#### `ParallaxLayerComponent` (客户端 - Client)
- **作用域**: 背景层 Entity
- **数据**:
    - `float ParallaxFactor`: 相对于摄像机移动的速度系数。

### 2.2. 系统 (Systems)

#### `TpsInputSystem` (Update)
1. **输入读取**: 获取鼠标/触控位置增量 或 绝对位置。
2. **虚拟点计算**:
   - `AimScreenOffset = InputPosition - ScreenCenter`
3. **边界限制 (Clamping)**:
   - `AimScreenOffset.x = Clamp(AimScreenOffset.x, -MaxX, MaxX)`
   - `AimScreenOffset.y = Clamp(AimScreenOffset.y, -MaxY, MaxY)`
4. **输出**: 更新 `TpsSceneControllerComponent.AimScreenOffset` 和 `CrosshairScreenPos`。

#### `TpsCrosshairSystem` (Update)
- **职责**: 同步 UI 准星位置。
- **逻辑**: 将 `TpsSceneControllerComponent.CrosshairScreenPos` 赋给 UI 准星的 RectTransform。

#### `TpsCameraSystem` (LateUpdate)
- **职责**: 计算摄像机最终位置。
- **流程**:
  1. **目标计算**: `TargetCamOffset = AimScreenOffset * CameraFollowRatio * PixelToWorldUnit`
  2. **平滑跟随**: `CurrentAimOffset = SmoothDamp(CurrentAimOffset, TargetCamOffset)`
  3. **震动衰减**: 更新并计算 `ShakeOffset` (如使用噪声或阻尼衰减)。
  4. **位置合成**:
     ```csharp
     FinalPos = OriginPos + CurrentAimOffset + ShakeOffset;
     ```
  5. **应用**: 设置 Unity Camera Transform。

#### `TpsShootingSystem` (Event/Call)
- **职责**: 触发射击和震动。
- **逻辑**: 调用 `TpsCameraComponent.ApplyShake(intensity)`，给 `ShakeOffset` 施加一个瞬间冲量或启动震动过程。

#### `ParallaxSystem` (LateUpdate)
- **时序**: 在 `TpsCameraSystem` 之后执行。
- **职责**: 根据摄像机的新位置，更新背景层。

## 3. 场景结构 (Scene Structure)

```text
Global
  └── UI Root (Screen Space)
      └── BattleUI
          └── Crosshair (Image)  <-- 由 TpsCrosshairSystem 控制

TpsScene (Entity/GameObject)
  ├── MainCamera (Position: 0, 0, -10)
  │    └── [TpsCameraComponent]
  │
  └── EnvironmentRoot
       ├── ... (Parallax Layers) ...
```

## 4. 工作流 (Workflows)

### 4.1. 初始化 (Initialization)
1. `TpsSceneFactory`: 创建场景 Entity，绑定 Camera，初始化 `OriginPos`。
2. `TpsUIFactory`: 创建战斗 UI，获取 Crosshair 引用。

### 4.2. 运行时 (Runtime)
1. **Input**: 玩家移动鼠标 -> `AimScreenOffset` 变化 (被 Clamp 限制)。
2. **UI**: 准星立刻移动到新位置 (响应快)。
3. **Camera**: 摄像机根据 `AimScreenOffset` 乘以系数，平滑地向目标移动 (响应慢，范围小)。
4. **Shoot**: 玩家射击 -> `ShakeOffset` 产生波动 -> 叠加到 Camera 位置 (此时 AimOffset 保持不变，互不冲突)。
5. **Parallax**: 背景层根据 Camera 移动产生视差。

## 5. 实施步骤 (Implementation Steps)

1. **Refactor TpsCamera**: 修改 Component 结构，分离 `ShakeOffset` 和 `AimOffset`。
2. **Enhance Input**: 实现屏幕坐标的 Clamp 逻辑。
3. **UI Integration**: 确保准星 UI 能根据 Input 数据移动。
4. **Update Systems**: 重写 Camera 和 Input System 逻辑。
