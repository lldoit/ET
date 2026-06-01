namespace ET.Client
{
    public static partial class AudioComponentSystem
    {
        private static AudioAgent PrepareAgent(this AudioComponent self, AudioGroup group, AudioPlayRequest request, AudioAssetHandle handle)
        {
            AudioAgent agent = group.SelectAgent(request.PlayParams, out AudioPlayErrorCode errorCode);
            if (agent == null)
            {
                self.FailLoadedRequest(request, handle, errorCode, $"No audio agent available: {request.GroupName}");
                return null;
            }

            if (agent.IsBusy)
            {
                agent.Stop(group, 0f, AudioStopReason.Replace, self.Scene(), self.Root().TimerComponent);
            }

            if (agent.SetAudioClip(handle))
            {
                return agent;
            }

            self.FailLoadedRequest(request, handle, AudioPlayErrorCode.SetAudioClipFailure, $"Set audio clip failed: {request.AssetName}");
            return null;
        }

        private static int StartPreparedAgent(this AudioComponent self, AudioGroup group, AudioAgent agent, AudioPlayRequest request)
        {
            agent.SerialId = request.SerialId;
            agent.AssetName = request.AssetName;
            agent.ApplyParams(request.PlayParams);
            agent.Play(group, request.PlayParams.FadeInSeconds, self.Root().TimerComponent);
            self.Requests.Remove(request.SerialId);
            self.PublishSuccess(request, agent);
            self.WatchEnd(agent).Coroutine();
            return request.SerialId;
        }

        private static void PublishSuccess(this AudioComponent self, AudioPlayRequest request, AudioAgent agent)
        {
            EventSystem.Instance.Publish(self.Scene(), new AudioPlaySuccess
            {
                SerialId = request.SerialId,
                AssetName = request.AssetName,
                GroupName = request.GroupName,
                Agent = agent
            });
        }
    }
}
