using System.Collections.Generic;
using System.Reflection;
using ET.Client;
using UnityEngine;

namespace ET.Test
{
    public class Audio_PlayModeSmoke_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            int destroyResult = this.VerifyDestroyDoesNotRequireActiveRoot();
            if (destroyResult != ErrorCode.ERR_Success)
            {
                return destroyResult;
            }

            Fiber fiber = context.Fiber ?? GetCurrentFiber();
            if (fiber?.Root == null)
            {
                Log.Debug("Audio smoke playback path requires an active Fiber root.");
                return ErrorCode.ERR_Success;
            }

            Scene scene = fiber.Root;
            bool addedTimer = false;
            if (scene.GetComponent<TimerComponent>() == null)
            {
                scene.AddComponent<TimerComponent>();
                addedTimer = true;
            }

            AudioComponent audioComponent = AudioHelper.Get(scene);
            SmokeAudioLoader loader = new(scene.TimerComponent);
            audioComponent.AudioLoader = loader;
            EntityRef<Scene> sceneRef = scene;
            EntityRef<AudioComponent> audioComponentRef = audioComponent;

            try
            {
                int playResult = await this.VerifyPlayPauseResumeAndStop(sceneRef, audioComponentRef);
                if (playResult != ErrorCode.ERR_Success)
                {
                    return playResult;
                }

                int groupResult = await this.VerifyGroupVolumeAndMute(sceneRef, audioComponentRef);
                if (groupResult != ErrorCode.ERR_Success)
                {
                    return groupResult;
                }

                int cancelResult = await this.VerifyCancelWhileLoading(sceneRef, audioComponentRef, loader);
                if (cancelResult != ErrorCode.ERR_Success)
                {
                    return cancelResult;
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                scene = sceneRef;
                if (scene != null && !scene.IsDisposed)
                {
                    scene.RemoveComponent<AudioComponent>();
                    if (addedTimer)
                    {
                        scene.RemoveComponent<TimerComponent>();
                    }
                }
            }
        }

        private async ETTask<int> VerifyPlayPauseResumeAndStop(EntityRef<Scene> sceneRef, EntityRef<AudioComponent> audioComponentRef)
        {
            Scene scene = sceneRef;
            AudioComponent audioComponent = audioComponentRef;
            int serialId = await AudioHelper.PlaySound(scene, "audio_smoke_sfx");
            scene = sceneRef;
            audioComponent = audioComponentRef;
            if (serialId <= 0 || !audioComponent.IsPlaying(serialId))
            {
                Log.Console("Audio smoke play should return a playing serial id.");
                return 1;
            }

            AudioAgent agent = this.FindAgent(audioComponent, "Sound", serialId);
            if (agent?.AudioSource == null || !agent.AudioSource.isPlaying)
            {
                Log.Console("Audio smoke play should start AudioSource.");
                return 2;
            }

            if (!AudioHelper.Pause(scene, serialId))
            {
                Log.Console("Audio smoke pause should find active serial id.");
                return 3;
            }

            await scene.TimerComponent.WaitFrameAsync();
            scene = sceneRef;
            audioComponent = audioComponentRef;
            if (agent.State != AudioAgentState.Paused)
            {
                Log.Console($"Audio smoke pause state mismatch: {agent.State}");
                return 4;
            }

            if (!AudioHelper.Resume(scene, serialId))
            {
                Log.Console("Audio smoke resume should find paused serial id.");
                return 5;
            }

            await scene.TimerComponent.WaitFrameAsync();
            scene = sceneRef;
            audioComponent = audioComponentRef;
            if (agent.State != AudioAgentState.Playing || !agent.AudioSource.isPlaying)
            {
                Log.Console("Audio smoke resume should restore playing AudioSource.");
                return 6;
            }

            if (!AudioHelper.Stop(scene, serialId))
            {
                Log.Console("Audio smoke stop should find active serial id.");
                return 7;
            }

            await scene.TimerComponent.WaitFrameAsync();
            scene = sceneRef;
            audioComponent = audioComponentRef;
            if (audioComponent.IsPlaying(serialId) || agent.SerialId != 0)
            {
                Log.Console("Audio smoke stop should reset the agent.");
                return 8;
            }

            return ErrorCode.ERR_Success;
        }

        private async ETTask<int> VerifyGroupVolumeAndMute(EntityRef<Scene> sceneRef, EntityRef<AudioComponent> audioComponentRef)
        {
            Scene scene = sceneRef;
            AudioComponent audioComponent = audioComponentRef;
            AudioPlayParams playParams = AudioPlayParams.Create(true);
            playParams.VolumeInGroup = 0.5f;
            int serialId = await AudioHelper.Play(scene, "audio_smoke_bgm", "Music", playParams);
            scene = sceneRef;
            audioComponent = audioComponentRef;
            AudioAgent agent = this.FindAgent(audioComponent, "Music", serialId);
            if (serialId <= 0 || agent?.AudioSource == null)
            {
                Log.Console("Audio smoke group test should create music agent.");
                return 11;
            }

            AudioHelper.SetGroupVolume(scene, "Music", 0.25f);
            if (!Mathf.Approximately(agent.AudioSource.volume, 0.125f))
            {
                Log.Console($"Audio smoke group volume mismatch: {agent.AudioSource.volume}");
                return 12;
            }

            AudioHelper.SetGroupMute(scene, "Music", true);
            if (!agent.AudioSource.mute)
            {
                Log.Console("Audio smoke group mute should mute active source.");
                return 13;
            }

            AudioHelper.SetGroupMute(scene, "Music", false);
            if (agent.AudioSource.mute)
            {
                Log.Console("Audio smoke group unmute should unmute active source.");
                return 14;
            }

            AudioHelper.SetGroupMixerGroup(scene, "Music", null);
            if (agent.AudioSource.outputAudioMixerGroup != null)
            {
                Log.Console("Audio smoke mixer group clear should apply to active source.");
                return 15;
            }

            AudioHelper.Stop(scene, serialId);
            await scene.TimerComponent.WaitFrameAsync();
            return ErrorCode.ERR_Success;
        }

        private async ETTask<int> VerifyCancelWhileLoading(EntityRef<Scene> sceneRef, EntityRef<AudioComponent> audioComponentRef, SmokeAudioLoader loader)
        {
            Scene scene = sceneRef;
            AudioComponent audioComponent = audioComponentRef;
            loader.DelayNextLoadFrame = true;
            ETTask<int> playTask = AudioHelper.PlaySound(scene, "audio_smoke_cancel");
            int loadingSerialId = audioComponent.Serial;
            if (!audioComponent.IsLoading(loadingSerialId))
            {
                Log.Console("Audio smoke cancel should create a loading request before load completes.");
                return 21;
            }

            if (!AudioHelper.Stop(scene, loadingSerialId))
            {
                Log.Console("Audio smoke cancel should stop loading request.");
                return 22;
            }

            await scene.TimerComponent.WaitFrameAsync();
            scene = sceneRef;
            audioComponent = audioComponentRef;
            int serialId = await playTask;
            scene = sceneRef;
            audioComponent = audioComponentRef;
            if (serialId != 0)
            {
                Log.Console($"Audio smoke cancelled load should return 0, got {serialId}.");
                return 23;
            }

            if (audioComponent.IsPlaying(loadingSerialId))
            {
                Log.Console("Audio smoke cancelled load should not start playback.");
                return 24;
            }

            AudioAgent agent = this.FindAgent(audioComponent, "Sound", loadingSerialId);
            if (agent != null && agent.SerialId != 0)
            {
                Log.Console("Audio smoke cancelled load should not occupy an audio agent.");
                return 25;
            }

            return ErrorCode.ERR_Success;
        }

        private int VerifyDestroyDoesNotRequireActiveRoot()
        {
            AudioComponent audioComponent = new();
            MethodInfo destroy = typeof(AudioComponentSystem).GetMethod(
                "Destroy",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(AudioComponent) },
                null);

            if (destroy == null)
            {
                Log.Console("Audio smoke destroy lifecycle method should be discoverable.");
                return 31;
            }

            try
            {
                destroy.Invoke(null, new object[] { audioComponent });
            }
            catch (TargetInvocationException e)
            {
                Log.Console($"Audio smoke destroy should not require an active root: {e.InnerException?.GetType().Name}");
                return 32;
            }

            return ErrorCode.ERR_Success;
        }

        private AudioAgent FindAgent(AudioComponent audioComponent, string groupName, int serialId)
        {
            if (!audioComponent.Groups.TryGetValue(groupName, out AudioGroup group))
            {
                return null;
            }

            foreach (AudioAgent agent in group.Agents)
            {
                if (agent.SerialId == serialId)
                {
                    return agent;
                }
            }

            return null;
        }

        private static Fiber GetCurrentFiber()
        {
            return typeof(Fiber).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as Fiber;
        }

        private sealed class SmokeAudioLoader : IAudioLoader
        {
            private readonly EntityRef<TimerComponent> timerComponent;
            private readonly Dictionary<string, AudioClip> clips = new();

            public SmokeAudioLoader(TimerComponent timerComponent)
            {
                this.timerComponent = timerComponent;
            }

            public bool DelayNextLoadFrame { get; set; }

            public async ETTask<AudioAssetHandle> LoadAsync(string assetName)
            {
                if (this.DelayNextLoadFrame)
                {
                    this.DelayNextLoadFrame = false;
                    TimerComponent timer = this.timerComponent;
                    await timer.WaitFrameAsync();
                }

                if (!this.clips.TryGetValue(assetName, out AudioClip clip))
                {
                    clip = AudioClip.Create(assetName, 44100, 1, 44100, false);
                    this.clips.Add(assetName, clip);
                }

                return new AudioAssetHandle(assetName, clip, null);
            }
        }
    }
}
