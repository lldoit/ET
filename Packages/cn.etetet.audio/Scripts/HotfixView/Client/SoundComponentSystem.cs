using UnityEngine;
using YooAsset;

namespace ET.Client
{
    [FriendOf(typeof(SoundComponent))]
    [EntitySystemOf(typeof(SoundComponent))]
    public static partial class SoundComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this SoundComponent self)
        {
            Scene scene = self.GetParent<Scene>();
            
            // 获取Global/Audio节点，如果不存在则创建
            Transform globalTransform = GameObject.Find("/Global")?.transform;
            if (globalTransform == null)
            {
                Log.Error("找不到/Global节点，无法初始化SoundComponent");
                return;
            }

            Transform audioTransform = globalTransform.Find("Audio");
            if (audioTransform == null)
            {
                GameObject audioObj = new GameObject("Audio");
                audioObj.transform.SetParent(globalTransform);
                audioTransform = audioObj.transform;
            }
            
            // 初始化背景音乐播放器
            self.MusicSource = audioTransform.gameObject.AddComponent<AudioSource>();
            self.MusicSource.loop = true;
            self.MusicSource.playOnAwake = false;
        }

        [EntitySystem]
        private static void Destroy(this SoundComponent self)
        {
            // 取消淡入淡出
            if (self.FadeCancellationToken != null)
            {
                self.FadeCancellationToken.Cancel();
                self.FadeCancellationToken = null;
            }
            
            // 停止所有正在播放的音效
            foreach (AudioSource source in self.ActiveSounds)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }
            self.ActiveSounds.Clear();
            
            // 销毁对象池中的AudioSource
            while (self.SoundPool.Count > 0)
            {
                AudioSource source = self.SoundPool.Dequeue();
                if (source != null && source.gameObject != null)
                {
                    UnityEngine.Object.Destroy(source.gameObject);
                }
            }
            
            // 释放所有YooAsset句柄
            foreach (var handle in self.Handles.Values)
            {
                if (handle != null)
                {
                    handle.Release();
                }
            }
            self.Handles.Clear();
            self.AudioCache.Clear();
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 异步加载音频剪辑
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="address">资源地址</param>
        /// <returns>加载的AudioClip，失败返回null</returns>
        private static async ETTask<AudioClip> GetAudioClip(this SoundComponent self, string address)
        {
            // 检查缓存
            if (self.AudioCache.TryGetValue(address, out AudioClip clip))
            {
                return clip;
            }

            // 使用协程锁避免重复加载同一资源
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>()
                    .Wait(CoroutineLockType.AudioLoader, address.GetHashCode());

            // 再次检查缓存（可能在等待锁的期间已经被加载）
            if (self.AudioCache.TryGetValue(address, out clip))
            {
                return clip;
            }

            try
            {
                // 使用YooAsset加载资源
                AssetHandle handle = YooAssets.LoadAssetAsync<AudioClip>(address);
                await handle.Task;

                if (handle.Status == EOperationStatus.Succeed)
                {
                    clip = handle.AssetObject as AudioClip;
                    self.Handles[address] = handle;
                    self.AudioCache[address] = clip;
                    return clip;
                }
                else
                {
                    Log.Error($"加载音频资源失败: {address}");
                    handle.Release();
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"加载音频资源异常: {address}, 错误: {e}");
                return null;
            }
        }

        #endregion

        #region 对象池管理

        /// <summary>
        /// 从对象池获取或创建一个可用的AudioSource
        /// </summary>
        private static AudioSource GetFreeSource(this SoundComponent self)
        {
            AudioSource source;
            
            if (self.SoundPool.Count > 0)
            {
                // 从对象池中获取
                source = self.SoundPool.Dequeue();
            }
            else
            {
                // 对象池为空，创建新的AudioSource
                Transform globalTransform = GameObject.Find("/Global")?.transform;
                Transform audioTransform = globalTransform?.Find("Audio");
                
                if (audioTransform == null)
                {
                    Log.Error("找不到/Global/Audio节点，无法创建AudioSource");
                    return null;
                }
                
                GameObject soundObj = new GameObject("PooledAudioSource");
                soundObj.transform.SetParent(audioTransform);
                source = soundObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
            }
            
            source.gameObject.SetActive(true);
            self.ActiveSounds.Add(source);
            return source;
        }

        /// <summary>
        /// 回收AudioSource到对象池
        /// </summary>
        private static void RecycleSource(this SoundComponent self, AudioSource source)
        {
            if (source == null)
            {
                return;
            }
            
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.gameObject.SetActive(false);
            self.ActiveSounds.Remove(source);

            // 如果对象池未满，回收；否则销毁
            if (self.SoundPool.Count < self.MaxPoolSize)
            {
                self.SoundPool.Enqueue(source);
            }
            else
            {
                UnityEngine.Object.Destroy(source.gameObject);
            }
        }

        #endregion

        #region 背景音乐播放

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="address">音频资源地址</param>
        /// <param name="loop">是否循环播放，默认true</param>
        public static async ETTask PlayMusic(this SoundComponent self, string address, bool loop = true)
        {
            if (string.IsNullOrEmpty(address))
            {
                Log.Warning("音乐资源地址为空");
                return;
            }

            AudioClip clip = await self.GetAudioClip(address);
            if (clip == null)
            {
                return;
            }

            self.MusicSource.clip = clip;
            self.MusicSource.loop = loop;
            self.MusicSource.volume = self.MusicVolume;
            self.MusicSource.Play();
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public static void StopMusic(this SoundComponent self)
        {
            if (self.MusicSource != null)
            {
                self.MusicSource.Stop();
            }
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public static void PauseMusic(this SoundComponent self)
        {
            if (self.MusicSource != null)
            {
                self.MusicSource.Pause();
            }
        }

        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        public static void ResumeMusic(this SoundComponent self)
        {
            if (self.MusicSource != null)
            {
                self.MusicSource.UnPause();
            }
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="volume">音量值 (0.0 - 1.0)</param>
        public static void SetMusicVolume(this SoundComponent self, float volume)
        {
            self.MusicVolume = Mathf.Clamp01(volume);
            if (self.MusicSource != null && !self.IsFading)
            {
                self.MusicSource.volume = self.MusicVolume;
            }
        }

        /// <summary>
        /// 带淡入淡出效果切换背景音乐
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="address">音频资源地址</param>
        /// <param name="fadeOutDuration">淡出时长（秒）</param>
        /// <param name="fadeInDuration">淡入时长（秒）</param>
        /// <param name="loop">是否循环播放，默认true</param>
        public static async ETTask PlayMusicWithFade(this SoundComponent self, string address, float fadeOutDuration = 1.0f, float fadeInDuration = 1.0f, bool loop = true)
        {
            if (string.IsNullOrEmpty(address))
            {
                Log.Warning("音乐资源地址为空");
                return;
            }

            // 如果正在播放相同的音乐，不需要切换
            if (self.CurrentMusicAddress == address && self.MusicSource != null && self.MusicSource.isPlaying)
            {
                return;
            }

            // 取消之前的淡入淡出
            if (self.FadeCancellationToken != null)
            {
                self.FadeCancellationToken.Cancel();
                self.FadeCancellationToken = null;
            }

            // 创建新的取消令牌
            self.FadeCancellationToken = new ETCancellationToken();
            ETCancellationToken cancellationToken = self.FadeCancellationToken;

            // 在await前创建EntityRef
            EntityRef<SoundComponent> selfRef = self;

            // 淡出当前音乐
            if (self.MusicSource != null && self.MusicSource.isPlaying)
            {
                await self.FadeOutMusic(fadeOutDuration, cancellationToken);
                
                // await后重新获取Entity并检查是否被取消
                self = selfRef;
                if (self == null || self.IsDisposed || cancellationToken.IsCancel())
                {
                    return;
                }
            }

            // 加载新音乐
            AudioClip clip = await self.GetAudioClip(address);
            
            // await后重新获取Entity并检查
            self = selfRef;
            if (self == null || self.IsDisposed || cancellationToken.IsCancel())
            {
                return;
            }

            if (clip == null)
            {
                return;
            }

            // 设置新音乐
            self.MusicSource.clip = clip;
            self.MusicSource.loop = loop;
            self.MusicSource.volume = 0f;
            self.MusicSource.Play();
            self.CurrentMusicAddress = address;

            // 淡入新音乐
            await self.FadeInMusic(fadeInDuration, cancellationToken);
        }

        /// <summary>
        /// 淡出背景音乐
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="duration">淡出时长（秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        private static async ETTask FadeOutMusic(this SoundComponent self, float duration, ETCancellationToken cancellationToken)
        {
            if (self.MusicSource == null || !self.MusicSource.isPlaying)
            {
                return;
            }

            self.IsFading = true;
            float startVolume = self.MusicSource.volume;
            float elapsedTime = 0f;

            // 在await前创建EntityRef
            EntityRef<SoundComponent> selfRef = self;

            while (elapsedTime < duration)
            {
                // 检查是否被取消
                if (cancellationToken.IsCancel())
                {
                    return;
                }

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                
                // await后重新获取Entity
                self = selfRef;
                if (self == null || self.IsDisposed || self.MusicSource == null)
                {
                    return;
                }

                self.MusicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                
                await self.Root().GetComponent<TimerComponent>().WaitFrameAsync();
            }

            // 最后确保音量为0
            self = selfRef;
            if (self != null && !self.IsDisposed && self.MusicSource != null)
            {
                self.MusicSource.volume = 0f;
                self.MusicSource.Stop();
                self.IsFading = false;
            }
        }

        /// <summary>
        /// 淡入背景音乐
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="duration">淡入时长（秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        private static async ETTask FadeInMusic(this SoundComponent self, float duration, ETCancellationToken cancellationToken)
        {
            if (self.MusicSource == null || !self.MusicSource.isPlaying)
            {
                return;
            }

            self.IsFading = true;
            float targetVolume = self.MusicVolume;
            float elapsedTime = 0f;

            // 在await前创建EntityRef
            EntityRef<SoundComponent> selfRef = self;

            while (elapsedTime < duration)
            {
                // 检查是否被取消
                if (cancellationToken.IsCancel())
                {
                    return;
                }

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                
                // await后重新获取Entity
                self = selfRef;
                if (self == null || self.IsDisposed || self.MusicSource == null)
                {
                    return;
                }

                self.MusicSource.volume = Mathf.Lerp(0f, targetVolume, t);
                
                await self.Root().GetComponent<TimerComponent>().WaitFrameAsync();
            }

            // 最后确保音量正确
            self = selfRef;
            if (self != null && !self.IsDisposed && self.MusicSource != null)
            {
                self.MusicSource.volume = targetVolume;
                self.IsFading = false;
            }
        }

        /// <summary>
        /// 淡出并停止背景音乐
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="fadeOutDuration">淡出时长（秒）</param>
        public static async ETTask StopMusicWithFade(this SoundComponent self, float fadeOutDuration = 1.0f)
        {
            // 取消之前的淡入淡出
            if (self.FadeCancellationToken != null)
            {
                self.FadeCancellationToken.Cancel();
                self.FadeCancellationToken = null;
            }

            // 创建新的取消令牌
            self.FadeCancellationToken = new ETCancellationToken();
            ETCancellationToken cancellationToken = self.FadeCancellationToken;

            // 在await前创建EntityRef
            EntityRef<SoundComponent> selfRef = self;

            // 淡出音乐
            await self.FadeOutMusic(fadeOutDuration, cancellationToken);
            
            // await后重新获取Entity
            self = selfRef;
            if (self != null && !self.IsDisposed)
            {
                self.CurrentMusicAddress = null;
            }
        }

        #endregion

        #region 音效播放

        /// <summary>
        /// 播放音效（带对象池管理）
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="address">音频资源地址</param>
        public static async ETTask PlaySound(this SoundComponent self, string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                Log.Warning("音效资源地址为空");
                return;
            }

            // 在await前创建EntityRef
            EntityRef<SoundComponent> selfRef = self;

            AudioClip clip = await self.GetAudioClip(address);
            
            // await后重新获取Entity
            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }

            if (clip == null)
            {
                return;
            }

            AudioSource source = self.GetFreeSource();
            if (source == null)
            {
                return;
            }

            source.clip = clip;
            source.volume = self.SoundVolume;
            source.loop = false;
            source.Play();

            // 等待音频播放结束
            long duration = (long)(clip.length * 1000);
            await self.Root().GetComponent<TimerComponent>().WaitAsync(duration);

            // await后再次检查Entity是否有效
            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }

            // 回收AudioSource
            self.RecycleSource(source);
        }

        /// <summary>
        /// 播放3D音效
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="address">音频资源地址</param>
        /// <param name="position">3D空间位置</param>
        public static async ETTask PlaySound3D(this SoundComponent self, string address, Vector3 position)
        {
            if (string.IsNullOrEmpty(address))
            {
                Log.Warning("音效资源地址为空");
                return;
            }

            // 在await前创建EntityRef
            EntityRef<SoundComponent> selfRef = self;

            AudioClip clip = await self.GetAudioClip(address);
            
            // await后重新获取Entity
            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }

            if (clip == null)
            {
                return;
            }

            AudioSource source = self.GetFreeSource();
            if (source == null)
            {
                return;
            }

            source.transform.position = position;
            source.spatialBlend = 1.0f; // 设为3D音效
            source.clip = clip;
            source.volume = self.SoundVolume;
            source.loop = false;
            source.Play();

            // 等待音频播放结束
            long duration = (long)(clip.length * 1000);
            await self.Root().GetComponent<TimerComponent>().WaitAsync(duration);

            // await后再次检查Entity是否有效
            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }

            // 回收AudioSource
            self.RecycleSource(source);
        }

        /// <summary>
        /// 停止所有音效
        /// </summary>
        public static void StopAllSounds(this SoundComponent self)
        {
            foreach (AudioSource source in self.ActiveSounds)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="volume">音量值 (0.0 - 1.0)</param>
        public static void SetSoundVolume(this SoundComponent self, float volume)
        {
            self.SoundVolume = Mathf.Clamp01(volume);
            
            // 更新所有正在播放的音效音量
            foreach (AudioSource source in self.ActiveSounds)
            {
                if (source != null)
                {
                    source.volume = self.SoundVolume;
                }
            }
        }

        /// <summary>
        /// 设置对象池最大大小
        /// </summary>
        /// <param name="self">SoundComponent实例</param>
        /// <param name="maxSize">对象池最大大小</param>
        public static void SetMaxPoolSize(this SoundComponent self, int maxSize)
        {
            self.MaxPoolSize = Mathf.Max(1, maxSize);
        }

        #endregion
    }
}

