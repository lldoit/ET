# 设计

## 总体方案

新增 `cn.etetet.audio` Level 2 客户端基础包。模块参考 GameFrameX Sound 的分层模型，但改成 ET 风格：

- `AudioComponent`：ET Entity，作为音频模块运行时根节点，保存分组、加载中请求、序列号和 Unity 实例根节点。
- `AudioGroup`：逻辑分组，保存组音量、静音、替换策略和代理池。
- `AudioAgent`：单个 `AudioSource` 播放代理，负责绑定 `AudioClip`、应用参数、播放、停止、暂停、恢复和重置。
- `AudioPlayParams`：播放参数，描述循环、优先级、音量、淡入淡出和 3D 音频参数。
- `AudioPlayResult` / `AudioEvent`：描述播放成功、失败、结束和重置事件。
- `AudioAssetHandle`：封装 `AudioClip` 和 YooAssets `AssetHandle`，保证停止、替换、销毁时释放资源。
- `AudioPlayRequest`：加载请求状态表条目，统一记录 `serialId`、资源名、分组名、播放参数、取消状态和用户数据。
- `IAudioLoader`：音频资源加载抽象，默认实现使用 YooAssets。核心调度层只依赖该接口，避免把资源系统细节散落在播放逻辑中。
- `UIAudioConfig`：可挂在 UI Prefab 上的 Unity 组件，用于配置点击、打开、关闭音效和优先级。

包内代码只放在 `Packages/cn.etetet.audio`，不直接修改现有业务包。

## 相对 GameFrameX Sound 的优化

本模块保留 GameFrameX Sound 的声音组、代理池和播放参数模型，但不照搬其框架依赖和状态组织方式。优化点如下：

- 资源 ownership 强类型化：不使用 `object soundAsset` 传递资源，统一使用 `AudioAssetHandle` 表示已加载的 `AudioClip` 和释放句柄。
- 加载状态集中化：不拆分为加载中列表与待释放集合，而是使用 `Dictionary<int, AudioPlayRequest>` 记录每个请求的生命周期。
- 替换策略显式化：分组使用 `AudioReplaceStrategy`，而不是只有“避免同优先级替换”布尔值。
- fade 状态防串扰：每个代理维护 `FadeVersion`，新的淡入淡出会让旧任务失效。
- 事件语义补全：区分播放成功、失败、自然结束、主动停止、加载取消和代理重置。
- ET 分层实现：Manager/Group/Agent 的逻辑分别落到 `AudioComponentSystem`、`AudioGroupSystem`、`AudioAgentSystem`，避免单个 Manager partial 类承载过多职责。

## 包边界与依赖

建议包编号使用 `55`，当前 UGF10 `packagegit.json` 已使用到 `54`，且 `55` 未冲突。

`packagegit.json`：

```json
{
  "Id": 55,
  "Name": "Audio",
  "Level": 2,
  "ScriptsReferences": {
    "ModelView": [
      "ET.YooAssets"
    ],
    "HotfixView": [
      "ET.YooAssets"
    ]
  }
}
```

`package.json` 显式依赖：

- `cn.etetet.core`：ET Entity、System、事件和基础工具。
- `cn.etetet.yooassets`：客户端 `AudioClip` 异步加载与资源释放。

音频播放核心不通过 YIUI 加载抽象获取音频资源。`UIAudioConfig` 是 UI Prefab 配置组件，放在 `Runtime/` 并通过 asmref 汇入 `ET.YIUIFramework`，和其它 YIUI Runtime 扩展组件保持一致。

UI 音效配置只依赖 Unity `GameObject` 与 `UnityEngine.UI.Button`。业务界面在打开时调用 `scene.BindUIAudio(rootGameObject)`，关闭前可调用 `scene.PlayUICloseSound(rootGameObject)`。这样 Audio 包不需要知道 YIUI 的窗口生命周期，也不需要修改 YIUI 框架。

`BindUIAudio` 必须允许界面复用或重复打开时安全重复调用。点击音效绑定只移除并替换 `UIAudioConfig` 自己上一次追加的监听器，不清空或替换业务已经配置在 `Button.onClick` 上的其它监听器。

## 目录结构

```text
Packages/cn.etetet.audio/
  AGENTS.md
  package.json
  packagegit.json
  Ignore.ET.Audio.asmdef
  Scripts/
    Runtime/
      AssemblyReference.asmref
      UIAudioConfig.cs
    ModelView/Client/
      AssemblyReference.asmref
      Component/
        AudioComponent.cs
        AudioGroup.cs
        AudioAgent.cs
        AudioAssetHandle.cs
        AudioPlayParams.cs
        AudioPlayRequest.cs
        AudioPlayErrorCode.cs
        AudioReplaceStrategy.cs
        AudioStopReason.cs
        AudioPlayInfo.cs
      Loader/
        IAudioLoader.cs
        YooAssetsAudioLoader.cs
      Event/
        AudioPlaySuccess.cs
        AudioPlayFailure.cs
        AudioPlayEnd.cs
        AudioPlayStopped.cs
        AudioPlayCancelled.cs
        AudioAgentReset.cs
    HotfixView/Client/
      AssemblyReference.asmref
      System/
        AudioComponentSystem.cs
        AudioGroupSystem.cs
        AudioAgentSystem.cs
        AudioHelper.cs
        UIAudioConfigSystem.cs
    Model/Share/
      AssemblyReference.asmref
      PackageType.cs
```

如现有程序集引用生成规则需要更少目录，实施时以项目当前 `AssemblyReference.asmref` 习惯为准。`UIAudioConfig` 是 Unity `MonoBehaviour`，放在包的 `Runtime/` 目录，并通过 asmref 汇入 `ET.YIUIFramework`，避免落在 ET Model/ModelView 分析器的普通类目录中，也不需要添加 `[EnableClass]`。

## API 设计

HotfixView 对外提供 `AudioHelper`，减少业务直接操作内部实体：

```csharp
public static class AudioHelper
{
    public static AudioComponent Get(Scene scene);
    public static ETTask<int> Play(Scene scene, string assetName, string groupName, AudioPlayParams playParams = null);
    public static ETTask<int> PlayMusic(Scene scene, string assetName, bool loop = true, float fadeInSeconds = 0f);
    public static ETTask<int> PlaySound(Scene scene, string assetName, int priority = 0);
    public static bool Stop(Scene scene, int serialId, float fadeOutSeconds = 0f);
    public static void StopGroup(Scene scene, string groupName, float fadeOutSeconds = 0f);
    public static void StopAll(Scene scene, float fadeOutSeconds = 0f);
    public static void Pause(Scene scene, int serialId, float fadeOutSeconds = 0f);
    public static void Resume(Scene scene, int serialId, float fadeInSeconds = 0f);
    public static void SetGroupMute(Scene scene, string groupName, bool mute);
    public static void SetGroupVolume(Scene scene, string groupName, float volume);
}
```

UI Prefab 音效配置入口：

```csharp
// 界面打开后，对根节点或局部节点扫描 UIAudioConfig。
scene.BindUIAudio(rootGameObject);

// 界面关闭前，播放根节点配置的关闭音效。
scene.PlayUICloseSound(rootGameObject);
```

`UIAudioConfig` 字段：

- `ClickSound`：同节点 `Button` 点击时播放的音效资源名。
- `OpenSound`：根节点绑定时播放的打开音效资源名。
- `CloseSound`：关闭前播放的音效资源名。
- `GroupName`：默认 `Sound`。
- `Priority`：播放优先级。
- `BindClick`：是否自动绑定同节点 `Button.onClick`。
- `IncludeInactiveChildren`：从根节点扫描时是否包含 inactive 子节点。

`UIAudioConfig` 内部保存本模块追加的点击监听引用，用于下一次绑定时去重。该字段只服务运行时去重，不作为 Prefab 可配置项暴露。

`AudioComponent` 提供底层 API：

- `AddGroup(string groupName, int agentCount, bool avoidSamePriorityReplace, bool mute, float volume)`
- `AddGroup(string groupName, int agentCount, AudioReplaceStrategy replaceStrategy, bool mute, float volume)`
- `Play(string assetName, string groupName, AudioPlayParams playParams)`
- `Stop(int serialId, float fadeOutSeconds)`
- `StopGroup(string groupName, float fadeOutSeconds)`
- `StopAll(float fadeOutSeconds)`
- `Pause(int serialId, float fadeOutSeconds)`
- `Resume(int serialId, float fadeInSeconds)`
- `IsLoading(int serialId)`
- `IsPlaying(int serialId)`

默认分组：

- `Music`：1 个代理，`ReplaceLowestPriority`，默认循环由调用参数决定。
- `Sound`：8 个代理，`ReplaceOldestSameOrLowerPriority`。
- `Voice`：2 个代理，`ReplaceOldestSameOrLowerPriority`。

替换策略：

- `RejectWhenFull`：代理池满时直接拒绝新请求。
- `ReplaceLowestPriority`：只允许高优先级替换当前最低优先级代理。
- `ReplaceOldestSameOrLowerPriority`：高优先级替换低优先级；同优先级替换最早设置资源的代理。
- `ReplaceOldest`：代理池满时替换最早设置资源的代理，主要用于非关键短音效。

## 播放流程

1. 调用方传入 `assetName`、`groupName` 和 `AudioPlayParams`。
2. `AudioComponent` 分配递增 `serialId`，创建 `AudioPlayRequest` 并写入请求状态表。
3. 通过 `IAudioLoader` 异步加载 `AudioAssetHandle`，默认实现使用 YooAssets 加载 `AudioClip`。
4. `await` 后通过 `EntityRef<AudioComponent>` 重新取实体，并检查：
   - 组件未销毁。
   - `serialId` 对应请求仍存在。
   - 请求未被 `Stop`、`StopGroup`、`StopAll` 或 `StopAllLoading` 标记取消。
5. 加载失败则移除请求并发布失败事件。
6. 加载成功后由 `AudioGroup` 选择可用代理：
   - 优先选择空闲代理。
   - 若无空闲代理，按分组 `AudioReplaceStrategy` 选择候选代理。
   - 若无候选代理，释放刚加载的资源并发布低优先级失败事件。
7. `AudioAgent` 设置 `AudioSource.clip` 和播放参数，执行淡入并播放。
8. 播放成功后从请求状态表移除请求并发布 `AudioPlaySuccess`。
9. 播放完成后代理重置并释放资源句柄；循环音频不自动结束。

## 请求状态表

`AudioPlayRequest` 至少包含：

- `SerialId`
- `AssetName`
- `GroupName`
- `AudioPlayParams`
- `UserData`
- `Cancelled`
- `CancelReason`

所有加载中取消都只标记请求，不直接等待资源加载任务终止。加载完成后如果请求已取消，立即释放 `AudioAssetHandle`，发布 `AudioPlayCancelled`，并从状态表移除。这样可以避免异步加载完成后误播放，也能避免资源泄漏。

## 资源释放

`AudioAssetHandle` 保存 `AudioClip` 与 YooAssets `AssetHandle`。代理替换、停止完成、播放结束、组件销毁时统一调用 `Release()`。`Release()` 必须幂等，重复调用不报错、不重复释放底层 handle。

资源加载失败、低优先级被忽略、加载完成但组件已销毁时必须立即释放句柄。加载中被取消时，加载完成后不播放，直接释放。

## 淡入淡出

淡入淡出采用 ET 协程式 `ETTask`，按帧调整 `AudioSource.volume`。实际音量为：

```text
agentVolume = groupVolume * playParams.VolumeInGroup * fadeFactor
```

当组音量或静音变化时，正在播放的代理立即刷新音量和静音状态。淡出结束后执行 `Stop` 或 `Pause`，并保持资源释放规则一致。

每个 `AudioAgent` 维护递增 `FadeVersion`。调用 `Play`、`Stop`、`Pause`、`Resume` 时都会递增版本号并启动新的 fade。旧 fade 循环每帧检查版本号，不一致则退出，避免连续操作时旧任务覆盖新音量或状态。

代理状态建议使用显式枚举：

- `Idle`
- `Loading`
- `Playing`
- `Pausing`
- `Paused`
- `Stopping`

状态枚举只表达代理本地播放状态；加载请求状态仍由 `AudioPlayRequest` 管理。

## 事件

使用 ET 事件发布以下结构：

- `AudioPlaySuccess`：`SerialId`、`AssetName`、`GroupName`、`AudioAgent`。
- `AudioPlayFailure`：`SerialId`、`AssetName`、`GroupName`、`AudioPlayErrorCode`、`ErrorMessage`。
- `AudioPlayEnd`：`SerialId`、`AssetName`、`GroupName`。
- `AudioPlayStopped`：`SerialId`、`AssetName`、`GroupName`、`AudioStopReason`。
- `AudioPlayCancelled`：`SerialId`、`AssetName`、`GroupName`、`AudioStopReason`。
- `AudioAgentReset`：`SerialId`、`GroupName`。

停止原因：

- `ManualStop`
- `StopGroup`
- `StopAll`
- `Replace`
- `NaturalEnd`
- `ComponentDestroy`
- `LoadCancelled`

失败码：

- `AssetNameInvalid`
- `GroupNotFound`
- `LoadAssetFailure`
- `AudioClipInvalid`
- `IgnoredDueToLowPriority`
- `SetAudioClipFailure`
- `Cancelled`
- `ComponentDisposed`

`AudioPlayFailure` 表示无法完成播放请求；`AudioPlayCancelled` 表示调用方主动取消仍在加载中的请求；`AudioPlayStopped` 表示已开始播放后的主动停止或替换；`AudioPlayEnd` 只表示非循环音频自然结束。

## Unity 对象生命周期

`AudioComponent` Awake 时创建一个隐藏的 `GameObject` 根节点，例如 `AudioRoot`，并对每个代理创建子节点和 `AudioSource`。Destroy 时销毁根节点，停止所有代理，释放所有加载中和已加载资源。

若项目已有全局 Root 或 Unity 场景对象管理约定，实施时应优先挂到现有根节点；否则使用 `UnityEngine.Object.DontDestroyOnLoad` 保持跨场景播放。

## 验证策略

基础验证：

```powershell
git diff --check
dotnet build ET.sln
```

定向代码验证：

- 检查 `AudioComponentSystem.Play` 中 `await` 后使用 `EntityRef<AudioComponent>`。
- 检查每个加载成功但不播放的分支都释放 `AudioAssetHandle`。
- 检查 `StopAll` 同时覆盖已加载和加载中的请求。
- 检查 `AudioPlayRequest` 状态表在成功、失败、取消和组件销毁路径上都会移除请求。
- 检查 `FadeVersion` 能阻止旧 fade 任务覆盖新状态。

运行时验证需要 UnityBridge 或 Unity Editor：

- 在测试场景创建 `AudioComponent` 后播放一个 `AudioClip`。
- 验证 BGM 循环播放、SFX 多代理播放、低优先级被拒绝、高优先级替换低优先级。
- 验证 Stop/Pause/Resume/SetGroupVolume/SetGroupMute 生效。
- 验证连续 Stop/Pause/Resume 不会留下错误音量或卡在中间状态。
