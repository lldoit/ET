# cn.etetet.messagepack

## 概述

内嵌 MessagePack-CSharp 包，供外部服务器 MessagePack payload 编解码使用。

## 边界

- 本包只承载 MessagePack 库源码、asmdef、预编译引用和必要的 Unity/Mathematics formatter。
- 不在本包内实现业务协议、登录、重连、心跳或 ET 主网络 serializer 切换。
- 不修改 `MessageSerializeHelper`、`Proto2CS` 或现有 ET Proto 生成物。
- 修改上游 MessagePack 源码前必须先证明是 UGF10 编译或运行所需的最小兼容修正。

## 验证

- 基础验证使用仓库唯一编译入口：`dotnet build ET.sln`。
- Unity package 解析问题优先通过 Unity 刷新或 UnityBridge 排查。
