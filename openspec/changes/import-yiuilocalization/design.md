# 设计

## 接入方式

采用整包移植方式，将源包目录完整复制到 UGF10 的 `Packages/cn.etetet.yiuilocalization`。该包是独立 Unity package，不拆分 Runtime、Editor 或 YIUI 绑定代码，避免破坏源包内部资产路径、`.asmref`、prefab 和 `.meta` 引用关系。

## 包边界

目标包保持 `packagegit.json`：

```json
{
  "Id": 1305,
  "Name": "YIUI.Localization"
}
```

当前 UGF10 包编号检查显示 `1305` 未被使用，因此不需要重新编号。

`package.json` 需要从源包的空依赖调整为显式依赖：

- `cn.etetet.core`：HotfixView invoke 与运行时 ET 类型依赖。
- `cn.etetet.yiuiframework`：YIUI 单例、数据绑定、常量、资源加载接口依赖。
- `cn.etetet.yiuiinvoke`：`YIUIInvokeEntity*` 与 Invoke handler 依赖。

`cn.etetet.yiuiyooassets` 不作为强依赖声明，因为本包通过 YIUI Invoke 抽象加载与释放资源，不直接引用 YooAssets 包内类型。

## 代码与程序集

源包包含：

- `Runtime/`：I2 Localization runtime、YIUI 数据绑定、语言管理。
- `Editor/`：I2 编辑器、YIUIAutoTool 多语言扫描与百度翻译工具。
- `Scripts/HotfixView/Client/`：语言切换事件的 HotfixView invoke 桥接。
- `Assets/`：I2 language source 和 CSV 资源。

迁移时保留 `Ignore.ET.YIUI.Localization.asmdef`、`YIUIAssemblyReference.asmref` 和 HotfixView `AssemblyReference.asmref`。不手工修改 `.csproj`，如需工程文件更新由 Unity 刷新生成。

## 异步安全

`I2LocalizeMgr` 中存在异步资源加载逻辑。迁移本身不改其行为，但需要检查：

- 是否在 `await` 后继续访问 `Entity`。
- 若存在访问，是否已有 `EntityRef<T>` 或无需长期实体引用的证明。
- 若目标仓库 API 与源仓库不一致导致编译失败，再做最小兼容调整。

## 验证

基础验证使用项目唯一编译入口：

```powershell
dotnet build ET.sln
```

同时运行：

```powershell
git diff --check
```

如果 dotnet 编译无法覆盖 Unity package Runtime/Editor 编译面，再记录需要 UnityBridge 或 Unity Editor 刷新验证的后续项。
