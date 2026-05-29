# 实施清单

## 1. 迁移前检查

- [x] 确认目标工作区当前状态，记录无关未跟踪文件，避免混入本次改动。
- [x] 再次确认 UGF10 中不存在 `Packages/cn.etetet.yiuilocalization`。
- [x] 确认 `packagegit.json` 编号 `1305` 未冲突。

## 2. 复制源包

- [x] 从 `/Users/lilei/Work/UGF_MessagePack/Packages/cn.etetet.yiuilocalization` 复制完整目录到 `Packages/cn.etetet.yiuilocalization`。
- [x] 保留源包 `.meta` 文件，不手工生成新的 `.meta`。
- [x] 删除复制过程中产生的系统临时文件，例如 `.DS_Store`，若存在。

## 3. 包规范适配

- [x] 新增 `Packages/cn.etetet.yiuilocalization/AGENTS.md`，说明包职责、依赖边界、验证方式。
- [x] 更新 `Packages/cn.etetet.yiuilocalization/package.json` 的 dependencies：
  - `cn.etetet.core`
  - `cn.etetet.yiuiframework`
  - `cn.etetet.yiuiinvoke`
- [x] 保持 `packagegit.json` 的 `Id=1305` 不变。

## 4. 代码兼容检查

- [x] 检查迁移包内对目标仓库不存在类型或 API 的引用。
- [x] 检查 `I2LocalizeMgr` 的 `ETTask` 与 `await` 后实体访问路径。
- [x] 若编译失败，只做最小兼容改动，不重构源包结构。

## 5. 验证

- [x] 运行 `git diff --check`。
- [x] 运行 `dotnet build ET.sln`。
- [x] 汇总新增/修改文件、验证结果和剩余风险。
