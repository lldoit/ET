# YIUI MCP 工具实现计划

> **给 agentic worker：** 必需子技能：使用 superpowers:executing-plans 按任务逐步实现本计划。步骤使用 checkbox（`- [ ]`）语法跟踪。

**目标：** 新增项目专用 MCP 工具，让 AI 能通过 Unity Editor API 创建和修改 YIUI prefab。

**架构：** 实现一个可复用的 Editor service 处理 YIUI prefab 操作，并通过 CoplayDev `McpForUnityTool` adapter 暴露。官方 Unity MCP 支持保留为后续基于同一 service 的薄 adapter。

**技术栈：** Unity Editor C#、YIUIFramework editor APIs、CoplayDev MCP for Unity、Newtonsoft `JObject`、Unity `PrefabUtility`、`AssetDatabase`。

---

### 任务 1：Editor Service 与结果类型

**文件：**
- 创建：`Packages/cn.etetet.yiuistatesync/Editor/McpTools/YIUI/YiuiMcpResult.cs`
- 创建：`Packages/cn.etetet.yiuistatesync/Editor/McpTools/YIUI/YiuiPrefabToolService.cs`

- [ ] 创建 `YiuiMcpResult`，包含 `Success`、`Message`、`Data`、`Ok(...)` 和 `Fail(...)`。
- [ ] 创建 `YiuiPrefabToolService`，包含 `CreateYIUIPanel`、`AddYIUIButton`、`BindYIUIEvent`、`GenerateYIUICode` 和 `OpenPrefabAndCapturePreview` 方法。
- [ ] 使用 `PrefabUtility.LoadPrefabContents` 和 `PrefabUtility.SaveAsPrefabAsset` 编辑 prefab。
- [ ] 使用 `UIBindEventTable.EditorAddEvent(UIBindEventTable.EUITaskEventType.Async, eventName)` 和 `UIEventBind.EditorAddBind(...)` 设置事件。
- [ ] 复用 `Packages/cn.etetet.yiuiframework/Editor/TemplatePrefabs/YIUI/YIUIButton.prefab` 作为按钮模板。

### 任务 2：CoplayDev MCP Adapter

**文件：**
- 创建：`Packages/cn.etetet.yiuistatesync/Editor/McpTools/YIUI/YiuiMcpTool.cs`

- [ ] 添加 `[McpForUnityTool("yiui_prefab", Description = "...", Group = "ui")]`。
- [ ] 实现 public static `HandleCommand(JObject @params)`。
- [ ] 分发支持的 actions：`ping`、`create_panel`、`add_button`、`bind_event`、`generate_code`、`open_preview`。
- [ ] 从 `YiuiMcpResult` 返回结构化结果。
- [ ] 将校验失败转换为错误结果，而不是未捕获异常。

### 任务 3：Assembly 引用

**文件：**
- 修改：`Packages/cn.etetet.yiuistatesync/Editor/ET.YIUI.StateSync.Editor.asmdef`

- [ ] 添加对 `ET.YIUIFramework`、`ET.YIUIFramework.Editor` 和 `MCPForUnity.Editor` 的引用。
- [ ] 保持该 assembly 仅用于 Editor。

### 任务 4：验证

**文件：**
- 除非编译错误暴露缺失引用，否则不修改生产文件。

- [ ] 通过可用的 MCP/Unity flow 运行 Unity 编译。
- [ ] 使用 `{"action":"ping"}` 调用 `yiui_prefab`。
- [ ] 使用 `create_panel` 创建临时 panel。
- [ ] 添加按钮并绑定事件。
- [ ] 为临时 prefab 生成 YIUI 代码。
- [ ] 捕获预览图。
- [ ] 报告所有无法在本地运行的验证。

### 自检

- Spec 覆盖：已覆盖请求的五个工具。
- 类型一致性：service 方法名与请求的 API 名称一致。
- 范围：官方 MCP adapter 明确延后，但当前 service 边界支持后续接入。
- 没有剩余占位任务。
