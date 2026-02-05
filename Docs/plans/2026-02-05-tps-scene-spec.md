# 战斗场景技术规格说明书

## 1. 概述 (Overview)
基于 ET 9.0 ECS 实现类似 **Nikke 风格的准星驱动视差 (Aim-Driven Parallax)** 战斗场景，采用 **World Space (SpriteRenderer)** 技术方案。
该系统利用玩家的瞄准输入轻微平移摄像机，通过多层视差产生深度感。

## 2. 架构 (Architecture)

### 2.1. 核心组件 (Core Components)

#### `TpsSceneControllerComponent` (客户端 - Client)
- **作用域**: 场景 (Client)
- **数据**:
  - `Vector2 AimScreenPosition`: 当前原始输入位置 (屏幕坐标)。
  - `Vector2 AimNormalizedPos`: 归一化瞄准位置 ((-1, -1) 到 (1, 1))。
  - `float MaxCameraOffset`: 摄像机偏离原点的最大距离 (例如: 2.0 Unity单位)。

#### `TpsCameraComponent` (客户端 - Client)
- **作用域**: 场景 (Client) / 摄像机 Entity
- **数据**:
  - `Vector3 OriginPos`: 摄像机初始位置。
  - `EntityRef<Camera> UnityCamera`: Unity 摄像机的引用。
  - `float SmoothTime`: 摄像机移动的阻尼平滑时间。

#### `ParallaxLayerComponent` (客户端 - Client)
- **作用域**: 背景层 Entity
- **数据**:
  - `float ParallaxFactor`: 相对于摄像机移动的速度系数。
    - `0.0`: 远景 (天空) - 相对于摄像机几乎静止。
    - `1.0`: 近景 (掩体/英雄) - 与摄像机 1:1 移动 (基准层)。
  - `Vector3 OriginPos`: 初始局部坐标。

### 2.2. 系统 (Systems)

#### `TpsInputSystem` (Update)
- 读取输入 (鼠标/触控)。
- 更新 `TpsSceneControllerComponent.AimNormalizedPos`。
- 将输入限制在有效的屏幕范围内。

#### `TpsCameraSystem` (LateUpdate)
- 读取 `TpsSceneControllerComponent.AimNormalizedPos`。
- 实现逻辑:
  ```csharp
  TargetPos = OriginPos + (AimNormalizedPos * MaxCameraOffset);
  CurrentPos = Vector3.Lerp(CurrentPos, TargetPos, Time.deltaTime * Speed);
  ```
- 更新 Unity 摄像机的 Transform。
- **关键点**: 必须在 Input *之后*、Parallax *之前* 执行。

#### `ParallaxSystem` (LateUpdate)
- 监听摄像机移动 (或在摄像机更新后的 LateUpdate 中运行)。
- 更新每个 `ParallaxLayerComponent` entity。
- 公式:
  ```csharp
  // 计算摄像机相对于原点的位移
  Vector3 distFromOrigin = Camera.position - CameraOrigin;
  
  // 应用视差系数 (反向方向以产生深度感)
  // 简单的相对定位逻辑:
  // LayerLocalPos = OriginPos + (CameraOffset * (1.0f - ParallaxFactor));
  ```

## 3. 场景结构 (Scene Structure)

```text
Global
  └── UI Root (Screen Space)
      └── ... UI 元素 ...

TpsScene (Entity/GameObject)
  ├── MainCamera (Position: 0, 0, -10)
  │    └── [TpsCameraComponent]
  │
  └── EnvironmentRoot
       ├── Layer0_Sky       [ParallaxFactor: 0.05] (SpriteRenderer, SortingOrder: -10)
       ├── Layer1_City      [ParallaxFactor: 0.2]  (SpriteRenderer, SortingOrder: -5)
       ├── Layer2_MidGround [ParallaxFactor: 0.5]  (SpriteRenderer, SortingOrder: 0)
       ├── Layer3_Battle    [ParallaxFactor: 1.0]  (基准层)
       │    ├── HeroPositions (占位符)
       │    └── EnemyPositions (占位符)
       └── Layer4_ForeGround [ParallaxFactor: 1.2] (SpriteRenderer, SortingOrder: 10)
```

## 4. 工作流 (Workflows)

### 4.1. 初始化 (Initialization)
1. `TpsSceneFactory`: 创建 TPS 场景 Entity。
2. 加载 `TpsScene.prefab` (包含层级结构)。
3. 将 `UnityCamera` 链接到 `TpsCameraComponent`。
4. 遍历 `EnvironmentRoot` 子节点，根据命名约定或组件数据自动添加 `ParallaxLayerComponent`。

### 4.2. 运行时 (Runtime)
1. 玩家拖动屏幕 -> `AimNormalizedPos` 改变。
2. `TpsCameraSystem` 将摄像机向目标偏移量移动。
3. `ParallaxSystem` 更新背景层以产生深度感。

## 5. 实施步骤 (Implementation Steps)
1. 创建组件 (`TpsCameraComponent`, `ParallaxLayerComponent`, `TpsSceneController`)。
2. 实现系统 (`TpsInputSystem`, `TpsCameraSystem`, `ParallaxSystem`)。
3. 构建 `TpsSceneFactory` 的资源加载逻辑。
