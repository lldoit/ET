---
trigger: always_on
---

# ET 9.0 核心开发底线规范

## 1. 基础开发原则
- **所有代码文件**必须创建在 `Packages/cn.etetet.*` 目录下
- **包命名规范**：`cn.etetet.{功能模块名}`
  - 核心包：`cn.etetet.core`
  - 功能包：`cn.etetet.mmo`、`cn.etetet.bag`、`cn.etetet.skill` 等
  - UI包：`cn.etetet.yiui*` 系列
- 每个package中都有packagegit.json文件，每个packagegit.json中的Id是项目唯一的
- 每个中都有Scripts/Model/Share/PackageType.cs文件，里面的编号就是packagegit.json中的Id

## 2. 架构设计规范
- **ECS 分离原则**：严格分离数据定义（Model/ModelView）与业务逻辑（Hotfix/HotfixView）。
- **Entity 纯粹性**：Entity 只能包含数据字段，**严禁**在 Entity 类中定义任何方法。
- **System 静态化**：System 类必须是 `static partial` 类，且所有方法必须是静态扩展方法。
- **Component**：Entity的组成部分，遵循组合优于继承
- **生命周期方法**：生命周期方法（如 Awake, Destroy）必须标记 `[EntitySystem]` 且声明为 `private static`。

## 3. 核心编程禁令
* **引用限制**：严禁直接声明 `Entity` 类型及其子类的字段或集合（如 List<Entity>），必须使用 `EntityRef<T>`。
* **Entity 管理限制**：Entity 只能管理其他 Entity 或 struct，禁止管理非 Entity 的 class。
* **命名空间与 Event**：在 `ET.Client` 命名空间下，严禁使用 `SceneType.Client`，应根据场景使用 `SceneType.StateSync` 或 `Current`。
* **异步安全红线**：在 `async/await` 路径中，await 之后禁止直接访问之前的 Entity 变量，必须通过 `EntityRef` 重新获取。

### EntityRef基本使用方法
```csharp
// Entity字段中使用EntityRef
public Dictionary<int, EntityRef<ProcessInfo>> ProcessDict { get; set; }

// 创建EntityRef引用
EntityRef<ProcessInfo> processRef = processInfo;

// 正确的Entity对象访问和检查方式
ProcessInfo entity = processRef;    // 直接赋值，不用.Entity
if (entity != null)
{
    // 安全使用Entity
}

// 错误方式
// var entity = processRef.Entity;  // 错误：不要用.Entity
// if (processRef.Entity != null) { /*  */ }    //错误：多次访问
```

### EntityRef在async/await环境下的使用规范（重要！）
**这是ET分析器的严格限制，必须遵循：**

```csharp
// ✅ 正确：await后使用Entity需要通过EntityRef重新获取
public static async ETTask ProcessUpdate(this UpdateCoordinatorComponent self, UpdateTask task)
{
    // 1. 在await前创建EntityRef引用
    EntityRef<UpdateCoordinatorComponent> selfRef = self;
    EntityRef<UpdateTask> taskRef = task;
    
    foreach (int processId in task.TargetProcessIds)
    {
        // 2. 在每次使用前通过EntityRef重新获取Entity
        task = taskRef;
        task.UpdateProgress(processId, "开始处理");
        
        // 3. await后需要重新获取所有Entity
        await SomeAsyncOperation();
        
        // 4. await后必须重新获取才能安全使用
        self = selfRef;
        task = taskRef;
        
        // 现在可以安全使用Entity
        task.UpdateProgress(processId, "处理完成");
    }
}
```

## 4. 命名与格式规范
- **命名空间**：
  - 共享代码：`namespace ET`
  - 客户端：`namespace ET.Client`
  - 服务器：`namespace ET.Server`
- **风格要求**：类名/方法名 PascalCase，字段名 camelCase，私有字段 _camelCase。
- **注释要求**：所有类、字段、方法必须包含详细的中文 `<summary>` 注释。

## 5. Module分析器规范
- Model，ModelView，Hotfix，HotfixView中的类可以指定模块
- Module(ModuleName.A)，这个加在类上，表示是属于A模块
- A模块调用B模块的方法，那么B模块就不能调用A模块的方法
- A模块不能访问B模块的字段
- ModuleName定义是partial，每个Package可以定义自己的Module
- 如果没有Module标签，那么该类属于Global模块，那么该类可以被其它Module调用，也可以调用其它Module

## 5. 程序集与包依赖
- 严格遵循 Model (不可热更数据)、Hotfix (可热更逻辑) 的分离原则。
- 遵循包层级依赖规范：仅允许高层包依赖低层包，禁止循环依赖。道具包等底层包严禁直接调用任务包等高层包，必须通过事件系统解耦。

## 6. 程序集分类规范
每个包必须支持以下四个程序集分类：

### 1. Model 程序集 (`Scripts/Model/`)
- **用途**：服务器和客户端共享的模型层
- **内容**：Entity定义、配置数据、共享逻辑
- **特点**：不可热更新，稳定性高

### 2. ModelView 程序集 (`Scripts/ModelView/`)  
- **用途**：客户端专用的视图模型层
- **内容**：UI相关Entity、客户端专用组件
- **特点**：不可热更新，UI底层支持

### 3. Hotfix 程序集 (`Scripts/Hotfix/`)
- **用途**：服务器和客户端共享的热更新逻辑层
- **内容**：System类、业务逻辑实现
- **特点**：可热更新，核心业务逻辑

### 4. HotfixView 程序集 (`Scripts/HotfixView/`)
- **用途**：客户端专用的热更新视图层
- **内容**：UI System类、客户端显示逻辑
- **特点**：可热更新，UI业务逻辑

## 7. 常见错误避免
1. ❌Entity中定义方法
2. ❌System忘记加特性
3. ❌生命周期方法不是private static
4. ❌忘记实现IAwake接口
5. ❌消息类字段使用camelCase（应该用PascalCase）
6. ❌Entity字段不完整（缺少System中使用的字段）
7. ❌重复定义相同的类（检查是否已存在）
8. ❌HTTP消息类字段名与使用处不匹配
9. ❌直接存储Entity引用（应该用EntityRef）
10. ❌使用EntityRef.Entity属性访问（应该直接赋值）
11. ❌不检查Entity的IsDisposed状态
12. ❌将EntityRef当作Entity直接使用
13. ❌await后直接使用Entity（违反ET分析器规则）
14. ❌[StaticField]，静态字段容易导致多线程问题，应该尽量避免使用，如果要使用，必须需要我手动确认

## 8. 代码质量规范

### 注释规范
```csharp
/// <summary>
/// 类的详细中文描述
/// 说明功能、用途和注意事项
/// </summary>
public class ExampleComponent : Entity, IAwake
{
    /// <summary>
    /// 字段的中文描述
    /// </summary>
    public int Value;
}

/// <summary>
/// 方法的详细中文描述
/// </summary>
/// <param name="self">当前组件实例</param>
/// <param name="value">参数的中文描述</param>
/// <returns>返回值的中文描述</returns>
public static bool DoSomething(this ExampleComponent self, int value)
{
    // 重要逻辑的中文注释
    return true;
}
```

### 编码风格
```csharp
// 命名规范
public class PlayerComponent        // 类名：PascalCase
public static void GetItem()       // 方法名：PascalCase  
public string playerName;          // 字段名：camelCase
private string _internalField;     // 私有字段：_camelCase
public const int MAX_PLAYERS = 100; // 常量：UPPER_SNAKE_CASE

// 代码格式
if (condition) {                   // 左大括号同行
    // 4空格缩进
}                                  // 右大括号单独一行
```

## 9. 包的依赖规范
1. 依赖的配置在包的package.json中
2. 包之间不能相互依赖，只能单项依赖
3. 包中只能访问自己包或者依赖包的符号
4. 目前各包的层级关系如下：

  第5层
  ├── cn.etetet.login         (登录系统)          AllowSameLevelAccess
  
  第4层
  ├── cn.etetet.actorlocation (location消息系统) 依赖netinner
  ├── cn.etetet.aoi           (数值系统) 依赖unit，numeric
  ├── cn.etetet.ai            (AI系统) 依赖unit，behaviortree

  第3层
  ├── cn.etetet.numeric       (数值系统) 依赖unit
  ├── cn.etetet.move          (移动系统) 依赖unit
  ├── cn.etetet.recast        (寻路系统) 依赖unit
  ├── cn.etetet.netinner      (内网消息系统) 依赖startconfig
  ├── cn.etetet.router        (软路由系统) 依赖startconfig，http
  ├── cn.etetet.watcher       (watcher系统) 依赖console，startconfig

  第2层
  ├── cn.etetet.unit          (单位系统)
  ├── cn.etetet.http          (http系统)
  ├── cn.etetet.startconfig   (服务器配置系统)
  ├── cn.etetet.yooassets     (资源加载系统)
  ├── cn.etetet.yiuiframework (yiuiframework)
  ├── cn.etetet.yiuiinvoke    (yiuiinvoke)
  ├── cn.etetet.yiuigm        (yiuigm)
  ├── cn.etetet.yiuiloopscrollrectasync (yiuiloopscrollrectasync)
  ├── cn.etetet.yiuiyooassets (yiuiyooassets)
  ├── cn.etetet.yiui          (yiui)
  ├── cn.etetet.yiuireddot    (yiuireddot)
  ├── cn.etetet.yiuitips      (yiuitips)
  ├── cn.etetet.yiui3ddisplay (yiui3ddisplay)
  ├── cn.etetet.yiuieffect    (yiuieffect)

  第1层
  ├── cn.etetet.core          (核心框架)
  ├── cn.etetet.excel         (协议定义)
  ├── cn.etetet.proto         (协议定义)
  ├── cn.etetet.loader        (加载器)
  
1. 请注意要递归依赖，修改依赖的时候要把依赖的依赖，全部递归加上去
2. 包的依赖关系直接读取所有包的package.json
3. 刷新包的时候请根据第6点中的层级关系，以及每个包后面说明的依赖包，来配置package.json，不在层级关系中的包不用处理，清理多余的依赖，比如已经递归依赖了，就不需要再加上直接依赖了
4. 请不要读package-lock.json
5. 通常只能高层包依赖低层包，但是假如A包的packagegit.json中加了"AllowSameLevelAccess": true，那么允许没有被A包依赖的同层包访问
6. 假如A包依赖了B包，那么B包永远不能访问A包，这样可以强制处理逻辑相互依赖的问题，比如任务包依赖道具包，那么道具包永远不能直接调用任务包中的方法，可以抛出事件，任务包订阅道具包的事件

## 10. AI开发助手使用指南

### 推荐的AI提示词模板
```
请帮我在ET框架中创建一个[功能描述]的Entity和对应的System：
- Entity名称：[EntityName]Component
- 主要功能：[详细功能描述]
- 父级Entity：[ParentEntityType]（如果有）
- 生命周期需求：[IAwake, IDestroy等]
- 程序集位置：[Model/ModelView]
- 命名空间：[ET/ET.Client/ET.Server]

请确保：
1. 严格遵循ET框架规范
2. 添加详细的中文注释
3. 正确使用特性标签
4. 实现必要的生命周期方法
```

### 代码审查检查清单
- [ ] Entity类继承Entity并实现IAwake
- [ ] Entity类不包含任何方法
- [ ] System类是静态partial类
- [ ] System类添加了正确的特性标签
- [ ] 实现了必要的生命周期方法
- [ ] 添加了详细的中文注释
- [ ] 文件放置在正确的目录和程序集中
- [ ] 使用了正确的命名空间
- [ ] 遵循了代码格式规范

## 10. 总结

严格遵循以上规范，确保：
1. **架构清晰**：Entity负责数据，System负责逻辑
2. **模块化**：功能按包组织，程序集合理分离
3. **可维护**：代码规范统一，注释详细
4. **可扩展**：遵循框架设计原则，易于扩展
5. **高质量**：充分的错误处理和性能优化
6. **代码一致性**：字段命名统一、Entity完整性、无重复定义
7. **EntityRef安全**：正确管理Entity引用，遵循async/await规范

这些规范是ET框架高效开发的基础，请AI严格遵循执行。