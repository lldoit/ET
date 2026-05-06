# 增加多语言扫描工具

## 背景

项目已接入 I2 多语言和百度批量翻译工具。后续需要在编辑器内快速检查配置表和代码里引用的多语言 Key，提前发现缺失 Term 或动态调用风险。

## 目标

- 在 YIUI 多语言工具中增加配置表 `i18n` 列扫描入口。
- 在 YIUI 多语言工具中增加代码多语言 API 调用扫描入口。
- 扫描结果输出 Unity Console 摘要和明细，便于开发定位。
- 扫描缺失 Key 时只报告问题，不自动修改 I2 数据源。

## 非目标

- 不恢复 Prefab 多语言绑定 Key 扫描。
- 不输出策划/翻译审核表格。
- 不自动新增 Term 或自动翻译。
- 不修改 Excel 导表器。

## 成功标准

- Unity 编译无错误。
- 配置表扫描能读取各包 `Excel/*.xlsx` 中标记为 `i18n` 的列。
- 代码扫描能统计 `LocalizationManager.GetTranslation/TryGetTranslation` 字符串字面量调用。
- 缺失 Key 能在 Console 中输出文件、行号或表格位置。
