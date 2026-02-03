---
name: "ET-Expert"
description: "深度遵循 ET 9.0 规范，处理异步安全、Module 隔离、递归依赖管理及 YIUI 框架"
---

# 核心开发指令 (Instructions)

## 1. 终极异步安全流 (EntityRef Safety)
**AI 必须强制执行以下模板以通过分析器检查：**
* **Await 前**：必须创建 `EntityRef<T> selfRef = self;`。
* **Await 后**：必须通过 `self = selfRef;` 重新获取实体。
* **访问规范**：严禁访问 `.Entity` 属性，必须采用直接赋值并检查 `self != null`。

## 2. Module 分析器隔离规范
* **模块标记**：使用 `[Module(ModuleName.X)]` 标记类。
* **权限规则**：
    * A 模块调用 B，则 B 严禁反向调用 A。
    * A 模块禁止访问 B 模块的私有字段。
    * 无标签类属于 Global 模块，可双向访问。

## 3. 包依赖递归管理
**包层级定义（从低层 L1 到高层 L5）：**
* **L1 (基础设施)**: `core`, `excel`, `proto`, `loader`。
* **L2 (支持层)**: `unit`, `http`, `startconfig`, `yooassets`, `yiuiframework` 系列。
* **L3-4 (系统层)**: `numeric`, `move`, `recast`, `netinner`, `router`, `watcher`, `aoi`, `ai`。
* **L5 (业务层)**: `login`。
* **递归规则**：修改依赖时必须包含依赖包的所有下游依赖。禁止包之间相互依赖，底层包（如道具）严禁调用高层包（如任务），必须通过事件系统解耦。

## 4. UI (YIUI) 与对象池规范
* **YIUI 接口**：UI Entity 必须实现 `IYIUIBind`, `IYIUIInitialize`, `IYIUIOpen`, `IYIUIClose`。
* **对象池**：频繁创建的对象通过 `ObjectPool.Fetch<T>()` 获取，使用完毕后通过 `Dispose()` 或 `ObjectPool.Return()` 归还。

## 5. 文件组织结构规范

### 标准包结构
```
Packages/cn.etetet.{包名}/
├── packagegit.json                    # 包配置文件
├── Editor/                            # 编辑器代码
├── Runtime/                           # 运行时代码
└── Scripts/
    ├── Model/                         # Model程序集
    │   ├── Share/                     # 共享代码
    │   ├── Client/                    # 客户端专用
    │   └── Server/                    # 服务器专用
    ├── ModelView/                     # ModelView程序集
    │   └── Client/                    # 客户端视图模型
    ├── Hotfix/                        # Hotfix程序集
    │   ├── Share/                     # 共享逻辑
    │   ├── Client/                    # 客户端逻辑
    │   └── Server/                    # 服务器逻辑
    └── HotfixView/                    # HotfixView程序集
        └── Client/                    # 客户端视图逻辑
```

### 文件命名规范
- Entity文件：`{功能名}Component.cs`
- System文件：`{功能名}ComponentSystem.cs`  
- 配置文件：`{功能名}Config.cs`
- 协议文件：`{协议名}.proto`

## 6. 网络协议规范

### 协议命名规范
- 请求协议：`C2X_` 开头（客户端到服务器）
- 响应协议：`X2C_` 开头（服务器到客户端）
- 服务器间：`G2M_`、`M2G_` 等（网关到地图等）

### 协议文件组织
```
Packages/cn.etetet.proto/
├── Proto/
│   ├── Common/          # 通用协议
│   ├── Login/           # 登录协议
│   ├── Game/            # 游戏协议
│   └── Battle/          # 战斗协议
└── Scripts/
    └── Model/
        └── Generate/     # 自动生成的协议代码
```

# 标准实现模板

## Entity 开发规范

### Entity 类定义规范
```csharp
// 位置：Packages/cn.etetet.{包名}/Scripts/Model/ 或 Scripts/ModelView/
namespace ET  // 或 ET.Client, ET.Server
{
    public class AA // 错误，逻辑类必须继承Entity
    {
    }
    
    public class BB : Entity // 正确
    {
    }

    /// <summary>
    /// 详细的中文描述
    /// </summary>
    [ComponentOf(typeof(ParentEntityType))]  // 指定父实体类型（如适用）
    public class ExampleComponent : Entity, IAwake, IDestroy
    {
        // 只包含数据字段，不包含方法
        public int SomeValue;
        public string SomeName;
        public List<int> SomeList;
    }
}
```

### Entity 类要求
- **必须**继承 `Entity` 基类
- **必须**实现 `IAwake` 接口（生命周期接口）
- **根据需要**实现其他接口：`IDestroy`、`IUpdate`、`ISerialize` 等
- **严禁**在Entity类中定义任何方法
- **必须**添加 `[ComponentOf]` 或 `[ChildOf]` 特性指定父级约束
- **Entity只能管理Entity跟struct，不允许管理非Entity class**

### 生命周期接口规范
```csharp
// 基础生命周期
public interface IAwake { }                          // 初始化
public interface IAwake<A> { }                       // 带参数初始化
public interface IDestroy { }                        // 销毁
public interface IUpdate : IClassEvent<UpdateEvent> { } // 更新
public interface ISerialize { }                     // 序列化前
public interface IDeserialize { }                   // 反序列化后

// 异步生命周期
public interface IAwakeAsync { }
public interface IAwakeAsync<A> { }
```

## System 开发规范

### System 类定义规范
```csharp
// 位置：Packages/cn.etetet.{包名}/Scripts/Hotfix/ 或 Scripts/HotfixView/
namespace ET  // 或 ET.Client, ET.Server
{
    /// <summary>
    /// 详细的中文描述
    /// </summary>
    [FriendOf(typeof(ExampleComponent))]           // 访问友元Entity
    [EntitySystemOf(typeof(ExampleComponent))]     // 指定对应的Entity类型
    public static partial class ExampleComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this ExampleComponent self)
        {
            // 初始化逻辑
        }

        [EntitySystem]
        private static void Awake(this ExampleComponent self, int value)
        {
            // 带参数的初始化逻辑
        }

        [EntitySystem]
        private static void Destroy(this ExampleComponent self)
        {
            // 销毁清理逻辑
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 业务方法的中文描述
        /// </summary>
        public static void DoSomething(this ExampleComponent self, int param)
        {
            // 业务逻辑实现
        }

        #endregion
    }
}
```

### System 类要求
- **必须**是静态类（`static`）
- **必须**包含 `partial` 关键字
- **必须**添加 `[EntitySystemOf(typeof(对应Entity类))]` 特性
- **必须**实现对应Entity的 `Awake` 生命周期函数
- **必须**添加 `[FriendOf(typeof(Entity类))]` 特性访问Entity字段
- **所有方法**必须是静态扩展方法
- **生命周期方法**必须添加 `[EntitySystem]` 特性并声明为 `private static`

## Client命名空间下Event开发规范

### EventHandler 类定义规范
```csharp
namespace ET.Client
{
    [Event(SceneType.Client)]   // 错误, ET.Client命名空间下不能使用SceneType.Client
    [Event(SceneType.StateSync)]  // 正确，客户端启动后创建的Scene，通常用于各种不需要单独创建Scene的系统服务器数据同步
    [Event(SceneType.Current)]  // 正确，当前Scene，通常用于战斗场景和三消
    public class ExampleEventHandler : AEvent<ExampleEvent>
    {
    }
}
```

## 消息规范

### 客户端发送消息到服务器
```csharp
// Send不需要等待返回
C2M_TestRobotCase1 message1 = C2M_TestRobotCase1.Create();
fiber.Root.GetComponent<ClientSenderComponent>().Send(message1);

// Call，可以等待返回值
C2M_TestRobotCase2 message2 = C2M_TestRobotCase2.Create();
var response = await fiber.Root.GetComponent<ClientSenderComponent>().Call(message2);
```

### 消息管理
- 消息一般不需要使用对象池，也不需要调用消息的Dispose方法
- 如果要优化，可以让用户自己优化

## 特性使用规范

### 核心特性说明
```csharp
// Entity组件约束
[ComponentOf(typeof(ParentType))]  // 指定唯一父级类型
[ComponentOf]                      // 允许多种父级类型
[ChildOf(typeof(ParentType))]      // 指定子实体父级类型
[ChildOf]                          // 允许多种父级类型

// System相关特性
[EntitySystemOf(typeof(EntityType))]        // 标记System对应的Entity
[EntitySystem]                               // 标记生命周期方法
[FriendOf(typeof(EntityType))]              // 允许访问Entity私有字段

// 生成器相关特性
[EnableAccessEntiyChildAttribute]           // 允许访问Entity的child和component
[EnableMethodAttribute]                      // 启用方法增强
```

## 异步安全与异常处理模板
```csharp
[FriendOf(typeof(MyComponent))]
[EntitySystemOf(typeof(MyComponent))]
public static partial class MyComponentSystem
{
    // 使用ETTask代替Task
    public static async ETTask DoAsyncWork(this MyComponent self)
    {
        await ETTask.CompletedTask;
    }

    // 带返回值的异步方法
    public static async ETTask<bool> TryDoWork(this MyComponent self)
    {
        await SomeAsyncOperation();
        return true;
    }

    /// <summary>
    /// 处理异步更新的逻辑示例
    /// </summary>
    public static async ETTask ProcessAsync(this MyComponent self)
    {
        EntityRef<MyComponent> selfRef = self; // Await前创建引用
        try 
        {
            await SomeAsyncOperation();
        }
        catch (Exception e)
        {
            Log.Error($"异步操作失败: {e}"); // 记录异常
        }
        finally
        {
            self = selfRef; // Await后必须重新获取
            if (self != null) { /* 业务逻辑实现 */ }
        }
    }
}
```

## UI框架 (YIUI) 规范

### YIUI Entity规范
```csharp
// UI Entity必须继承特定接口
public class ExamplePanelComponent : Entity, IAwake, IDestroy,
    IYIUIBind,          // UI绑定
    IYIUIInitialize,    // UI初始化  
    IYIUIOpen,          // UI打开
    IYIUIClose          // UI关闭
{
    // UI相关数据字段
}
```

### YIUI System规范
```csharp
[FriendOf(typeof(ExamplePanelComponent))]
[EntitySystemOf(typeof(ExamplePanelComponent))]
public static partial class ExamplePanelComponentSystem
{
    [EntitySystem]
    private static void YIUIBind(this ExamplePanelComponent self)
    {
        // UI组件绑定逻辑
    }

    [EntitySystem]
    private static void YIUIInitialize(this ExamplePanelComponent self)
    {
        // UI初始化逻辑
    }

    [EntitySystem]
    private static async ETTask<bool> YIUIOpen(this ExamplePanelComponent self)
    {
        // UI打开逻辑
        return true;
    }

    [EntitySystem]
    private static async ETTask<bool> YIUIClose(this ExamplePanelComponent self)
    {
        // UI关闭逻辑
        return true;
    }
}
```

## 配置数据规范

### 配置类定义
```csharp
// 单个配置项
[Config]
public class ExampleConfig : AConfig
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
}

// 配置容器
[ConfigCategory]
public class ExampleConfigCategory : AConfigCategory<ExampleConfig>
{
    public static ExampleConfigCategory Instance => ConfigComponent.Instance.GetCategory<ExampleConfigCategory>();
}
```

## 错误处理和日志规范

### 日志使用
```csharp
// 日志级别选择
Log.Debug("调试信息，仅开发时使用");
Log.Info("重要的业务信息");
Log.Warning("可能的问题，但不影响运行");
Log.Error("错误信息，需要关注");
Log.Error(exception); // 异常对象

// 日志格式
Log.Info($"玩家{playerId}完成了任务{taskId}");
```

### 异常处理
```csharp
public static bool TryDoSomething(this ExampleComponent self)
{
    try
    {
        // 可能抛出异常的代码
        return true;
    }
    catch (Exception e)
    {
        Log.Error($"操作失败: {e}");
        return false;
    }
    finally
    {
        // 清理代码
    }
}
```

## 性能优化规范

### 对象池使用
```csharp
// 从对象池获取对象
var obj = ObjectPool.Fetch<SomeClass>();

// 使用完毕归还对象池
obj.Dispose(); // 如果实现了IDisposable
// 或
ObjectPool.Return(obj);
```

### 内存管理
```csharp
// 避免频繁装箱
public static void ProcessValue(this ExampleComponent self, object value)
{
    // 错误：会产生装箱
    // if (value.Equals(0)) { }
    
    // 正确：使用泛型避免装箱
    if (value is int intValue && intValue == 0) { }
}
```

## 常见问题和解决方案

### 1. Entity字段访问权限问题
```csharp
// 问题：无法访问Entity的private字段
// 解决：在System类上添加FriendOf特性
[FriendOf(typeof(ExampleComponent))]
public static partial class ExampleComponentSystem
{
    public static void AccessField(this ExampleComponent self)
    {
        self.privateField = 100; // 现在可以访问了
    }
}
```

### 2. 生命周期方法缺失
```csharp
// 问题：Entity没有对应的System生命周期方法
// 解决：确保System中实现了对应的生命周期方法
[EntitySystem]
private static void Awake(this ExampleComponent self)
{
    // 必须实现，即使是空方法
}
```

### 3. 程序集引用错误
```csharp
// 问题：无法访问其他程序集的类型
// 解决：检查packagegit.json中的ScriptsReferences配置
{
    "ScriptsReferences": {
        "Model": ["ET.Core"],
        "Hotfix": ["ET.Core", "ET.Model"],
        "ModelView": ["ET.Core", "ET.Model", "ET.YIUIFramework"],
        "HotfixView": ["ET.Core", "ET.Model", "ET.ModelView", "ET.Hotfix"]
    }
}
```