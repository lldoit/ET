using System;
using UnityEngine;

namespace ET.Client
{
    public static class AudioAgentSystem
    {
        public static void Initialize(this AudioAgent self, string groupName, Transform parent, int index)
        {
            self.GroupName = groupName;
            self.GameObject = new GameObject($"AudioAgent_{groupName}_{index}");
            self.GameObject.transform.SetParent(parent, false);
            self.AudioSource = self.GameObject.AddComponent<AudioSource>();
            self.AudioSource.playOnAwake = false;
            self.AudioSource.volume = 1f;
            self.AudioSource.pitch = 1f;
            self.State = AudioAgentState.Idle;
        }

        public static bool SetAudioClip(this AudioAgent self, AudioAssetHandle handle)
        {
            if (self.AudioSource == null || handle == null || !handle.IsValid)
            {
                return false;
            }

            self.ReleaseAsset();
            self.AssetHandle = handle;
            self.AudioSource.clip = handle.AudioClip;
            self.SetAudioClipTime = DateTime.UtcNow;
            return true;
        }

        public static void ApplyParams(this AudioAgent self, AudioPlayParams playParams)
        {
            AudioSource source = self.AudioSource;
            if (source == null || playParams == null)
            {
                return;
            }

            self.PlayParams = playParams;
            self.Priority = playParams.Priority;
            self.VolumeInGroup = Mathf.Clamp01(playParams.VolumeInGroup);
            source.time = Mathf.Clamp(playParams.Time, 0f, source.clip != null ? source.clip.length : 0f);
            source.mute = playParams.MuteInGroup;
            source.loop = playParams.Loop;
            source.pitch = playParams.Pitch;
            source.panStereo = Mathf.Clamp(playParams.PanStereo, -1f, 1f);
            source.spatialBlend = Mathf.Clamp01(playParams.SpatialBlend);
            source.maxDistance = Mathf.Max(0f, playParams.MaxDistance);
            source.dopplerLevel = Mathf.Max(0f, playParams.DopplerLevel);
        }

        public static void RefreshMute(this AudioAgent self, AudioGroup group)
        {
            if (self.AudioSource == null || group == null)
            {
                return;
            }

            self.AudioSource.mute = group.Mute || self.PlayParams?.MuteInGroup == true;
        }

        public static void RefreshVolume(this AudioAgent self, AudioGroup group)
        {
            if (self.AudioSource == null || group == null)
            {
                return;
            }

            float groupVolume = Mathf.Clamp01(group.Volume);
            self.AudioSource.volume = groupVolume * Mathf.Clamp01(self.VolumeInGroup) * Mathf.Clamp01(self.FadeFactor);
        }

        public static void Play(this AudioAgent self, AudioGroup group, float fadeInSeconds, TimerComponent timerComponent)
        {
            if (self.AudioSource == null || self.AudioSource.clip == null)
            {
                return;
            }

            self.State = AudioAgentState.Playing;
            self.FadeVersion++;
            self.FadeFactor = fadeInSeconds > 0f ? 0f : 1f;
            self.RefreshMute(group);
            self.RefreshVolume(group);
            self.AudioSource.Play();

            if (fadeInSeconds > 0f)
            {
                self.FadeTo(group, self.FadeVersion, 1f, fadeInSeconds, timerComponent).Coroutine();
            }
        }

        public static void Stop(this AudioAgent self, AudioGroup group, float fadeOutSeconds, AudioStopReason reason, Scene scene, TimerComponent timerComponent)
        {
            if (!self.IsBusy)
            {
                return;
            }

            self.State = AudioAgentState.Stopping;
            self.FadeVersion++;
            int version = self.FadeVersion;
            self.StopAsync(group, fadeOutSeconds, version, reason, scene, timerComponent).Coroutine();
        }

        public static void Pause(this AudioAgent self, AudioGroup group, float fadeOutSeconds, TimerComponent timerComponent)
        {
            if (!self.IsBusy || self.State == AudioAgentState.Paused)
            {
                return;
            }

            self.State = AudioAgentState.Pausing;
            self.FadeVersion++;
            self.PauseAsync(group, fadeOutSeconds, self.FadeVersion, timerComponent).Coroutine();
        }

        public static void Resume(this AudioAgent self, AudioGroup group, float fadeInSeconds, TimerComponent timerComponent)
        {
            if (!self.IsBusy || self.AudioSource == null || self.State != AudioAgentState.Paused)
            {
                return;
            }

            self.State = AudioAgentState.Playing;
            self.FadeVersion++;
            self.FadeFactor = fadeInSeconds > 0f ? 0f : 1f;
            self.RefreshVolume(group);
            self.AudioSource.UnPause();

            if (fadeInSeconds > 0f)
            {
                self.FadeTo(group, self.FadeVersion, 1f, fadeInSeconds, timerComponent).Coroutine();
            }
        }

        public static void Reset(this AudioAgent self, Scene scene, bool publishReset = true)
        {
            int serialId = self.SerialId;
            string groupName = self.GroupName;
            self.FadeVersion++;
            self.AudioSource?.Stop();
            if (self.AudioSource != null)
            {
                self.AudioSource.clip = null;
            }

            self.ReleaseAsset();
            self.SerialId = 0;
            self.AssetName = null;
            self.Priority = 0;
            self.VolumeInGroup = 1f;
            self.FadeFactor = 1f;
            self.PlayParams = null;
            self.State = AudioAgentState.Idle;

            if (publishReset && scene != null && !scene.IsDisposed && serialId != 0)
            {
                EventSystem.Instance.Publish(scene, new AudioAgentReset { SerialId = serialId, GroupName = groupName });
            }
        }

        public static void Destroy(this AudioAgent self)
        {
            self.FadeVersion++;
            self.ReleaseAsset();
            if (self.GameObject != null)
            {
                UnityEngine.Object.Destroy(self.GameObject);
            }

            self.GameObject = null;
            self.AudioSource = null;
        }

        private static void ReleaseAsset(this AudioAgent self)
        {
            self.AssetHandle?.Release();
            self.AssetHandle = null;
        }

        private static async ETTask FadeTo(this AudioAgent self, AudioGroup group, int version, float target, float duration, TimerComponent timerComponent)
        {
            float start = self.FadeFactor;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                await timerComponent.WaitFrameAsync();
                if (self.FadeVersion != version || self.AudioSource == null)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                self.FadeFactor = Mathf.Lerp(start, target, t);
                self.RefreshVolume(group);
            }

            if (self.FadeVersion == version)
            {
                self.FadeFactor = target;
                self.RefreshVolume(group);
            }
        }

        private static async ETTask StopAsync(this AudioAgent self, AudioGroup group, float fadeOutSeconds, int version, AudioStopReason reason, Scene scene, TimerComponent timerComponent)
        {
            EntityRef<Scene> sceneRef = scene;
            if (fadeOutSeconds > 0f)
            {
                await self.FadeTo(group, version, 0f, fadeOutSeconds, timerComponent);
            }

            if (self.FadeVersion != version)
            {
                return;
            }

            int serialId = self.SerialId;
            string assetName = self.AssetName;
            string groupName = self.GroupName;
            scene = sceneRef;
            self.Reset(scene);
            if (scene != null && !scene.IsDisposed && serialId != 0)
            {
                EventSystem.Instance.Publish(scene, new AudioPlayStopped
                {
                    SerialId = serialId,
                    AssetName = assetName,
                    GroupName = groupName,
                    Reason = reason
                });
            }
        }

        private static async ETTask PauseAsync(this AudioAgent self, AudioGroup group, float fadeOutSeconds, int version, TimerComponent timerComponent)
        {
            if (fadeOutSeconds > 0f)
            {
                await self.FadeTo(group, version, 0f, fadeOutSeconds, timerComponent);
            }

            if (self.FadeVersion != version || self.AudioSource == null)
            {
                return;
            }

            self.AudioSource.Pause();
            self.State = AudioAgentState.Paused;
        }

    }
}
