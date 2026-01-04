using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET.Client
{
    /// <summary>
    /// 音频管理组件
    /// 负责管理背景音乐和音效的播放
    /// 支持YooAsset资源加载和AudioSource对象池管理
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class SoundComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 背景音乐播放器
        /// </summary>
        public AudioSource MusicSource;
        
        /// <summary>
        /// 闲置的AudioSource对象池队列
        /// </summary>
        public Queue<AudioSource> SoundPool = new();
        
        /// <summary>
        /// 正在播放的音效列表（用于全局停止或音量控制）
        /// </summary>
        public List<AudioSource> ActiveSounds = new();
        
        /// <summary>
        /// 对象池最大大小，防止极端情况下内存溢出
        /// </summary>
        public int MaxPoolSize = 10;
        
        /// <summary>
        /// YooAsset资源句柄缓存 (Key: 资源地址, Value: YooAsset句柄)
        /// </summary>
        public Dictionary<string, AssetHandle> Handles = new();
        
        /// <summary>
        /// AudioClip缓存 (Key: 资源地址, Value: AudioClip)
        /// </summary>
        public Dictionary<string, AudioClip> AudioCache = new();
        
        /// <summary>
        /// 背景音乐音量 (0.0 - 1.0)
        /// </summary>
        public float MusicVolume = 1.0f;
        
        /// <summary>
        /// 音效音量 (0.0 - 1.0)
        /// </summary>
        public float SoundVolume = 1.0f;
        
        /// <summary>
        /// 是否正在淡入淡出
        /// </summary>
        public bool IsFading;
        
        /// <summary>
        /// 淡入淡出的取消令牌
        /// </summary>
        public ETCancellationToken FadeCancellationToken;
        
        /// <summary>
        /// 当前正在播放的音乐地址
        /// </summary>
        public string CurrentMusicAddress;
    }
}

