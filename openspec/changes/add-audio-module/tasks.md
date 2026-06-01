# 实施清单

## 1. 包落点与规范文件

- [x] 确认 `Packages/cn.etetet.audio` 不存在。
- [x] 再次枚举 `packagegit.json`，确认 `Id=55` 未冲突。
- [x] 创建 `Packages/cn.etetet.audio/AGENTS.md`，说明包职责、依赖边界、验证方式。
- [x] 创建 `Packages/cn.etetet.audio/package.json`，依赖 `cn.etetet.core` 和 `cn.etetet.yooassets`。
- [x] 创建 `Packages/cn.etetet.audio/packagegit.json`，声明 `Id=55`、`Name=Audio`、`Level=2` 和 ModelView/HotfixView 依赖。
- [x] 创建 `Packages/cn.etetet.audio/Ignore.ET.Audio.asmdef`。
- [x] 创建 `Scripts/Model/Share/PackageType.cs` 和对应 `AssemblyReference.asmref`。
- [x] 创建 `Scripts/ModelView/Client/AssemblyReference.asmref`、`Scripts/HotfixView/Client/AssemblyReference.asmref`。

## 2. ModelView 数据结构

- [x] 创建 `AudioPlayErrorCode.cs`，定义播放失败码。
- [x] 创建 `AudioReplaceStrategy.cs`，定义 `RejectWhenFull`、`ReplaceLowestPriority`、`ReplaceOldestSameOrLowerPriority`、`ReplaceOldest`。
- [x] 创建 `AudioStopReason.cs`，定义主动停止、分组停止、全部停止、替换、自然结束、组件销毁、加载取消。
- [x] 创建 `AudioPlayParams.cs`，包含默认值、循环、优先级、组内音量、淡入、音调、声相、空间混合、最大距离、多普勒等级。
- [x] 创建 `AudioPlayInfo.cs`，记录 `SerialId`、`AssetName`、`GroupName`、`AudioPlayParams`。
- [x] 创建 `AudioPlayRequest.cs`，记录加载请求状态、取消标记、停止原因和用户数据。
- [x] 创建 `AudioAssetHandle.cs`，封装 `AudioClip` 与 YooAssets `AssetHandle`，提供 `Release()`。
- [x] 确认 `AudioAssetHandle.Release()` 幂等，重复释放不会重复调用底层 handle。
- [x] 创建 `AudioAgentState.cs`，定义 `Idle`、`Loading`、`Playing`、`Pausing`、`Paused`、`Stopping`。
- [x] 创建 `AudioAgent.cs`，保存 `AudioSource`、当前资源、序列号、优先级、播放状态、设置资源时间和 `FadeVersion`。
- [x] 创建 `AudioGroup.cs`，保存分组名、静音、音量、`AudioReplaceStrategy` 和代理列表。
- [x] 创建 `AudioComponent.cs`，保存分组字典、请求状态表、序列号、Unity 根节点和 `IAudioLoader`。

## 3. 资源加载抽象

- [x] 创建 `IAudioLoader.cs`，定义异步加载 `AudioAssetHandle` 的接口。
- [x] 创建 `YooAssetsAudioLoader.cs`，使用 YooAssets 加载 `AudioClip` 并返回 `AudioAssetHandle`。
- [x] 在 `AudioComponent` Awake 时初始化默认 `IAudioLoader`。

## 4. 事件结构

- [x] 创建 `AudioPlaySuccess.cs`。
- [x] 创建 `AudioPlayFailure.cs`。
- [x] 创建 `AudioPlayEnd.cs`。
- [x] 创建 `AudioPlayStopped.cs`。
- [x] 创建 `AudioPlayCancelled.cs`。
- [x] 创建 `AudioAgentReset.cs`。

## 5. AudioAgent 系统

- [x] 实现代理初始化：创建 `GameObject` 和 `AudioSource`。
- [x] 实现 `SetAudioClip(AudioAssetHandle handle)`，替换旧资源前先释放旧 handle。
- [x] 实现参数应用：`time`、`loop`、`pitch`、`panStereo`、`spatialBlend`、`maxDistance`、`dopplerLevel`。
- [x] 实现 `RefreshMute()` 和 `RefreshVolume()`。
- [x] 实现 `Play(float fadeInSeconds)`。
- [x] 实现 `Stop(float fadeOutSeconds)`，淡出结束后重置并释放资源。
- [x] 实现 `Pause(float fadeOutSeconds)` 和 `Resume(float fadeInSeconds)`。
- [x] 实现 `FadeVersion`：每次 Play/Stop/Pause/Resume 递增版本，旧 fade 检测到版本不一致后退出。
- [x] 实现显式代理状态切换，避免 Stop/Pause/Resume 连续调用时状态错乱。
- [x] 实现播放结束轮询或协程检查，非循环音频结束后发布 `AudioPlayEnd` 并重置代理。

## 6. AudioGroup 系统

- [x] 实现添加代理。
- [x] 实现按 `AudioReplaceStrategy` 选择播放代理：
  - 空闲代理优先。
  - `RejectWhenFull`：池满时拒绝。
  - `ReplaceLowestPriority`：只允许高优先级替换最低优先级代理。
  - `ReplaceOldestSameOrLowerPriority`：替换低优先级或同优先级最早代理。
  - `ReplaceOldest`：替换最早代理。
  - 没有候选时返回 `IgnoredDueToLowPriority`。
- [x] 实现按 `serialId` 查询播放状态。
- [x] 实现按 `serialId` 停止、暂停、恢复。
- [x] 实现停止分组内所有已加载代理。
- [x] 实现分组静音与音量刷新。

## 7. AudioComponent 系统

- [x] 实现 Awake：创建 Unity 根节点，初始化默认分组 `Music`、`Sound`、`Voice`。
- [x] 实现 Destroy：停止所有声音，取消所有加载中请求，释放资源，销毁 Unity 根节点。
- [x] 实现 `AddGroup`。
- [x] 实现 `Play`：
  - 校验 `assetName` 与 `groupName`。
  - 分配 `serialId`。
  - 创建 `AudioPlayRequest` 并写入请求状态表。
  - 通过 `IAudioLoader` 异步加载 `AudioAssetHandle`。
  - `await` 后使用 `EntityRef<AudioComponent>` 重新取实体。
  - 处理取消、销毁、加载失败、低优先级失败等分支。
  - 每个加载成功但不播放的分支都释放 `AudioAssetHandle`。
  - 所有成功、失败、取消分支都从请求状态表移除请求。
  - 播放成功后发布 `AudioPlaySuccess`。
- [x] 实现 `Stop`、`StopGroup`、`StopAll`。
- [x] 实现 `StopAllLoading` 和加载中请求取消标记，加载完成后发布 `AudioPlayCancelled` 并释放资源。
- [x] 实现 `Pause`、`Resume`。
- [x] 实现 `SetGroupMute`、`SetGroupVolume`。
- [x] 实现 `IsLoading`、`IsPlaying`。

## 8. 对外 Helper

- [x] 创建 `AudioHelper.cs`。
- [x] 实现 `Get(Scene scene)`：按项目现有根组件约定获取或创建 `AudioComponent`。
- [x] 实现 `Play`、`PlayMusic`、`PlaySound` 快捷入口。
- [x] 实现 `Stop`、`StopGroup`、`StopAll`、`Pause`、`Resume` 快捷入口。
- [x] 实现 `SetGroupMute`、`SetGroupVolume` 快捷入口。

## 8.1 UI Prefab 音效配置

- [x] 在 `Runtime/` 创建 `UIAudioConfig.cs`，允许在 UI Prefab 上配置点击、打开、关闭音效。
- [x] 为 `Runtime/` 创建 `AssemblyReference.asmref`，汇入 `ET.YIUIFramework`。
- [x] 移除 `UIAudioConfig` 的 `[EnableClass]`，避免 Unity `MonoBehaviour` 误用 ET 普通类标记。
- [x] 创建 `UIAudioConfigSystem.cs`，提供 `BindUIAudio(Scene, GameObject)` 和 `PlayUICloseSound(Scene, GameObject)`。
- [x] 点击音效绑定只追加监听器，不清空业务已有 `Button.onClick`。
- [x] 点击音效绑定支持重复调用去重，只替换 `UIAudioConfig` 自己上一次追加的监听器。
- [x] UI 音效配置组件汇入 `ET.YIUIFramework`，由具体界面在生命周期里显式调用绑定。

## 9. 编译与静态验证

- [x] 运行 `git diff --check`。
- [x] 运行 `dotnet build ET.sln`。
- [x] 若编译失败，按最小改动修正包依赖、命名空间或 API 不匹配。
- [x] 复核 `await` 后 Entity 访问，确认使用 `EntityRef<AudioComponent>` 或生命周期检查。
- [x] 复核资源释放分支，确认加载成功但不播放时不会泄露 handle。
- [x] 复核请求状态表，确认成功、失败、取消、销毁路径都会移除请求。
- [x] 复核 fade 版本控制，确认旧 fade 不会覆盖新状态。

## 10. 运行时验证记录

- [ ] 如果 UnityBridge 可用，查询 Unity 编译状态。
- [ ] 如果可进入 PlayMode，创建最小音频播放烟测，验证播放、停止、暂停、恢复、分组音量和静音。
- [ ] 验证连续 Stop/Pause/Resume 不会留下错误音量或卡住状态。
- [ ] 验证加载中 Stop 后，加载完成不会播放且资源会释放。
- [x] 如果 UnityBridge 不可用，在交付说明中明确运行时验证未执行及原因。
