# KOF HitBox 与 UpdateMap (动画烘焙数据) 实施计划

**日期**：2026-03-10
**目标**：在 ET 9.0 `cn.etetet.kof` 包中，实现基于单一脚本的静态判定框配置，同时编写编辑器工具从 UFE 原工程中导出 `AnimationMap` 逐帧重载数据，供 Model 层实现精确的 `UpdateMap` 碰撞判定机制。

---

## 阶段 1：静态判定框配置 (单脚本驱动)

这是获取基础默认碰撞数据和 Gizmos 可视化的前提。

**1. Model 层结构**
- 创建 `KofHitBoxType` 和 `KofHitBoxShape` 枚举。
- 创建 `KofHitBoxData` 结构体，包含 `BoxType`, `Shape`, `Radius`, `Offset`, `BoneName`。
- 创建 `KofHitBoxesComponent` (挂载在 `KofFighterComponent` 下)，持有 `List<KofHitBoxData>`。

**2. View 层结构**
- 创建 `KofHitBoxConfig` 和 `KofHitBoxesView` (MonoBehaviour)。
- 在 `KofHitBoxesView` 中移植 UFE 的 `OnDrawGizmos` 代码，实现在 Scene 视图可视化的红绿圈。

**3. 转化逻辑**
- 在战斗初始化节点（如 `KofBattleHelper.EnterBattle` 创建化身时），从预制体实例的 `KofHitBoxesView` 读取配置，生成 `KofHitBoxData` 列表并传递给 Model 层的 `KofHitBoxesComponent`。

---

## 阶段 2：UpdateMap 动效烘焙数据导出与驱动

**1. 导出工具 (Unity Editor Script)**
- 编写 `UfeAnimationMapExporter.cs`（放在 `Packages/cn.etetet.kof/Scripts/Editor/`）。
- **工具功能**：
  - 加载指定的 UFE `MoveSetScript` 或特定的 `.asset`。
  - 遍历里面的 `AnimationMap[]`，提取出每一帧（`frame`）各个 `BodyPart`（转化为我们定义的 `BoneName`）的 `mappedPosition`。
  - 将这些数据组织成 ET9 友好的格式（如生成 JSON 文件，或生成 C# 静态配置类代码）。为了 ET 习惯，可以先输出一份 JSON 到配置目录。

**2. Model 层 UpdateMap 控制结构**
- **数据层**：
  - `KofFrameHitBoxData`：存储单骨骼单帧偏移。
  - `KofAnimationFrameData`：存储某一帧下所有骨骼的偏移集合。
  - `KofAnimationMapConfig`：存储一个招式（MoveId）下的所有帧的偏移数组。
- **组件层**：
  - `KofAnimationMapComponent`：挂载在 Entity 上，持有此角色解析后的字典 `Dictionary<int, KofAnimationMapConfig>`。
- **逻辑层**：
  - `KofHitBoxesUpdateSystem.Tick`：在碰撞检测之前调用。它读取当前角色的 `MoveId` 和进行到哪一 `Frame`，从 `KofAnimationMapComponent` 中查表，把帧相对坐标拿出来覆盖到 `KofHitBoxesComponent` 里的 `Offset`，并结合 `FacingRight` 进行 X 轴符号翻转。

---

## 阶段 3：装配与验证

1. **执行导出**：通过工具将 UFE 的 `Robot_Kyle` 动作地图数据导出为 KOF 可读格式。
2. **场景连通**：加载战斗场景，AI 执行招式时（或人工键盘发出招式时），断点检验 `KofHitBoxesUpdateSystem` 是否准确地取到了对应的 Frame 数据并修改了 HitBox 位置。
3. **表现层连通**：为了直观验证，可以在原本的 Gizmos 系统之上，在 `Update()` 里向 Model 询问当前的实际纯数学坐标，并在 Scene 里用紫色球体画出来，肉眼对比是否与动画对齐。
