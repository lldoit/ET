# 接入 YIUI 多语言包设计

## 技术选择

采用官方 `cn.etetet.yiuilocalization` 包作为运行时底座。该包基于 I2 Localization，并提供 YIUI 适配层：

- `I2LocalizeMgr`：语言管理入口，负责默认语言、运行时 CSV 加载、语言切换和事件广播。
- `UIDataBindTextI2Base` / `UIDataBindTextI2TMP` / `UIDataBindI2Text`：YIUI 文本数据绑定组件。
- `UII2LocalizationModule`：YIUI Auto Tool 中的基础多语言 CSV 导入导出模块。
- `I2LocalizationSourceManager.prefab`：运行时管理器 Prefab。

## 接入方式

当前工程多数 `cn.etetet.*` 包都以 `Packages/<package-name>` embedded package 方式存在，`packages-lock.json` 也解析为 `source: embedded`。因此本次将 `cn.etetet.yiuilocalization` 作为 embedded package 放入 `Packages/cn.etetet.yiuilocalization`，并在 `Packages/manifest.json` 中声明版本 `3.0.0`。

## 资源与数据

官方包自带示例数据：

- `Assets/Editor/I2Localization/I2Languages.asset`
- `Assets/Editor/I2Localization/I2_AllSource.csv`
- `Assets/GameRes/I2Localization/I2_Chinese.csv`
- `Assets/GameRes/I2Localization/I2_English.csv`

这些数据先保留为包内默认示例和后续工具验证入口。资源收集、语言资源热更、真实业务语言表迁移不在本次范围内。

## 风险

- 包声明 Unity `2022.3`，当前项目是 Unity 6，需要 Unity 编译验证。
- 包内包含完整 I2 Localization Editor/Runtime，体量较大，后续如果出现编译冲突，应优先通过 asmdef/define 约束处理。
- 包的工具目前是 CSV 导入导出，不包含配置表/代码扫描能力。

## 百度翻译替换

I2 Localization 现有术语翻译和语言批量翻译入口均调用 `GoogleTranslation`。为降低改动面，本次保留 I2 Editor UI 和调用点，把 `GoogleTranslation` 的网络后端切换为百度翻译：

- `GoogleTranslation.CanTranslate()` 改为检查百度翻译 App ID 和 Secret Key。
- `TranslationJob_Main` 直接使用百度翻译 Job，不再调用 I2 Google WebService 的 POST/GET Job。
- 百度翻译配置只保存在本机 `EditorPrefs`，不写入源码、资源或语言表。
- YIUI Auto Tool 的多语言模块显示百度翻译配置入口。
- 语言代码从 I2/Google 的国际化代码映射到百度翻译语言代码，例如 `zh-CN -> zh`、`zh-TW -> cht`、`en -> en`、`ja -> jp`、`ko -> kor`。

## 范围收敛

本次只接入官方多语言包，并把 I2 现有 Google 翻译后端替换为百度翻译。不新增独立机器翻译适配器，不实现 Prefab Key 扫描，也不输出策划/翻译审核表格。
