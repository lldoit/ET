# 接入 YIUI 多语言包任务

- [x] 拉取并检查官方 `cn.etetet.yiuilocalization` 仓库结构。
- [x] 确认当前工程采用 embedded package 管理方式。
- [x] 将官方包接入 `Packages/cn.etetet.yiuilocalization`。
- [x] 更新 `Packages/manifest.json`，声明 `cn.etetet.yiuilocalization` 依赖。
- [x] 更新 `Packages/packages-lock.json`，声明 embedded package 条目。
- [x] 打开 Unity，验证 Package Manager 解析和脚本编译。
- [x] 在 Unity 中确认 YIUI Auto Tool 出现多语言模块。
- [x] 在 Unity 中确认 `I2LocalizationSourceManager.prefab` 可被识别。
- [x] 将 I2 `GoogleTranslation` 后端替换为百度翻译。
- [x] 在 YIUI 多语言工具中增加百度翻译本机配置入口。
- [x] 验证百度翻译签名、语言码映射和 Unity 编译。
