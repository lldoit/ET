namespace ET.Client
{
    using UnityEngine.Audio;

    public static class AudioHelper
    {
        public static AudioComponent Get(Scene scene)
        {
            AudioComponent audioComponent = scene.GetComponent<AudioComponent>();
            return audioComponent ?? scene.AddComponent<AudioComponent>();
        }

        public static ETTask<int> Play(Scene scene, string assetName, string groupName, AudioPlayParams playParams = null)
        {
            return Get(scene).Play(assetName, groupName, playParams);
        }

        public static ETTask<int> PlayMusic(Scene scene, string assetName, bool loop = true, float fadeInSeconds = 0f)
        {
            AudioPlayParams playParams = AudioPlayParams.Create(loop);
            playParams.FadeInSeconds = fadeInSeconds;
            return Get(scene).Play(assetName, "Music", playParams);
        }

        public static ETTask<int> PlaySound(Scene scene, string assetName, int priority = 0)
        {
            AudioPlayParams playParams = AudioPlayParams.Create();
            playParams.Priority = priority;
            return Get(scene).Play(assetName, "Sound", playParams);
        }

        public static bool Stop(Scene scene, int serialId, float fadeOutSeconds = 0f)
        {
            AudioComponent audioComponent = scene.GetComponent<AudioComponent>();
            return audioComponent != null && audioComponent.Stop(serialId, fadeOutSeconds);
        }

        public static void StopGroup(Scene scene, string groupName, float fadeOutSeconds = 0f)
        {
            scene.GetComponent<AudioComponent>()?.StopGroup(groupName, fadeOutSeconds);
        }

        public static void StopAll(Scene scene, float fadeOutSeconds = 0f)
        {
            scene.GetComponent<AudioComponent>()?.StopAll(fadeOutSeconds);
        }

        public static bool Pause(Scene scene, int serialId, float fadeOutSeconds = 0f)
        {
            AudioComponent audioComponent = scene.GetComponent<AudioComponent>();
            return audioComponent != null && audioComponent.Pause(serialId, fadeOutSeconds);
        }

        public static bool Resume(Scene scene, int serialId, float fadeInSeconds = 0f)
        {
            AudioComponent audioComponent = scene.GetComponent<AudioComponent>();
            return audioComponent != null && audioComponent.Resume(serialId, fadeInSeconds);
        }

        public static void SetGroupMute(Scene scene, string groupName, bool mute)
        {
            scene.GetComponent<AudioComponent>()?.SetGroupMute(groupName, mute);
        }

        public static void SetGroupVolume(Scene scene, string groupName, float volume)
        {
            scene.GetComponent<AudioComponent>()?.SetGroupVolume(groupName, volume);
        }

        public static void SetGroupMixerGroup(Scene scene, string groupName, AudioMixerGroup mixerGroup)
        {
            scene.GetComponent<AudioComponent>()?.SetGroupMixerGroup(groupName, mixerGroup);
        }
    }
}
