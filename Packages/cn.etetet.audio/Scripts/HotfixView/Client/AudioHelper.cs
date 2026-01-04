using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 音频系统辅助工具类
    /// 提供便捷的音频初始化和访问方法
    /// </summary>
    public static class AudioHelper
    {
        /// <summary>
        /// 快速播放音效（不等待完成）
        /// </summary>
        /// <param name="scene">客户端场景</param>
        /// <param name="address">音频资源地址</param>
        public static void PlaySoundQuick(Scene scene, string address)
        {
            SoundComponent soundComp = scene?.GetComponent<SoundComponent>();
            soundComp?.PlaySound(address).NoContext();
        }
        
        /// <summary>
        /// 快速播放3D音效（不等待完成）
        /// </summary>
        /// <param name="scene">客户端场景</param>
        /// <param name="address">音频资源地址</param>
        /// <param name="position">3D空间位置</param>
        public static void PlaySound3DQuick(Scene scene, string address, Vector3 position)
        {
            SoundComponent soundComp = scene?.GetComponent<SoundComponent>();
            soundComp?.PlaySound3D(address, position).NoContext();
        }
        
        /// <summary>
        /// 快速播放背景音乐（不等待完成）
        /// </summary>
        /// <param name="scene">客户端场景</param>
        /// <param name="address">音频资源地址</param>
        /// <param name="loop">是否循环</param>
        public static void PlayMusicQuick(Scene scene, string address, bool loop = true)
        {
            SoundComponent soundComp = scene?.GetComponent<SoundComponent>();
            soundComp?.PlayMusic(address, loop).NoContext();
        }
        
        /// <summary>
        /// 快速播放带淡入淡出效果的背景音乐（不等待完成）
        /// </summary>
        /// <param name="scene">客户端场景</param>
        /// <param name="address">音频资源地址</param>
        /// <param name="fadeOutDuration">淡出时长（秒）</param>
        /// <param name="fadeInDuration">淡入时长（秒）</param>
        /// <param name="loop">是否循环</param>
        public static void PlayMusicWithFadeQuick(Scene scene, string address, float fadeOutDuration = 1.0f, float fadeInDuration = 1.0f, bool loop = true)
        {
            SoundComponent soundComp = scene?.GetComponent<SoundComponent>();
            soundComp?.PlayMusicWithFade(address, fadeOutDuration, fadeInDuration, loop).NoContext();
        }
        
        /// <summary>
        /// 快速停止背景音乐（带淡出效果，不等待完成）
        /// </summary>
        /// <param name="scene">客户端场景</param>
        /// <param name="fadeOutDuration">淡出时长（秒）</param>
        public static void StopMusicWithFadeQuick(Scene scene, float fadeOutDuration = 1.0f)
        {
            SoundComponent soundComp = scene?.GetComponent<SoundComponent>();
            soundComp?.StopMusicWithFade(fadeOutDuration).NoContext();
        }
        
        /// <summary>
        /// 设置主音量（同时调整音乐和音效）
        /// </summary>
        /// <param name="scene">客户端场景</param>
        /// <param name="volume">音量值 (0.0 - 1.0)</param>
        public static void SetMasterVolume(Scene scene, float volume)
        {
            SoundComponent soundComp = scene?.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                soundComp.SetMusicVolume(volume);
                soundComp.SetSoundVolume(volume);
            }
        }
        
        /// <summary>
        /// 检查音频组件是否已初始化
        /// </summary>
        /// <param name="scene">客户端场景</param>
        /// <returns>是否已初始化</returns>
        public static bool IsAudioInitialized(Scene scene)
        {
            return scene?.GetComponent<SoundComponent>() != null;
        }
        
        /// <summary>
        /// 初始化音频系统（如果尚未初始化）
        /// </summary>
        /// <param name="scene">客户端场景</param>
        /// <param name="musicVolume">背景音乐音量</param>
        /// <param name="soundVolume">音效音量</param>
        /// <param name="poolSize">对象池大小</param>
        /// <returns>SoundComponent实例</returns>
        public static SoundComponent EnsureAudioInitialized(Scene scene, float musicVolume = 0.8f, float soundVolume = 0.8f, int poolSize = 10)
        {
            if (scene == null)
            {
                Log.Error("Scene为null，无法初始化音频系统");
                return null;
            }
            
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null)
            {
                soundComp = scene.AddComponent<SoundComponent>();
                soundComp.SetMusicVolume(musicVolume);
                soundComp.SetSoundVolume(soundVolume);
                soundComp.SetMaxPoolSize(poolSize);
                Log.Info($"音频系统已初始化 - 音乐音量:{musicVolume}, 音效音量:{soundVolume}, 对象池大小:{poolSize}");
            }
            
            return soundComp;
        }
    }
}

