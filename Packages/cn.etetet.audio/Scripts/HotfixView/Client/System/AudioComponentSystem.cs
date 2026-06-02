using UnityEngine;
using UnityEngine.Audio;

namespace ET.Client
{
    [EntitySystemOf(typeof(AudioComponent))]
    public static partial class AudioComponentSystem
    {
        [EntitySystem]
        private static void Awake(this AudioComponent self)
        {
            self.AudioLoader = new YooAssetsAudioLoader();
            self.RootGameObject = new GameObject("AudioRoot");
            UnityEngine.Object.DontDestroyOnLoad(self.RootGameObject);
            self.AddGroup("Music", 1, AudioReplaceStrategy.ReplaceLowestPriority, false, 1f);
            self.AddGroup("Sound", 8, AudioReplaceStrategy.ReplaceOldestSameOrLowerPriority, false, 1f);
            self.AddGroup("Voice", 2, AudioReplaceStrategy.ReplaceOldestSameOrLowerPriority, false, 1f);
        }

        [EntitySystem]
        private static void Destroy(this AudioComponent self)
        {
            self.StopAllLoading(AudioStopReason.ComponentDestroy);
            Scene scene = self.Scene();
            foreach (AudioGroup group in self.Groups.Values)
            {
                group.StopAll(0f, AudioStopReason.ComponentDestroy, scene, null);
                group.Destroy();
            }

            self.Groups.Clear();
            self.Requests.Clear();
            if (self.RootGameObject != null)
            {
                UnityEngine.Object.Destroy(self.RootGameObject);
                self.RootGameObject = null;
            }
        }

        public static bool AddGroup(this AudioComponent self, string groupName, int agentCount, AudioReplaceStrategy strategy, bool mute, float volume)
        {
            if (string.IsNullOrWhiteSpace(groupName) || agentCount <= 0 || self.Groups.ContainsKey(groupName))
            {
                return false;
            }

            AudioGroup group = new AudioGroup();
            group.Initialize(groupName, agentCount, strategy, mute, volume, self.RootGameObject.transform);
            self.Groups.Add(groupName, group);
            return true;
        }

        public static bool Stop(this AudioComponent self, int serialId, float fadeOutSeconds = 0f)
        {
            if (self.CancelRequest(serialId, AudioStopReason.ManualStop))
            {
                return true;
            }

            foreach (AudioGroup group in self.Groups.Values)
            {
                if (group.Stop(serialId, fadeOutSeconds, AudioStopReason.ManualStop, self.Scene(), self.Root().TimerComponent))
                {
                    return true;
                }
            }

            return false;
        }

        public static void StopGroup(this AudioComponent self, string groupName, float fadeOutSeconds = 0f)
        {
            foreach (AudioPlayRequest request in self.Requests.Values)
            {
                if (request.GroupName == groupName)
                {
                    request.Cancelled = true;
                    request.CancelReason = AudioStopReason.StopGroup;
                }
            }

            if (self.Groups.TryGetValue(groupName, out AudioGroup group))
            {
                group.StopAll(fadeOutSeconds, AudioStopReason.StopGroup, self.Scene(), self.Root().TimerComponent);
            }
        }

        public static void StopAll(this AudioComponent self, float fadeOutSeconds = 0f)
        {
            self.StopAllLoading(AudioStopReason.StopAll);
            foreach (AudioGroup group in self.Groups.Values)
            {
                group.StopAll(fadeOutSeconds, AudioStopReason.StopAll, self.Scene(), self.Root().TimerComponent);
            }
        }

        public static void StopAllLoading(this AudioComponent self, AudioStopReason reason = AudioStopReason.LoadCancelled)
        {
            foreach (AudioPlayRequest request in self.Requests.Values)
            {
                request.Cancelled = true;
                request.CancelReason = reason;
            }
        }

        public static bool Pause(this AudioComponent self, int serialId, float fadeOutSeconds = 0f)
        {
            foreach (AudioGroup group in self.Groups.Values)
            {
                if (group.Pause(serialId, fadeOutSeconds, self.Root().TimerComponent))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Resume(this AudioComponent self, int serialId, float fadeInSeconds = 0f)
        {
            foreach (AudioGroup group in self.Groups.Values)
            {
                if (group.Resume(serialId, fadeInSeconds, self.Root().TimerComponent))
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetGroupMute(this AudioComponent self, string groupName, bool mute)
        {
            if (!self.Groups.TryGetValue(groupName, out AudioGroup group))
            {
                return;
            }

            group.Mute = mute;
            group.RefreshMute();
        }

        public static void SetGroupVolume(this AudioComponent self, string groupName, float volume)
        {
            if (!self.Groups.TryGetValue(groupName, out AudioGroup group))
            {
                return;
            }

            group.Volume = Mathf.Clamp01(volume);
            group.RefreshVolume();
        }

        public static void SetGroupMixerGroup(this AudioComponent self, string groupName, AudioMixerGroup mixerGroup)
        {
            if (!self.Groups.TryGetValue(groupName, out AudioGroup group))
            {
                return;
            }

            group.MixerGroup = mixerGroup;
            group.RefreshMixerGroup();
        }

        public static bool IsLoading(this AudioComponent self, int serialId)
        {
            return self.Requests.ContainsKey(serialId);
        }

        public static bool IsPlaying(this AudioComponent self, int serialId)
        {
            foreach (AudioGroup group in self.Groups.Values)
            {
                if (group.IsPlaying(serialId))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
