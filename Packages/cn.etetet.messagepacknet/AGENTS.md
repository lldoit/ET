# cn.etetet.messagepacknet

## 概述

外部服务器 MessagePack payload 编解码包。

## 边界

- 只处理外部服务器 DTO 与 MessagePack payload 的互转。
- 不接管 ET 主网络 `MessageSerializeHelper`。
- 不修改 `Proto2CS` 或现有 ET Proto 生成物。
- 不实现登录、重连、心跳或真实服务器联调。

## 依赖

- 依赖 `cn.etetet.core` 使用 `MemoryBuffer`。
- 依赖 `cn.etetet.messagepack` 使用 MessagePack-CSharp。
- `Scripts/Model/Share` 汇入 `ET.Model`，`Scripts/Hotfix/Test` 汇入 `ET.Hotfix`。

## 验证

- 使用最小 DTO roundtrip 覆盖 `byte[]` 和 `MemoryBuffer` 路径。
- 使用 `dotnet build ET.sln` 做基础编译验证。
