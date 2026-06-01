# 新增 Audio 音频模块

## 背景

UGF10 当前只有 Unity 基础 `AudioManager` 设置、场景 `AudioListener`、DOTween 音频补间扩展和 I2 Localization 对 `AudioSource` 的本地化支持，没有项目级音频播放模块。业务侧缺少统一的 BGM、音效、语音等播放入口，也没有分组音量、静音、播放池、优先级替换、淡入淡出和播放生命周期管理。

GameFrameX 的 `com.gameframex.unity.sound` 已提供成熟的声音组、声音代理、播放参数、异步加载和事件通知设计。UGF10 需要参考其能力模型，但不能直接引入 GameFrameX 运行时依赖，应改造成符合 ET package、Entity/System、`ETTask`、YooAssets 资源加载和本项目包边界的 `cn.etetet.audio` 模块。

## 目标

- 新增 `Packages/cn.etetet.audio` 独立包，作为客户端音频基础能力包。
- 提供完整参考版音频能力：
  - 声音分组，例如 `Music`、`Sound`、`Voice`。
  - 每组独立静音、音量、优先级替换策略。
  - 每组维护固定数量 `AudioSource` 代理池。
  - 支持异步加载 `AudioClip` 并播放。
  - 支持播放、停止、暂停、恢复、停止所有、停止指定分组。
  - 支持淡入、淡出、循环、音量、音调、声相、空间混合、最大距离、多普勒等级。
  - 支持播放成功、播放失败、播放结束、代理重置等事件。
  - 支持加载中取消，避免已取消的加载完成后继续播放。
- API 设计参考 `GameFrameX.Sound` 的 `SoundManager`、`SoundGroup`、`SoundAgent`、`PlaySoundParams`，但命名、异步和生命周期适配 ET。
- 在参考实现基础上补充优化：
  - 用强类型资源句柄明确 `AudioClip` 与 YooAssets `AssetHandle` 的 ownership。
  - 用请求状态表统一管理加载中、取消、完成和失败状态。
  - 用可配置替换策略替代单一优先级规则。
  - 用 fade 版本号避免淡入淡出任务互相覆盖。
  - 补全自然结束、主动停止、取消和代理重置事件语义。
- 通过 `dotnet build ET.sln` 做基础编译验证。

## 非目标

- 不引入 `com.gameframex.unity.sound`、`GameFrameX.Runtime`、`UniTask`、`GameEntry` 或 GameFrameX Asset/Event 包。
- 不制作音频资源，不提交 `.wav`、`.mp3`、`.ogg` 等素材。
- 不改现有地图、UI、战斗业务逻辑来主动播放音频。
- 不新增 Unity 编辑器窗口或复杂 Inspector。
- 不新增 AudioMixer 资产；先预留 `AudioMixerGroup` 接口，实际混音资产后续由项目资源配置。
- 不做服务端音频逻辑。

## 成功标准

- `Packages/cn.etetet.audio` 包结构、`package.json`、`packagegit.json`、`PackageType.cs`、`AssemblyReference.asmref` 符合 UGF10 包规范。
- 包编号不与现有 `packagegit.json` 冲突。
- HotfixView 客户端可通过一个明确入口创建或获取 `AudioComponent`，并调用音频播放 API。
- 音频模块能通过 YooAssets 或项目现有资源加载入口异步加载 `AudioClip`，并在停止或销毁时释放资源句柄。
- 低优先级音频在代理池满时按策略失败；高优先级音频可替换低优先级音频。
- 停止、暂停、恢复和分组音量/静音会立即影响正在播放的代理。
- 加载中停止不会在加载完成后继续播放。
- 连续 Stop/Pause/Resume 或重复淡入淡出时，旧 fade 任务不会覆盖新状态。
- 资源句柄在播放失败、替换、取消、停止、自然结束和组件销毁路径上都有明确释放点。
- `dotnet build ET.sln` 成功，或失败原因被明确定位到非本次改动。

## 风险

- 纯 `dotnet build` 可能无法完整覆盖 Unity 运行时组件绑定、`AudioSource` 行为和 YooAssets 资源句柄释放，需要后续 UnityBridge 或 Unity Editor PlayMode 验证。
- 新包需要选择包编号与依赖层级，若现有包编号规划后续变化，可能需要调整。
- 音频播放依赖 Unity 主线程对象，异步加载完成后必须使用 `EntityRef<T>` 或生命周期检查避免访问已销毁实体。
- 若直接复用 YIUI 的加载抽象，会把 Audio 包不必要地依赖 UI 包；本方案应优先直接依赖 `cn.etetet.yooassets` 或现有基础加载能力。
