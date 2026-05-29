# 移植 YIUI 多语言模块

## 背景

UGF10 当前已包含 YIUI Framework、YIUI Invoke、YIUI YooAssets 等 UI 相关包，但缺少 `cn.etetet.yiuilocalization`。源工作区 `/Users/lilei/Work/UGF_MessagePack` 已有可用的 YIUI 多语言包，包含 I2 Localization 运行时、编辑器工具、YIUI 数据绑定与 HotfixView 语言切换事件桥接。

## 目标

- 将 `/Users/lilei/Work/UGF_MessagePack/Packages/cn.etetet.yiuilocalization` 移植到 UGF10 的 `Packages/cn.etetet.yiuilocalization`。
- 保留源包 Unity `.meta`，维持资产 GUID 与 prefab/script 引用稳定。
- 补齐目标包规范文件与 package 依赖，使包边界符合 UGF10 package 规则。
- 通过 UGF10 唯一编译入口 `dotnet build ET.sln` 做基础验证。

## 非目标

- 不批量修改现有 UI prefab 文案或绑定多语言组件。
- 不新增业务翻译 key，不扩展语言表内容。
- 不调整 Unity PlayerSettings 全局宏，除非编译验证证明迁移必须修改。
- 不提交或处理当前工作区已有的无关未跟踪文件。

## 成功标准

- UGF10 中存在完整 `Packages/cn.etetet.yiuilocalization` 包。
- `packagegit.json` 的 `Id=1305` 在 UGF10 内无冲突。
- 包内依赖声明覆盖实际跨包使用，且不引入反向依赖。
- `dotnet build ET.sln` 成功，或若失败，失败原因被定位到与本次迁移无关或列出明确后续修复项。

## 风险

- 源包包含 Runtime、Editor、HotfixView 和资源文件，迁移范围较大，可能暴露 Unity 编译而非 dotnet 编译问题。
- 包内 `TextMeshPro` 相关代码依赖目标工程已有 `TextMeshPro` 宏与 Unity.TextMeshPro 包。
- 包内 `I2LocalizeMgr` 和 YIUI 绑定涉及 `ETTask`、YIUI Invoke 与资源加载释放链路，需要重点检查异步安全。
