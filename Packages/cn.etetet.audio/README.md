# ET.Audio - ET框架音频管理模块

## 简介

ET.Audio 是为 ET 框架设计的音频管理模块，提供了完整的音乐和音效播放功能。

## 功能特性

- ✅ 背景音乐播放控制（播放、暂停、停止、恢复）
- ✅ **背景音乐淡入淡出切换（新增）**
- ✅ 音效播放支持（2D和3D音效）
- ✅ YooAsset资源加载集成
- ✅ AudioSource对象池管理（避免频繁创建销毁）
- ✅ 音量独立控制（背景音乐和音效分开控制）
- ✅ 协程锁避免重复加载
- ✅ 完全遵循ET框架规范（Entity-System分离）
- ✅ 异步加载和播放
- ✅ 资源自动缓存和释放
- ✅ **智能音乐切换（避免重复播放）**

## 安装

在项目的 `Packages/manifest.json` 中添加依赖：

```json
{
  "dependencies": {
    "cn.etetet.audio": "1.0.0",
    "cn.etetet.core": "3.0.3",
    "cn.etetet.yooassets": "2.3.6"
  }
}
```

## 快速开始

### 1. 初始化

在客户端场景中添加 SoundComponent：

```csharp
Scene clientScene = Root.Instance.Get(1);
clientScene.AddComponent<SoundComponent>();
```

### 2. 播放背景音乐

```csharp
SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();

// 播放循环背景音乐
await soundComp.PlayMusic("Audio_BGM_MainMenu");

// 播放不循环的音乐
await soundComp.PlayMusic("Audio_BGM_Victory", false);

// 带淡入淡出效果切换音乐（推荐）
await soundComp.PlayMusicWithFade("Audio_BGM_Battle", fadeOutDuration: 1.5f, fadeInDuration: 2.0f);

// 淡出并停止音乐
await soundComp.StopMusicWithFade(fadeOutDuration: 1.0f);

// 暂停音乐
soundComp.PauseMusic();

// 恢复音乐
soundComp.ResumeMusic();

// 停止音乐（立即）
soundComp.StopMusic();
```

### 3. 播放音效

```csharp
// 播放2D音效
await soundComp.PlaySound("Audio_SFX_Click");

// 播放3D音效（在指定位置）
Vector3 explosionPos = new Vector3(10, 0, 5);
await soundComp.PlaySound3D("Audio_SFX_Explosion", explosionPos);

// 停止所有音效
soundComp.StopAllSounds();
```

### 4. 音量控制

```csharp
// 设置背景音乐音量 (0.0 - 1.0)
soundComp.SetMusicVolume(0.8f);

// 设置音效音量 (0.0 - 1.0)
soundComp.SetSoundVolume(0.6f);
```

## 在YIUI中使用

可以在UI事件中直接调用：

```csharp
// UI按钮点击音效
private void OnButtonClick()
{
    Scene clientScene = this.Root() as Scene;
    clientScene.GetComponent<SoundComponent>().PlaySound("Audio_SFX_ButtonClick").NoContext();
}
```

## 基于事件的解耦使用

更推荐使用事件系统来触发音效：

### 1. 定义事件

```csharp
namespace ET.Client
{
    public struct PlaySoundEvent
    {
        public string Address;
    }
}
```

### 2. 创建事件处理器

```csharp
namespace ET.Client
{
    [Event(SceneType.Client)]
    public class PlaySoundEventHandler : AEvent<Scene, PlaySoundEvent>
    {
        protected override async ETTask Run(Scene scene, PlaySoundEvent args)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                await soundComp.PlaySound(args.Address);
            }
        }
    }
}
```

### 3. 触发事件

```csharp
// 在任何地方发布事件
EventSystem.Instance.Publish(clientScene, new PlaySoundEvent { Address = "Audio_SFX_Victory" });
```

## 对象池配置

可以调整对象池大小：

```csharp
SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();
soundComp.SetMaxPoolSize(20); // 默认为10
```

## 注意事项

1. **资源地址**：确保使用的资源地址与YooAsset中配置的Address一致
2. **/Global节点**：SoundComponent需要场景中存在 `/Global` 节点，会自动创建 `/Global/Audio` 子节点
3. **生命周期**：SoundComponent通常挂载在客户端Scene上，随Scene生命周期管理
4. **资源释放**：当SoundComponent销毁时会自动释放所有加载的音频资源
5. **EntityRef规范**：代码严格遵循ET框架的await后EntityRef安全访问规范

## 架构说明

### ModelView 层 (不可热更新)
- `SoundComponent.cs` - 音频组件Entity定义，包含所有数据字段

### HotfixView 层 (可热更新)
- `SoundComponentSystem.cs` - 音频组件System实现，包含所有业务逻辑

### Model 层
- `PackageType.cs` - 包类型定义
- `CoroutineLockType.cs` - 协程锁类型定义

## 技术特点

- **异步加载**：使用ETTask包装YooAsset异步操作
- **对象池**：AudioSource复用，减少GC
- **协程锁**：避免重复加载同一资源
- **资源缓存**：AudioClip缓存，提高性能
- **句柄管理**：正确管理YooAsset句柄，避免内存泄漏
- **EntityRef安全**：遵循ET分析器规范，await后安全访问Entity

## 版本历史

### v1.0.0
- 初始版本
- 支持背景音乐和音效播放
- 集成YooAsset资源加载
- AudioSource对象池管理

## 许可证

MIT License

