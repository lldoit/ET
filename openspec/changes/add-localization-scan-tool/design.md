# 多语言扫描工具设计

## 方案

新增 Editor 侧扫描工具，挂在现有 `UII2LocalizationModule` 面板下。扫描逻辑拆成配置表扫描、代码扫描、通用报告和轻量 `.xlsx` 读取器，避免继续膨胀面板类。

## 配置表扫描

- 扫描 `Assets` 和 `Packages` 下路径包含 `/Excel/` 的 `.xlsx` 文件。
- 跳过临时文件 `~$` 和文件名包含 `#` 的表。
- 按当前导表器约定读取表头：
  - 第 2 行：字段客户端/服务端标记。
  - 第 3 行：字段描述。
  - 第 4 行：字段名。
  - 第 5 行：字段类型。
  - 第 6 行开始：数据。
- 字段类型包含 `i18n` 时视为多语言 Key 列。
- 扫描数据行时跳过前缀列包含 `#` 的行、空 Id 行和空 Key。
- Key 不存在于 I2 Editor 语言源时输出缺失问题。

## 代码扫描

- 扫描 `Assets` 和 `Packages` 下 `.cs` 文件，排除 `cn.etetet.yiuilocalization` 包自身，避免 I2 框架内部调用污染项目结果。
- 识别 `LocalizationManager.GetTranslation("key")` 和 `LocalizationManager.TryGetTranslation("key", ...)`。
- 字符串字面量 Key 不存在时输出缺失问题。
- 非字符串字面量调用统计为动态调用，仅提醒人工关注，不做缺失判断。

## 输出

- 提示框只显示摘要。
- Console 输出问题明细，包含文件路径、行号或 Excel 表格位置。
- 不生成外部审核表格。

## 边界

本次扫描工具以静态扫描为主，不做 Roslyn 语义分析。后续如果代码中大量使用封装 API 或动态 Key，再补充白名单和项目封装 API 识别。
