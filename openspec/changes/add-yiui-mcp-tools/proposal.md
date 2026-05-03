# 新增 YIUI MCP 工具

## 背景

AI 代理已经可以调用通用 Unity MCP 工具，但 YIUI 预制体包含框架专属状态，例如 `UIBindCDETable`、`UIBindEventTable`、`UIPanelSplitData`、Odin 序列化数据、生成的 ET 组件代码，以及 YIUI 事件绑定。直接编辑 YAML 不安全，通用 Unity 工具也不了解这些规则。

## 目标

提供一层小型项目专用工具，让 MCP client 通过 Unity Editor API 创建和更新 YIUI 预制体，而不是编辑 `.prefab` 文本。

## 成功标准

- MCP client 可以根据 package 和 panel name 创建 YIUI panel 预制体。
- MCP client 可以在目标路径下添加 YIUI 按钮，并绑定零参数 task event。
- MCP client 可以把已有对象绑定到 YIUI event。
- MCP client 可以触发某个预制体的 YIUI 代码生成。
- MCP client 可以打开预制体并捕获预览图路径，便于检查。
- 工具响应是结构化的，并包含 `success`、`message`，以及相关路径或校验错误。

## 非目标

- 不手写或手改 prefab YAML。
- 第一阶段不实现官方 Unity MCP adapter。
- 本次不增加大范围 UI 布局生成或视觉设计智能。
- 不改变现有 YIUI runtime 行为。

## 不做的风险

代理要么会避开 YIUI prefab 工作，要么会直接修改序列化 prefab。这可能破坏 Odin/YIUI 序列化数据，生成“能编译但运行时失败”的预制体。
