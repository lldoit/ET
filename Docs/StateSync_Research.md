# ET9 `cn.etetet.statesync` 包研究报告

## 1. 概述与定位

`cn.etetet.statesync` 是基于 ET 9.0 架构实现的一个**状态同步（State Sync）模块演示/基础实现包**。它展示了如何使用 ET 的核心包（如 `Core`, `Model`, `Hotfix`, `AOI`, `Recast`, `Move`, `Unit` 等）来构建一个具备客户端-服务端状态同步功能的游戏基础框架（如 MMO、ARPG 甚至 TPS 的底层架构参考）。

从依赖关系（`package.json`）可以看出，它高度集成了 ET 的各个核心系统，包括寻路（Recast）、数值（Numeric）、AI（BehaviorTree）、视野（AOI）、软路由（Router）等。

## 2. 目录架构与拆分机制

严格遵循了 ET 9.0 的红线规范，按**四层程序集**进行了严格的分离和封装：`Model`, `ModelView`, `Hotfix`, `HotfixView`。此外还包含了 `Excel` 和 `Proto` 作为配置和协议定义的来源。

### 2.1 数据与实体层（Model / ModelView）
- **Model** (前后端共享模型): 
  - 包含了全局使用的事件定义（`EventType.cs`）和业务场景的类型设定（`SceneType.cs`）。
  - 其中定义了特定的 `SceneType`：`Http(1001)`, `Map(1002)`, `Robot(1003)`, `StateSync(1020)`, `Current(1021)`, `StateSyncView(1024)`。
  - 提供了客户端专用的 `CurrentScenesComponent` 用于管理单/多客户场景实例（适用于大世界无缝地图）。
- **ModelView** (客户端专属视觉与交互模型):
  - `OperaComponent`: 管理玩家的操作输入数据层定义（射线检测 LayerMask 和 点击坐标）。
  - `GameObjectComponent`: 基础组件，用于持有 Unity 的 `GameObject` 和 `Transform` 引用，桥接 Entity 逻辑与 Unity 原生表现。
  - `AnimatorComponent`: 管理动画表现层数据。

### 2.2 逻辑与热更层（Hotfix / HotfixView）
- **Hotfix** (前后端共享热更新逻辑):
  - **服务器 (Server)**: 
    - 处理场景和地图管理：`GateMapFactory`, `TransferHelper` 处理跨地图和进程切换。
    - 处理广播与AOI：`MapMessageHelper` 负责向视野内（`GetBeSeePlayers()`）的玩家广播消息，如实体的出现、消失或位置的变更。利用了底层的 `MessageLocationSenderComponent` 实现高性能的局部广播。
    - 处理寻路与移动：响应客户端发来的 `C2M_PathfindingResult`。
  - **客户端 (Client)**:
    - 场景切换管理：`SceneChangeHelper` 控制繁杂的切换流程，释放旧场景，拉取新场景资源加载事件，并监听 `M2C_CreateMyUnit` 由服务端统一下发玩家主体。
    - 处理服务端同步的移动与停止：如 `M2C_PathfindingResultHandler` 和 `M2C_StopHandler` 实现对服务端轨迹在客户端的模拟与插值。
- **HotfixView** (客户端表现与交互热更逻辑):
  - **输入控制 (`OperaComponentSystem`)**: 使用 `Input` 系统进行按键检测与射线拾取（通过 `Raycast` 地面层）并发送行动指令到服务器。并且内部植入了相关的协程和 ETTask Cancellation 的边界测试案例。
  - **UI 与生命周期整合**: 使用事件订阅 `AEvent<Scene, AfterCreateCurrentScene>`，在创建完 Current 场景后顺手挂载 `UIComponent` 等核心表现模块组件。

## 3. 核心运行原理解析

`cn.etetet.statesync` 完整串联了一个基于 **“状态同步”** 模式的玩家交互循环：

1. **玩家登录与进图**:
   - 客户端通过 Login / Gate 相关请求后，Gate 端建立对应的 Session 并派发进图请求，在 Map 进程实例化对应的 `Unit`。
2. **场景下发与可见性 (AOI)**:
   - Server 利用 `UnitEnterSightRange_NotifyClient` 和 `UnitLeaveSightRange_NotifyClient` 协管实体的显隐。
   - 这完全依赖于下层包 `cn.etetet.aoi` 的驱动，从而节省网络包量，只有进入视野的实体，客户端才会收到 `M2C_CreateUnits` 从而在本地利用 `UnitFactory` 创建表现实体挂载 `GameObjectComponent`。
3. **状态发起与服务端权威验证**:
   - 客户端的操作（点击地面）由 `OperaComponent` 检测发往服务端（`C2M_PathfindingResult`）。 
   - 客户端**不直接修改本地状态与坐标**，而是完全由服务端在内部执行真正的 Recast 寻路逻辑。
   - 服务端计算出结果后，向该 Unit 周围的 AOI 范围广播路点信息或状态变换信息。
4. **客户端表现同步**:
   - 客户端收到带有路点和时间戳的信息后，利用 `MoveHelper` 执行平滑插值移动，从而在视觉上达到一致性。

## 4. 的特殊之处与规范示范

1. **严格的协程安全测试**:
   - 在 `OperaComponentSystem.cs` 中，故意放置了关于 `CoroutineLockComponent` 死锁检测机制以及 `ETCancellationToken` 取消机制的测试用例（如按 Q, W, A 等热键）。展示了在 ET 中如何安全地挂起和取消异步任务。
2. **场景分层管理的典范 (CurrentScene 机制)**:
   - 客户端引入了 `CurrentScenesComponent` 并将实际的游戏交互映射在 `SceneType.Current` 下，这与常规的 Global 场景或框架 Scene 分开。这样当玩家进行场景切换（如从主城到副本）时，只需将旧的 `CurrentScene` 调用 `Dispose()` 即可瞬间完成资源的释放和实体的解绑。
3. **彻底贯彻 ECS 逻辑**:
   - 没有任何 Unity 自带行为的参与（不使用 `Update` / `FixedUpdate` Mono 机制），所有 Unity GameObject 仅仅被当做 “表现壳”，通过 `GameObjectComponent` 持有引用。坐标同步和位移运算完全由抽象的纯 C# Mathematics 计算执行后再映射给 Transform。

## 5. 总结

`cn.etetet.statesync` 在 ET9 中充当了一个极高价值的 “Best Practice” 官方样板工程。它展示了如何组织跨服、软路由、寻路、视野和客户端表现同步之间的复杂关系，且完全契合严苛的 Module 和 Assembly 隔离标准。任何想要基于 ET 开发传统 MMO / ARPG 状态同步手游的开发者，其工程主干均可从此包复刻或继承。
