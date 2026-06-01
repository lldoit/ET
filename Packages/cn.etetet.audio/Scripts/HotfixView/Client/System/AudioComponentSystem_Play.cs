namespace ET.Client
{
    public static partial class AudioComponentSystem
    {
        public static async ETTask<int> Play(this AudioComponent self, string assetName, string groupName, AudioPlayParams playParams = null, object userData = null)
        {
            if (!self.TryValidatePlay(assetName, groupName))
            {
                return 0;
            }

            AudioPlayRequest request = self.CreateRequest(assetName, groupName, playParams, userData);
            EntityRef<AudioComponent> selfRef = self;
            AudioAssetHandle handle = await self.AudioLoader.LoadAsync(assetName);
            self = selfRef;
            return self == null || self.IsDisposed ? self.ReleaseLoadedHandle(handle) : self.CompletePlay(request.SerialId, handle);
        }

        private static bool TryValidatePlay(this AudioComponent self, string assetName, string groupName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
            {
                self.PublishFailure(0, assetName, groupName, AudioPlayErrorCode.AssetNameInvalid, "Audio asset name is invalid.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(groupName) && self.Groups.ContainsKey(groupName))
            {
                return true;
            }

            self.PublishFailure(0, assetName, groupName, AudioPlayErrorCode.GroupNotFound, $"Audio group not found: {groupName}");
            return false;
        }

        private static AudioPlayRequest CreateRequest(this AudioComponent self, string assetName, string groupName, AudioPlayParams playParams, object userData)
        {
            AudioPlayRequest request = new AudioPlayRequest
            {
                SerialId = ++self.Serial,
                AssetName = assetName,
                GroupName = groupName,
                PlayParams = (playParams ?? AudioPlayParams.Create()).Clone(),
                UserData = userData
            };
            self.Requests.Add(request.SerialId, request);
            return request;
        }

        private static int ReleaseLoadedHandle(this AudioComponent self, AudioAssetHandle handle)
        {
            handle?.Release();
            return 0;
        }

        private static int CompletePlay(this AudioComponent self, int serialId, AudioAssetHandle handle)
        {
            if (!self.TryGetRequest(serialId, handle, out AudioPlayRequest request))
            {
                return 0;
            }

            if (self.TryCancelLoadedRequest(request, handle))
            {
                return 0;
            }

            if (!self.TryGetPlayableGroup(request, handle, out AudioGroup group))
            {
                return 0;
            }

            AudioAgent agent = self.PrepareAgent(group, request, handle);
            return agent == null ? 0 : self.StartPreparedAgent(group, agent, request);
        }

        private static bool TryGetRequest(this AudioComponent self, int serialId, AudioAssetHandle handle, out AudioPlayRequest request)
        {
            if (self.Requests.TryGetValue(serialId, out request))
            {
                return true;
            }

            handle?.Release();
            return false;
        }

        private static bool TryCancelLoadedRequest(this AudioComponent self, AudioPlayRequest request, AudioAssetHandle handle)
        {
            if (!request.Cancelled)
            {
                return false;
            }

            self.Requests.Remove(request.SerialId);
            handle?.Release();
            EventSystem.Instance.Publish(self.Scene(), new AudioPlayCancelled
            {
                SerialId = request.SerialId,
                AssetName = request.AssetName,
                GroupName = request.GroupName,
                Reason = request.CancelReason
            });
            return true;
        }

        private static bool TryGetPlayableGroup(this AudioComponent self, AudioPlayRequest request, AudioAssetHandle handle, out AudioGroup group)
        {
            group = null;
            if (!self.CheckLoadedHandle(request, handle))
            {
                return false;
            }

            if (self.Groups.TryGetValue(request.GroupName, out group))
            {
                return true;
            }

            self.FailLoadedRequest(request, handle, AudioPlayErrorCode.GroupNotFound, $"Audio group not found: {request.GroupName}");
            return false;
        }

        private static bool CheckLoadedHandle(this AudioComponent self, AudioPlayRequest request, AudioAssetHandle handle)
        {
            if (handle != null && handle.IsValid)
            {
                return true;
            }

            self.Requests.Remove(request.SerialId);
            self.PublishFailure(request.SerialId, request.AssetName, request.GroupName, AudioPlayErrorCode.LoadAssetFailure, $"Load audio failed: {request.AssetName}");
            return false;
        }

        private static void FailLoadedRequest(this AudioComponent self, AudioPlayRequest request, AudioAssetHandle handle, AudioPlayErrorCode errorCode, string message)
        {
            self.Requests.Remove(request.SerialId);
            handle?.Release();
            self.PublishFailure(request.SerialId, request.AssetName, request.GroupName, errorCode, message);
        }
    }
}
