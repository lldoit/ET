# 接入 YIUI 多语言包

## 背景

项目需要制作多语言功能和多语言工具。现有 YIUI 包目录中已经预留 `cn.etetet.yiuilocalization` 多语言包入口，但当前工程未安装该包。

参考文章提出的完整工具链包含 Prefab、Excel、代码扫描，机器翻译，Excel 人工审核，以及运行时数据导出。当前项目不需要 Prefab 多语言绑定 Key 扫描，也不需要输出给策划/翻译审核的表格；第一步应先接入官方 YIUI 多语言运行时和基础 Editor 工具，避免重复实现已有能力。

## 目标

- 将官方 `cn.etetet.yiuilocalization` 包接入当前 Unity 工程。
- 保持当前项目 embedded package 管理方式一致。
- 让 Unity 能识别 `I2LocalizeMgr`、I2 Localization 运行时、YIUI 多语言绑定组件和基础 CSV 导入导出工具。
- 将 I2 Localization Editor 的 Google 翻译后端替换为百度翻译。

## 成功标准

- `Packages/manifest.json` 声明 `cn.etetet.yiuilocalization`。
- `Packages/cn.etetet.yiuilocalization` 作为 embedded package 存在。
- `Packages/packages-lock.json` 包含该 embedded package 条目。
- Unity 打开项目后可解析该包并进入编译流程。
- 不改变已有业务 UI、YIUI 状态同步、资源收集和启动逻辑。

## 非目标

- 本次不实现 Prefab 多语言绑定 Key 扫描。
- 本次不实现 Excel/C# 多语言 Key 扫描。
- 本次不改造运行时语言切换 UI。
- 本次不输出给策划/翻译审核的表格。
- 本次不新增独立机器翻译适配器，直接替换 I2 现有 Google 翻译后端。

## 不做的风险

继续缺失官方多语言包会导致后续工具没有稳定运行时落点，容易在翻译和导出工具中重复实现语言表、语言切换、UI 刷新等基础能力。
