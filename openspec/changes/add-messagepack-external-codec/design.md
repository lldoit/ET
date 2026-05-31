# 设计

## 总体方案

采用旁路接入 + session 显式切换方式：保留 UGF10 当前 ET 主网络栈的 Nino 序列化，新增一个独立的 MessagePack 编解码能力供另一套服务器通信使用，并允许客户端外网 session 在 Nino 与 MessagePack 网络 codec 之间切换。

现有链路保持：

```text
ET server <-> MessageSerializeHelper <-> NinoHelper <-> ET Proto 消息
```

新增链路为：

```text
External server <-> NetComponent/Session <-> MessagePack network codec <-> External DTO
```

这两个链路不共享 `MessageSerializeHelper` 的 payload 序列化选择，不改变现有 Nino 默认行为。第一阶段的 MessagePack network codec 复用 ET 当前 packet size + opcode framing，只替换 opcode 后的 payload 编解码；如果外部服务器 framing 不同，应新增独立 adaptor，而不是硬塞进 `MessageSerializeHelper`。

## 包接入

从 `/Users/lilei/Work/UGF_MessagePack/Packages/cn.etetet.messagepack` 迁入 `Packages/cn.etetet.messagepack`。

保留源包内部 asmdef 名称：

- `MessagePack`
- `MessagePack.Annotations`

保留源包 `packagegit.json`：

```json
{
  "Id": 1503,
  "Name": "MessagePack"
}
```

当前 UGF10 包编号检查显示 `1503` 未被占用。`Packages/packages-lock.json` 需要增加 embedded package 记录。

## 代码落点

第一阶段优先新增一个小型外部服务器 codec/adaptor 包或模块。候选落点：

- 若已有明确外部服务器业务包，则放入该业务包。
- 若当前没有明确业务包，则新增 `Packages/cn.etetet.messagepacknet` 或类似独立 package，依赖 `cn.etetet.core` 与 `cn.etetet.messagepack`。

codec 只负责二进制与 DTO 的转换，不负责登录、重连、心跳或业务流程。

core 包新增网络 codec 抽象，避免 `cn.etetet.core` 反向依赖 MessagePack package：

```csharp
public interface INetworkMessageCodec
{
    (ushort Opcode, MemoryBuffer MemoryBuffer) ToMemoryBuffer(AService service, FiberInstanceId fiberInstanceId, object message);
    (FiberInstanceId FiberInstanceId, object Message) ToMessage(AService service, MemoryBuffer memoryBuffer);
}
```

`NinoNetworkMessageCodec` 放在 `cn.etetet.core`，内部包装现有 `MessageSerializeHelper`。`MessagePackNetworkMessageCodec` 放在 `cn.etetet.messagepacknet`，依赖 `cn.etetet.messagepack`，只处理显式设置为 MessagePack 的 session。

建议接口形态：

```csharp
public interface IExternalMessageCodec
{
    void Serialize<T>(T message, MemoryBuffer buffer);
    T Deserialize<T>(ReadOnlyMemory<byte> bytes);
}
```

若外部服务器已有固定 framing，例如长度头、消息 id、压缩标记，应在 adaptor 层处理；MessagePack codec 只处理 payload。本阶段的 network codec 默认 packet body 为：

```text
[opcode: ushort][messagepack payload]
```

## DTO 约定

外部服务器 DTO 与 ET Proto 消息分离，不复用现有 `Packages/cn.etetet.proto/CodeMode/Model` 生成类。

DTO 可以按 MessagePack-CSharp 常规方式声明：

```csharp
[MessagePackObject]
public sealed class ExternalPing
{
    [Key(0)]
    public string Text { get; set; }
}
```

字段编号由外部服务器协议约定决定，不能从 ET Proto 字段顺序推导。

## 与现有 Nino 路径的关系

`MessageSerializeHelper`、`NinoHelper`、`OpcodeType`、ET Proto 生成物和现有网络 session 逻辑保持不变。

不增加全局 serializer enum，不在 `MessageSerializeHelper` 中做全局 serializer 分支。切换挂在 `Session.NetworkMessageCodec` 上：

- 默认值为 `NinoNetworkMessageCodec.Instance`。
- ET 主网络 session 不设置 codec 时行为不变。
- 外部服务器 session 创建后显式设置为 `MessagePackNetworkMessageCodec.Instance`。

`NetComponentSystem.OnRead` 从当前 session 取得 codec 解析消息；`SessionSystem.Send` 从同一 session 取得 codec 打包消息，保证同一连接收发一致。

## 验证

基础验证：

```powershell
dotnet build ET.sln
```

本地 roundtrip 验证：

- 创建最小 MessagePack DTO。
- 序列化到 `MemoryBuffer` 或 `byte[]`。
- 反序列化回 DTO。
- 断言字段值一致。
- 使用 MessagePack network codec 对带 opcode 的 packet body 做一次 roundtrip。

不做网络登录测试，不要求真实外部服务器在线。

## 后续扩展

如果后续拿到外部服务器协议定义，再补充：

- DTO 落点与命名规范。
- framing 格式。
- message id 与 DTO 类型映射。
- 错误码和异常处理策略。
- AOT/IL2CPP formatter 覆盖范围。
