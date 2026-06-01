using UnityEngine;

namespace ET.Client
{
    public static class AudioGroupSystem
    {
        public static void Initialize(this AudioGroup self, string name, int agentCount, AudioReplaceStrategy strategy, bool mute, float volume, Transform root)
        {
            self.Name = name;
            self.ReplaceStrategy = strategy;
            self.Mute = mute;
            self.Volume = Mathf.Clamp01(volume);

            GameObject groupObject = new GameObject($"AudioGroup_{name}");
            groupObject.transform.SetParent(root, false);
            for (int i = 0; i < agentCount; i++)
            {
                AudioAgent agent = new AudioAgent();
                agent.Initialize(name, groupObject.transform, i);
                self.Agents.Add(agent);
            }
        }

        public static AudioAgent SelectAgent(this AudioGroup self, AudioPlayParams playParams, out AudioPlayErrorCode errorCode)
        {
            errorCode = AudioPlayErrorCode.None;
            AudioAgent candidate = null;

            foreach (AudioAgent agent in self.Agents)
            {
                if (!agent.IsBusy)
                {
                    return agent;
                }
            }

            switch (self.ReplaceStrategy)
            {
                case AudioReplaceStrategy.RejectWhenFull:
                    break;
                case AudioReplaceStrategy.ReplaceLowestPriority:
                    candidate = self.SelectLowestPriority(playParams.Priority, false);
                    break;
                case AudioReplaceStrategy.ReplaceOldestSameOrLowerPriority:
                    candidate = self.SelectLowestPriority(playParams.Priority, true);
                    break;
                case AudioReplaceStrategy.ReplaceOldest:
                    candidate = self.SelectOldest();
                    break;
            }

            if (candidate == null)
            {
                errorCode = AudioPlayErrorCode.IgnoredDueToLowPriority;
            }

            return candidate;
        }

        public static bool IsPlaying(this AudioGroup self, int serialId)
        {
            AudioAgent agent = self.Find(serialId);
            return agent != null && agent.IsPlaying;
        }

        public static bool Stop(this AudioGroup self, int serialId, float fadeOutSeconds, AudioStopReason reason, Scene scene, TimerComponent timerComponent)
        {
            AudioAgent agent = self.Find(serialId);
            if (agent == null)
            {
                return false;
            }

            agent.Stop(self, fadeOutSeconds, reason, scene, timerComponent);
            return true;
        }

        public static bool Pause(this AudioGroup self, int serialId, float fadeOutSeconds, TimerComponent timerComponent)
        {
            AudioAgent agent = self.Find(serialId);
            if (agent == null)
            {
                return false;
            }

            agent.Pause(self, fadeOutSeconds, timerComponent);
            return true;
        }

        public static bool Resume(this AudioGroup self, int serialId, float fadeInSeconds, TimerComponent timerComponent)
        {
            AudioAgent agent = self.Find(serialId);
            if (agent == null)
            {
                return false;
            }

            agent.Resume(self, fadeInSeconds, timerComponent);
            return true;
        }

        public static void StopAll(this AudioGroup self, float fadeOutSeconds, AudioStopReason reason, Scene scene, TimerComponent timerComponent)
        {
            foreach (AudioAgent agent in self.Agents)
            {
                agent.Stop(self, fadeOutSeconds, reason, scene, timerComponent);
            }
        }

        public static void RefreshMute(this AudioGroup self)
        {
            foreach (AudioAgent agent in self.Agents)
            {
                agent.RefreshMute(self);
            }
        }

        public static void RefreshVolume(this AudioGroup self)
        {
            foreach (AudioAgent agent in self.Agents)
            {
                agent.RefreshVolume(self);
            }
        }

        public static void Destroy(this AudioGroup self)
        {
            foreach (AudioAgent agent in self.Agents)
            {
                agent.Destroy();
            }

            self.Agents.Clear();
        }

        private static AudioAgent Find(this AudioGroup self, int serialId)
        {
            foreach (AudioAgent agent in self.Agents)
            {
                if (agent.SerialId == serialId)
                {
                    return agent;
                }
            }

            return null;
        }

        private static AudioAgent SelectLowestPriority(this AudioGroup self, int priority, bool allowSamePriority)
        {
            AudioAgent candidate = null;
            foreach (AudioAgent agent in self.Agents)
            {
                bool canReplace = allowSamePriority ? agent.Priority <= priority : agent.Priority < priority;
                if (!canReplace)
                {
                    continue;
                }

                if (candidate == null ||
                    agent.Priority < candidate.Priority ||
                    agent.Priority == candidate.Priority && agent.SetAudioClipTime < candidate.SetAudioClipTime)
                {
                    candidate = agent;
                }
            }

            return candidate;
        }

        private static AudioAgent SelectOldest(this AudioGroup self)
        {
            AudioAgent candidate = null;
            foreach (AudioAgent agent in self.Agents)
            {
                if (candidate == null || agent.SetAudioClipTime < candidate.SetAudioClipTime)
                {
                    candidate = agent;
                }
            }

            return candidate;
        }
    }
}
