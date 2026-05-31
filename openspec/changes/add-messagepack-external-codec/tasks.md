# 实施清单

## 1. 迁移前检查

- [x] 确认当前工作区状态，记录并排除无关未跟踪文件。
- [x] 确认 UGF10 中不存在 `Packages/cn.etetet.messagepack`。
- [x] 确认 `packagegit.json` 编号 `1503` 未冲突。
- [x] 检查源包是否包含不应迁入的临时构建产物。

## 2. 接入 MessagePack package

- [x] 从 `/Users/lilei/Work/UGF_MessagePack/Packages/cn.etetet.messagepack` 复制必要 package 文件到 `Packages/cn.etetet.messagepack`。
- [x] 保留源包 `.meta` 文件，不手工生成新的 `.meta`。
- [x] 排除 `DotNet~/obj`、临时缓存或其他无需入库产物，若源包确有服务端构建必需 DLL/工程文件则单独确认。
- [x] 新增 `Packages/cn.etetet.messagepack/AGENTS.md`，说明包职责和禁止修改上游源码的边界。
- [x] 更新 `Packages/packages-lock.json`，加入 embedded package 记录。

## 3. 新增外部 MessagePack codec/adaptor

- [x] 确认外部 codec 的 package 落点；若无现成业务包，新增独立 package。
- [x] 为 codec package 声明 `cn.etetet.messagepack` 依赖。
- [x] 新增最小 MessagePack codec 接口与实现。
- [x] 保持现有 `MessageSerializeHelper`、`NinoHelper`、`Proto2CS` 和 ET Proto 生成物不变。

## 4. 本地验证

- [x] 新增或使用最小 DTO 做 MessagePack serialize/deserialize roundtrip。
- [x] 覆盖 `byte[]` 路径；如实现支持 `MemoryBuffer`，同时覆盖 `MemoryBuffer` 路径。
- [x] 运行 `dotnet build ET.sln`。
- [x] 运行 `git diff --check`。

## 5. 收尾检查

- [x] 汇总新增/修改文件。
- [x] 确认未暂存或混入当前已有无关未跟踪文件。
- [x] 说明未覆盖真实服务器联调和登录测试是本次明确非目标。

## 6. 客户端网络 codec 切换

- [x] 在 `cn.etetet.core` 新增 `INetworkMessageCodec` 抽象。
- [x] 在 `cn.etetet.core` 新增默认 `NinoNetworkMessageCodec`，包装现有 `MessageSerializeHelper`。
- [x] 给 `Session` 增加 `NetworkMessageCodec` 字段或属性，默认使用 Nino。
- [x] 改造 `NetComponentSystem.OnRead`，从 session codec 解析消息。
- [x] 改造 `SessionSystem.Send`，从 session codec 打包消息。
- [x] 在 `cn.etetet.messagepacknet` 新增 `MessagePackNetworkMessageCodec`，复用 ET opcode framing，payload 使用 MessagePack。
- [x] 补充本地 codec roundtrip 验证，不做网络登录测试。
- [x] 运行 `dotnet build ET.sln` 与定向 MessagePack 测试。
