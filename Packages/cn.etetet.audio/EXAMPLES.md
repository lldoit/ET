# ET.Audio 使用示例

## 示例1: 基础音乐和音效播放

```csharp
using ET;
using ET.Client;
using UnityEngine;

namespace ET.Client
{
    public static class AudioExample
    {
        /// <summary>
        /// 初始化音频系统
        /// </summary>
        public static void InitializeAudio(Scene clientScene)
        {
            // 添加音频组件
            SoundComponent soundComp = clientScene.AddComponent<SoundComponent>();
            
            // 设置初始音量
            soundComp.SetMusicVolume(0.8f);
            soundComp.SetSoundVolume(0.7f);
            
            // 设置对象池大小
            soundComp.MaxPoolSize = 15;
        }
        
        /// <summary>
        /// 播放主菜单背景音乐
        /// </summary>
        public static async ETTask PlayMainMenuMusic(Scene clientScene)
        {
            SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();
            await soundComp.PlayMusic("Audio_BGM_MainMenu");
        }
        
        /// <summary>
        /// 播放按钮点击音效
        /// </summary>
        public static async ETTask PlayClickSound(Scene clientScene)
        {
            SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();
            await soundComp.PlaySound("Audio_SFX_Click");
        }
    }
}
```

## 示例2: YIUI面板中使用音效

```csharp
using ET;
using ET.Client;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 主菜单面板组件
    /// </summary>
    [ComponentOf(typeof(YIUIChild))]
    public class MainMenuPanelComponent : Entity, IAwake, IDestroy,
        IYIUIBind,
        IYIUIInitialize,
        IYIUIOpen
    {
        // UI绑定代码相关字段
        // ...
    }
    
    [FriendOf(typeof(MainMenuPanelComponent))]
    [EntitySystemOf(typeof(MainMenuPanelComponent))]
    public static partial class MainMenuPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MainMenuPanelComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this MainMenuPanelComponent self)
        {
        }
        
        [EntitySystem]
        private static void YIUIBind(this MainMenuPanelComponent self)
        {
            // 绑定按钮点击事件
            // self.u_btn_start.SetUIEventClick(self.OnStartButtonClick);
        }
        
        [EntitySystem]
        private static void YIUIInitialize(this MainMenuPanelComponent self)
        {
            // 初始化逻辑
        }
        
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MainMenuPanelComponent self)
        {
            // 播放面板打开音效
            Scene clientScene = self.Root() as Scene;
            SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                await soundComp.PlaySound("Audio_SFX_PanelOpen");
            }
            
            return true;
        }
        
        /// <summary>
        /// 开始按钮点击事件
        /// </summary>
        private static void OnStartButtonClick(this MainMenuPanelComponent self)
        {
            // 播放点击音效（不等待）
            Scene clientScene = self.Root() as Scene;
            SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();
            soundComp?.PlaySound("Audio_SFX_ButtonClick").NoContext();
            
            // 执行其他逻辑...
        }
    }
}
```

## 示例3: 基于事件的音效系统

### 3.1 定义音效事件

```csharp
namespace ET.Client
{
    /// <summary>
    /// 播放音效事件
    /// </summary>
    public struct PlaySoundEvent
    {
        public string Address;
    }
    
    /// <summary>
    /// 播放3D音效事件
    /// </summary>
    public struct PlaySound3DEvent
    {
        public string Address;
        public Vector3 Position;
    }
    
    /// <summary>
    /// 播放背景音乐事件
    /// </summary>
    public struct PlayMusicEvent
    {
        public string Address;
        public bool Loop;
    }
}
```

### 3.2 创建事件处理器

```csharp
using UnityEngine;

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
    
    [Event(SceneType.Client)]
    public class PlaySound3DEventHandler : AEvent<Scene, PlaySound3DEvent>
    {
        protected override async ETTask Run(Scene scene, PlaySound3DEvent args)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                await soundComp.PlaySound3D(args.Address, args.Position);
            }
        }
    }
    
    [Event(SceneType.Client)]
    public class PlayMusicEventHandler : AEvent<Scene, PlayMusicEvent>
    {
        protected override async ETTask Run(Scene scene, PlayMusicEvent args)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                await soundComp.PlayMusic(args.Address, args.Loop);
            }
        }
    }
}
```

### 3.3 使用事件触发音效

```csharp
namespace ET.Client
{
    public static class AudioEventExample
    {
        /// <summary>
        /// 在战斗中播放爆炸音效
        /// </summary>
        public static void OnExplosion(Scene scene, Vector3 explosionPos)
        {
            // 发布3D音效事件
            scene.GetComponent<EventComponent>().Publish(scene, new PlaySound3DEvent
            {
                Address = "Audio_SFX_Explosion",
                Position = explosionPos
            });
        }
        
        /// <summary>
        /// 玩家获胜时播放胜利音乐
        /// </summary>
        public static void OnVictory(Scene scene)
        {
            // 发布音乐事件
            scene.GetComponent<EventComponent>().Publish(scene, new PlayMusicEvent
            {
                Address = "Audio_BGM_Victory",
                Loop = false
            });
        }
        
        /// <summary>
        /// UI按钮点击
        /// </summary>
        public static void OnButtonClick(Scene scene)
        {
            // 发布音效事件
            scene.GetComponent<EventComponent>().Publish(scene, new PlaySoundEvent
            {
                Address = "Audio_SFX_Click"
            });
        }
    }
}
```

## 示例4: 游戏设置中的音量控制

```csharp
using ET;
using ET.Client;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 游戏设置面板
    /// </summary>
    [ComponentOf(typeof(YIUIChild))]
    public class SettingsPanelComponent : Entity, IAwake, IDestroy,
        IYIUIBind,
        IYIUIInitialize
    {
        // UI Slider组件引用
        // public UnityEngine.UI.Slider u_slider_music;
        // public UnityEngine.UI.Slider u_slider_sound;
    }
    
    [FriendOf(typeof(SettingsPanelComponent))]
    [EntitySystemOf(typeof(SettingsPanelComponent))]
    public static partial class SettingsPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SettingsPanelComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this SettingsPanelComponent self)
        {
        }
        
        [EntitySystem]
        private static void YIUIBind(this SettingsPanelComponent self)
        {
            // 绑定滑动条事件
            // self.u_slider_music.onValueChanged.AddListener(self.OnMusicVolumeChanged);
            // self.u_slider_sound.onValueChanged.AddListener(self.OnSoundVolumeChanged);
        }
        
        [EntitySystem]
        private static void YIUIInitialize(this SettingsPanelComponent self)
        {
            // 初始化滑动条值
            Scene clientScene = self.Root() as Scene;
            SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                // self.u_slider_music.value = soundComp.MusicVolume;
                // self.u_slider_sound.value = soundComp.SoundVolume;
            }
        }
        
        /// <summary>
        /// 音乐音量改变
        /// </summary>
        private static void OnMusicVolumeChanged(this SettingsPanelComponent self, float value)
        {
            Scene clientScene = self.Root() as Scene;
            SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();
            soundComp?.SetMusicVolume(value);
        }
        
        /// <summary>
        /// 音效音量改变
        /// </summary>
        private static void OnSoundVolumeChanged(this SettingsPanelComponent self, float value)
        {
            Scene clientScene = self.Root() as Scene;
            SoundComponent soundComp = clientScene.GetComponent<SoundComponent>();
            soundComp?.SetSoundVolume(value);
            
            // 播放测试音效
            soundComp?.PlaySound("Audio_SFX_Test").NoContext();
        }
    }
}
```

## 示例5: 场景切换时的音乐管理

```csharp
namespace ET.Client
{
    /// <summary>
    /// 场景管理音乐示例
    /// </summary>
    public static class SceneMusicManager
    {
        /// <summary>
        /// 进入主菜单场景（带淡入淡出）
        /// </summary>
        public static async ETTask EnterMainMenu(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null)
            {
                soundComp = scene.AddComponent<SoundComponent>();
            }
            
            // 使用淡入淡出切换到主菜单音乐
            await soundComp.PlayMusicWithFade("Audio_BGM_MainMenu", fadeOutDuration: 1.5f, fadeInDuration: 2.0f);
        }
        
        /// <summary>
        /// 进入战斗场景（带淡入淡出）
        /// </summary>
        public static async ETTask EnterBattle(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                // 平滑切换到战斗音乐
                await soundComp.PlayMusicWithFade("Audio_BGM_Battle", fadeOutDuration: 1.0f, fadeInDuration: 1.5f);
            }
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public static void PauseGame(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                // 暂停背景音乐
                soundComp.PauseMusic();
                
                // 停止所有音效
                soundComp.StopAllSounds();
            }
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public static void ResumeGame(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            soundComp?.ResumeMusic();
        }
        
        /// <summary>
        /// 退出游戏（淡出音乐）
        /// </summary>
        public static async ETTask ExitGame(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                // 淡出并停止音乐
                await soundComp.StopMusicWithFade(fadeOutDuration: 2.0f);
            }
        }
    }
}
```

## 示例6: 高级音乐切换控制

```csharp
namespace ET.Client
{
    /// <summary>
    /// 高级音乐管理示例
    /// </summary>
    public static class AdvancedMusicManager
    {
        /// <summary>
        /// 根据游戏状态动态切换音乐
        /// </summary>
        public static async ETTask SwitchMusicByGameState(Scene scene, GameState gameState)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null) return;
            
            string musicAddress = gameState switch
            {
                GameState.MainMenu => "Audio_BGM_MainMenu",
                GameState.Battle => "Audio_BGM_Battle",
                GameState.Victory => "Audio_BGM_Victory",
                GameState.Defeat => "Audio_BGM_Defeat",
                GameState.Shop => "Audio_BGM_Shop",
                _ => null
            };
            
            if (!string.IsNullOrEmpty(musicAddress))
            {
                // 使用淡入淡出平滑切换
                await soundComp.PlayMusicWithFade(musicAddress, fadeOutDuration: 1.0f, fadeInDuration: 1.5f);
            }
        }
        
        /// <summary>
        /// Boss战音乐切换（快速切换）
        /// </summary>
        public static async ETTask SwitchToBossMusic(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null) return;
            
            // 快速淡入淡出，营造紧张感
            await soundComp.PlayMusicWithFade("Audio_BGM_Boss", fadeOutDuration: 0.3f, fadeInDuration: 0.5f);
        }
        
        /// <summary>
        /// 关卡完成音乐（慢速淡入）
        /// </summary>
        public static async ETTask PlayVictoryMusic(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null) return;
            
            // 慢速淡入，营造庆祝氛围
            await soundComp.PlayMusicWithFade("Audio_BGM_Victory", 
                fadeOutDuration: 0.5f, 
                fadeInDuration: 3.0f, 
                loop: false); // 胜利音乐不循环
        }
        
        /// <summary>
        /// 使用辅助类快速切换音乐（不等待）
        /// </summary>
        public static void QuickSwitchMusic(Scene scene, string musicAddress)
        {
            // 使用AudioHelper快速切换（不阻塞当前逻辑）
            AudioHelper.PlayMusicWithFadeQuick(scene, musicAddress, 
                fadeOutDuration: 1.0f, 
                fadeInDuration: 1.5f);
        }
    }
    
    public enum GameState
    {
        MainMenu,
        Battle,
        Victory,
        Defeat,
        Shop
    }
}
```

## 示例6: Match3游戏中的音效

```csharp
namespace ET.Client
{
    /// <summary>
    /// 三消游戏音效管理
    /// </summary>
    public static class Match3AudioHelper
    {
        /// <summary>
        /// 播放糖果消除音效
        /// </summary>
        public static async ETTask PlayCandyMatchSound(Scene scene, int matchCount)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null) return;
            
            // 根据消除数量播放不同音效
            string soundAddress = matchCount switch
            {
                3 => "Audio_SFX_Match3",
                4 => "Audio_SFX_Match4",
                5 => "Audio_SFX_Match5",
                _ => "Audio_SFX_MatchSuper"
            };
            
            await soundComp.PlaySound(soundAddress);
        }
        
        /// <summary>
        /// 播放特殊糖果激活音效
        /// </summary>
        public static void PlaySpecialCandySound(Scene scene, string candyType)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null) return;
            
            string soundAddress = candyType switch
            {
                "Striped" => "Audio_SFX_StripedCandy",
                "Wrapped" => "Audio_SFX_WrappedCandy",
                "ColorBomb" => "Audio_SFX_ColorBomb",
                _ => "Audio_SFX_SpecialCandy"
            };
            
            soundComp.PlaySound(soundAddress).NoContext();
        }
        
        /// <summary>
        /// 播放Combo音效
        /// </summary>
        public static void PlayComboSound(Scene scene, int comboCount)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null) return;
            
            string soundAddress = $"Audio_SFX_Combo_{Mathf.Min(comboCount, 10)}";
            soundComp.PlaySound(soundAddress).NoContext();
        }
    }
}
```

## 资源地址命名建议

```
背景音乐 (BGM):
- Audio_BGM_MainMenu
- Audio_BGM_Battle
- Audio_BGM_Victory
- Audio_BGM_Defeat

UI音效 (SFX_UI):
- Audio_SFX_ButtonClick
- Audio_SFX_PanelOpen
- Audio_SFX_PanelClose
- Audio_SFX_TabSwitch

游戏音效 (SFX_Game):
- Audio_SFX_Match3
- Audio_SFX_Match4
- Audio_SFX_Match5
- Audio_SFX_Explosion
- Audio_SFX_Collect

特殊效果 (SFX_Special):
- Audio_SFX_PowerUp
- Audio_SFX_LevelUp
- Audio_SFX_Achievement
```

## 注意事项

1. **await的使用**: 如果不需要等待音效播放完成，使用 `.NoContext()` 方法
2. **EntityRef规范**: 代码中已经正确处理了await前后的Entity引用安全
3. **空引用检查**: 使用前检查SoundComponent是否为null
4. **资源预加载**: 对于频繁使用的音效，可以在游戏启动时预加载
5. **3D音效**: 需要正确设置AudioListener组件在摄像机上

