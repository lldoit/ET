# cn.etetet.audio

客户端音频基础包，负责统一管理 BGM、音效、语音等 `AudioClip` 播放。

## 职责

- 提供 `AudioComponent`、`AudioGroup`、`AudioAgent` 和 `AudioHelper`。
- 在 `Runtime/` 提供 `UIAudioConfig`，允许在 UI Prefab 上配置点击、打开、关闭音效，并通过 asmref 汇入 `ET.YIUIFramework`。
- 管理音频分组、代理池、优先级替换、淡入淡出、加载中取消和资源释放。
- 默认通过 YooAssets 加载 `AudioClip`，并由 `AudioAssetHandle` 负责释放 `AssetHandle`。

## 边界

- 只做客户端音频播放，不包含服务端逻辑。
- 音频播放核心不通过 YIUI 加载抽象获取音频资源；仅 Runtime UI Prefab 配置组件汇入 `ET.YIUIFramework`。
- UI 音效配置只依赖 Unity `Button` 和 `GameObject`，放在 `Runtime/`，由业务界面在打开/关闭时显式调用绑定。
- 不提交音频素材和 AudioMixer 资产。
- 修改异步播放逻辑时必须确认 `await` 后 Entity 访问使用 `EntityRef<T>` 或生命周期检查。

## 验证

- 基础验证使用项目唯一编译入口：`dotnet build ET.sln`。
- 提交前至少运行：`git diff --check`。
