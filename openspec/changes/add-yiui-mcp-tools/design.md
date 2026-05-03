# 新增 YIUI MCP 工具设计

## 架构

本变更新增一个 YIUI 业务服务层和一个薄 MCP adapter。

- `YiuiPrefabToolService` 负责所有 YIUI 专属 Editor 工作。它使用 `PrefabUtility`、`AssetDatabase`、`MenuItemYIUIPanelSource`、`UIBindEventTable`、`UIEventBind` 和 `UICreateModule`。
- `YiuiMcpTool` 暴露一个 CoplayDev MCP 命令，名称为 `yiui_prefab`，通过 `action` 参数区分动作。它把 JSON 输入转换为对 `YiuiPrefabToolService` 的调用。
- 后续可以用 `[McpTool]` 方法包装同一个 service 来增加官方 Unity MCP 支持。该 adapter 不应重复实现任何业务逻辑。

## 工具动作

### `create_panel`

输入：

- `packageName`：`Assets/GameRes/YIUI` 下的 YIUI package 文件夹。
- `panelName`：最终 prefab 名称，例如 `LobbyPanel`。

行为：

- 确保 `Assets/GameRes/YIUI/{packageName}/Source` 存在。
- 使用现有 YIUI source-panel factory 创建 panel source prefab。
- 将生成的 prefab 重命名为 `{panelName}.prefab`。
- 加载 prefab 内容，并确保 `UIBindCDETable` 具备 `PkgName`、`ResName`、`UICodeType`、`IsSplitData`、`AllViewParent` 和 `AllPopupViewParent`。

### `add_button`

输入：

- `prefabPath`
- `path`：prefab 内部用斜杠分隔的父级路径。空值表示 root。
- `objectName`
- `eventName`
- `text`

行为：

- 加载 prefab 内容。
- 根据路径找到父级节点。
- 在该父级节点下实例化 YIUI `YIUIButton` 模板。
- 重命名新对象。
- 当存在 `Text` 或 TMP 组件时设置子标签文本。
- 在 root `UIBindEventTable` 中创建或复用零参数 async event。
- 在按钮对象上添加或更新 `UITaskEventBindClick`。
- 保存 prefab 内容。

### `bind_event`

输入：

- `prefabPath`
- `objectName`
- `eventName`

行为：

- 加载 prefab 内容。
- 根据对象名称查找子孙节点。
- 在 root `UIBindEventTable` 中创建或复用零参数 async event。
- 在该对象上添加或更新 `UITaskEventBindClick`。
- 保存 prefab 内容。

### `generate_code`

输入：

- `prefabPath`

行为：

- 加载 prefab asset。
- 查找 root `UIBindCDETable`。
- 通过 `UICreateModule.CreatePackages` 运行现有 YIUI package 代码生成。
- 刷新 AssetDatabase。

### `open_preview`

输入：

- `prefabPath`

行为：

- 在 Prefab Mode 中打开 prefab asset。
- 在 `Temp/YIUIMcpPreviews` 下捕获 PNG 预览图。
- 返回截图路径。

## 错误处理

每个工具返回结构化结果，而不是对预期内的校验失败抛异常。它应报告 prefab 缺失、路径无效、root `UIBindCDETable` 缺失、`UIBindEventTable` 缺失、对象重复，以及不支持的 prefab 位置。非预期异常由 MCP adapter 捕获，并以错误结果返回。

## 验证

验证使用 Unity 编译加定向 MCP 工具 smoke test：

- 编译 editor scripts；
- 用 `ping` 调用 `yiui_prefab`；
- 在测试 package 文件夹中创建临时 panel；
- 添加按钮并绑定事件；
- 为临时 prefab 生成 YIUI 代码；
- 确认 Console 没有错误。
