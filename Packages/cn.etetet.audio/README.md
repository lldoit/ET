# cn.etetet.audio

## UI Prefab 绑定示例

在 UI Prefab 根节点或按钮节点添加 `ET/Audio/UI Audio Config`：

- 根节点：用于配置面板打开、关闭音效；`Bind Click` 可关闭。
- 按钮节点：填写 `Click Sound`，保持 `Group Name` 为 `Sound`。
- 业务界面打开时调用 `scene.BindUIAudio(self.UIBase.OwnerGameObject)`。
- 关闭面板前可调用 `scene.PlayUICloseSound(self.UIBase.OwnerGameObject)`。

示例资源名：

- `SFX_UI_Click`
- `SFX_UI_Open`
- `SFX_UI_Close`

## AudioMixer 配置

本包不提交 `.mixer` 资源。项目侧创建并配置 `AudioMixer` 后，通过运行时代码绑定到分组：

```csharp
AudioHelper.SetGroupMixerGroup(scene, "Music", musicMixerGroup);
AudioHelper.SetGroupMixerGroup(scene, "Sound", soundMixerGroup);
AudioHelper.SetGroupMixerGroup(scene, "Voice", voiceMixerGroup);
```

绑定后会刷新当前分组内已存在的 `AudioSource.outputAudioMixerGroup`，后续播放也会自动使用该分组的 mixer。
