# cn.etetet.yiuilocalization

## 包职责

- 提供 YIUI 多语言能力，包含 I2 Localization runtime、editor 工具、语言资源和 YIUI 数据绑定。
- 提供 HotfixView 语言切换事件桥接，使运行时语言切换可以通知 ET/YIUI 事件系统。

## 依赖边界

- 本包可依赖 `cn.etetet.core`、`cn.etetet.yiuiframework`、`cn.etetet.yiuiinvoke`。
- 本包不直接依赖 `cn.etetet.yiuiyooassets`，资源加载与释放通过 YIUI Invoke 抽象完成。
- 不要让低层包反向引用本包；需要多语言能力的业务包应显式声明对本包的依赖。

## 开发规则

- 保留 Unity `.meta` 文件，移动资产时同步移动 `.meta`，不要手工生成新的 `.meta`。
- 不手工修改 `.csproj`，需要工程文件更新时通过 Unity 刷新生成。
- 修改 `ETTask`、`await` 或 Entity 访问路径时，必须按项目 `et-async` 规则检查 await 后实体引用安全。
- Editor 工具可使用 UnityEditor API；Runtime 和 HotfixView 代码不要引入 Editor-only 类型。

## 验证

- 基础验证使用项目唯一编译入口：`dotnet build ET.sln`。
- 提交前运行 `git diff --check`。
