namespace ET.Client
{
    public static partial class AudioComponentSystem
    {
        private static bool CancelRequest(this AudioComponent self, int serialId, AudioStopReason reason)
        {
            if (!self.Requests.TryGetValue(serialId, out AudioPlayRequest request))
            {
                return false;
            }

            request.Cancelled = true;
            request.CancelReason = reason;
            return true;
        }

        private static void PublishFailure(this AudioComponent self, int serialId, string assetName, string groupName, AudioPlayErrorCode errorCode, string errorMessage)
        {
            EventSystem.Instance.Publish(self.Scene(), new AudioPlayFailure
            {
                SerialId = serialId,
                AssetName = assetName,
                GroupName = groupName,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            });
        }

        private static async ETTask WatchEnd(this AudioComponent self, AudioAgent agent)
        {
            int serialId = agent.SerialId;
            string assetName = agent.AssetName;
            string groupName = agent.GroupName;
            EntityRef<AudioComponent> selfRef = self;
            TimerComponent timerComponent = self.Root().TimerComponent;
            while (await self.ShouldKeepWatching(selfRef, timerComponent, agent, serialId))
            {
            }

            self = selfRef;
            self?.PublishNaturalEnd(agent, serialId, assetName, groupName);
        }

        private static async ETTask<bool> ShouldKeepWatching(this AudioComponent self, EntityRef<AudioComponent> selfRef, TimerComponent timerComponent, AudioAgent agent, int serialId)
        {
            await timerComponent.WaitFrameAsync();
            self = selfRef;
            return self != null &&
                   !self.IsDisposed &&
                   agent.SerialId == serialId &&
                   agent.State != AudioAgentState.Idle &&
                   (agent.State == AudioAgentState.Paused ||
                    agent.AudioSource == null ||
                    agent.PlayParams == null ||
                    agent.PlayParams.Loop ||
                    agent.AudioSource.isPlaying);
        }

        private static void PublishNaturalEnd(this AudioComponent self, AudioAgent agent, int serialId, string assetName, string groupName)
        {
            if (self == null || self.IsDisposed || agent.SerialId != serialId || agent.State == AudioAgentState.Idle)
            {
                return;
            }

            agent.Reset(self.Scene());
            EventSystem.Instance.Publish(self.Scene(), new AudioPlayEnd
            {
                SerialId = serialId,
                AssetName = assetName,
                GroupName = groupName
            });
        }
    }
}
