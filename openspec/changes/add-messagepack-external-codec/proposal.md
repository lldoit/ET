# 增加外部服务器 MessagePack 编解码路径

## 背景

UGF10 当前 ET 主网络消息序列化路径使用 `NinoHelper`，并由 `MessageSerializeHelper` 负责 opcode、`FiberInstanceId` 包头和 payload 读写。现有链路服务于当前 ET 服务器通信，不应因接入另一套服务器而改变。

源工作区 `/Users/lilei/Work/UGF_MessagePack/Packages/cn.etetet.messagepack` 已有可复用的 MessagePack-CSharp Unity package。当前需求是为另一套服务器通信增加 MessagePack 解析和序列化能力，而不是替换现有 ET Proto/Nino 协议。

## 目标

- 将 `cn.etetet.messagepack` 作为独立 package 接入 UGF10。
- 新增一条面向外部服务器的 MessagePack codec/adaptor 路径。
- 在客户端外网 session 上支持 Nino 与 MessagePack 两条网络解析路径可切换。
- 保持现有 ET 主网络 Nino 通信路径不变。
- 提供最小 DTO roundtrip 验证，确认 MessagePack 序列化和反序列化在目标工程可用。
- 通过 `dotnet build ET.sln` 做基础编译验证。

## 非目标

- 不修改 `Packages/cn.etetet.proto/DotNet~/Proto2CS.cs`。
- 不修改现有 ET 协议生成物，不给当前 ET Proto 消息批量增加 MessagePack 标注。
- 不把 `MessageSerializeHelper` 全局切换到 MessagePack；切换只能按 session 显式设置。
- 不做 ET 登录链路测试。
- 不做真实外部服务器联调。
- 不重构现有网络传输层、opcode 分发或 `FiberInstanceId` 包头。
- 不在本阶段实现与外部服务器不同 framing 的完整适配；本阶段 MessagePack 网络 codec 复用 ET 当前 packet size + opcode framing，仅替换 payload 编解码。

## 成功标准

- UGF10 中存在完整 `Packages/cn.etetet.messagepack` 包，且 `packagegit.json` 编号不冲突。
- 目标使用方可以通过明确的外部服务器 codec/adaptor 调用 MessagePack serialize/deserialize。
- 客户端网络收发入口可以在 session 维度选择默认 Nino codec 或 MessagePack codec。
- 现有 `MessageSerializeHelper` 的 Nino 路径保持不变。
- 至少有一个本地 DTO roundtrip 验证覆盖 MessagePack 写入和读取。
- `dotnet build ET.sln` 成功；若失败，需定位是否与本次接入相关。

## 风险

- 源 MessagePack package 依赖 `System.Memory`、`System.Buffers` 等预编译引用，需要确认 UGF10 当前 Unity/package 环境能解析。
- 外部服务器协议结构尚未在本提案中定义，第一阶段只能提供 codec/adaptor 能力和示例 DTO 验证。
- 若后续外部服务器 DTO 依赖复杂泛型、Unity.Mathematics 或 AOT/IL2CPP 场景，需要补充 formatter 或 resolver 设计。
