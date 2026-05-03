# 新增 YIUI MCP 工具任务

- [ ] 新增一个聚焦 YIUI prefab 操作的 Editor service。
- [ ] 新增一个名为 `yiui_prefab` 的 CoplayDev MCP adapter 命令。
- [ ] 更新 StateSync editor asmdef，引用 `ET.YIUIFramework`、`ET.YIUIFramework.Editor` 和 `MCPForUnity.Editor`。
- [ ] 新增结构化结果模型，保证 MCP 响应可预测。
- [ ] 在 Unity 中验证编译。
- [ ] 对 `ping`、`create_panel`、`add_button`、`bind_event`、`generate_code` 和 `open_preview` 做 smoke test。
- [ ] 记录官方 Unity MCP adapter 后续应复用同一个 service。
