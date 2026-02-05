# NumericWatcher 使用指南

在 ET 9.0 中，`NumericWatcher` 是实现 **数值系统 (Model) -> 表现系统 (View/Logic)** 单向数据绑定的核心机制。它本质上是一个基于类型的事件监听器。

## 1. 核心原理

当 `NumericComponent` 中的数值发生变化（如 `unit.GetComponent<NumericComponent>().Set(NumericType.Hp, 100)`）时，系统会自动分发事件，查找带有 `[NumericWatcher]` 特性的类并执行其 `Run` 方法。

## 2. 基本用法步骤

### 步骤一：创建 Watcher 类
在 `Hotfix` 或 `HotfixView` 程序集中创建一个类实现 `INumericWatcher`。
- `Hotfix`：用于处理纯逻辑（如属性计算、数值联动）。
- `HotfixView`：用于处理 UI 更新、特效播放等表现层逻辑。

### 步骤二：添加特性
使用 `[NumericWatcher(SceneType, NumericType)]` 标记该类。
*   `SceneType`：指定监听哪个场景的事件（通常客户端用 `SceneType.StateSync` 或 `SceneType.Current`）。
*   `NumericType`：指定监听哪个数值的变化（如 `NumericType.Hp`, `NumericType.MaxHp`, `NumericType.Speed` 等）。

### 步骤三：实现逻辑
在 `Run` 方法中处理变化。

## 3. 数据绑定（UI同步）示例

**场景**：当角色的 **HP** 变化时，自动更新头顶的 **血条UI**。

该代码通常位于 **Client** 的 **HotfixView** 程序集中。

```csharp
using ET.Client;

namespace ET.Client
{
    // 监听 StateSync 场景下的 HP 数值变化
    // SceneType 请根据你的项目实际情况选择，StateSync项目通常是 SceneType.StateSync
    [NumericWatcher(SceneType.StateSync, NumericType.Hp)] 
    public class NumericWatcher_Hp_UpdateUI : INumericWatcher
    {
        public void Run(Unit unit, NumbericChange args)
        {
            // args.NewValue 是变化后的新值
            // args.OldValue 是变化前的旧值
            
            // 1. 获取该 Unit 对应的 UI 组件
            // 假设在这个 Unit 上挂了一个 HeadBarUIComponent (View层组件)
            // 注意：这里需要确保你的 View 组件是可以从 Unit 获取到的
            HeadBarComponent headBar = unit.GetComponent<HeadBarComponent>();
            
            if (headBar == null)
            {
                return;
            }

            // 2. 获取最大血量（计算百分比用）
            // NumericComponent 也是在 Unit 上的
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            long maxHp = numericComponent.GetAsLong(NumericType.MaxHp);
            
            if (maxHp <= 0) return;

            // 3. 执行 UI 更新逻辑
            float fillAmount = (float)args.NewValue / maxHp;
            headBar.UpdateHealthBar(fillAmount);
            
            // 也可以做其他表现，比如飘字
            if (args.NewValue < args.OldValue)
            {
                 // 掉血，飘红字
                 // DamageNumberManager.Instance.Spawn(unit.Position, args.NewValue - args.OldValue);
            }
        }
    }
}
```

## 4. 监听扩展示例 (MaxHP 也要监听)

通常血条变化不仅受 HP 影响，也受 MaxHP 影响。你可以再写一个 Watcher，或者同一个类监听多个类型（取决于框架具体版本支持情况，通常建议分开写以保持清晰）。

```csharp
namespace ET.Client
{
    [NumericWatcher(SceneType.StateSync, NumericType.MaxHp)]
    public class NumericWatcher_MaxHp_UpdateUI : INumericWatcher
    {
        public void Run(Unit unit, NumbericChange args)
        {
            // 获取 UI 并刷新 -> 逻辑同上，重新计算百分比
            unit.GetComponent<HeadBarComponent>()?.UpdateHealthBar();
        }
    }
}
```

## 5. 关键注意事项

1.  **程序集分离**：
    *   **Hotfix**：仅包含逻辑。
    *   **HotfixView**：包含 Unity 组件操作（如 `Transform`, `Slider` 等）。
    *   如果 Watcher 涉及 UI 更新，**必须**放在 `HOTFIX_VIEW` 相关程序集中。

2.  **线程安全**：
    *   `NumericWatcher` 是同步执行的。
    *   通常 UI 操作都在主线程执行，可以直接操作 Unity 对象。
    *   如果是 `async` 方法调用，要注意 await 之后的上下文。

3.  **循环触发陷阱**：
    *   **严禁**在 `NumericWatcher` 中再次修改 **同一个** 数值类型。
    *   例如：在 HP Watcher 中又调用 `Set(NumericType.Hp, ...)`。
    *   这会导致死循环栈溢出（Set -> Watcher -> Set -> Watcher...）。

4.  **SceneType 匹配**：
    *   如果 Watcher 不触发，首先检查 `SceneType` 是否匹配。
    *   确认 Unit 所在的 Scene 类型是 `Current`, `StateSync`, 还是 `Map` (Server)。
    *   客户端通常使用的是客户端场景类型。
