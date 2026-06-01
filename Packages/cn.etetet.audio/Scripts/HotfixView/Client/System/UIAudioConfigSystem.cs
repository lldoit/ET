using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET.Client
{
    public static class UIAudioConfigSystem
    {
        public static void BindUIAudio(this Scene scene, GameObject root)
        {
            if (scene == null || scene.IsDisposed || root == null)
            {
                return;
            }

            UIAudioConfig rootConfig = root.GetComponent<UIAudioConfig>();
            if (rootConfig != null)
            {
                PlayConfiguredSound(scene, rootConfig.OpenSound, rootConfig.GroupName, rootConfig.Priority);
            }

            bool includeInactive = rootConfig == null || rootConfig.IncludeInactiveChildren;
            UIAudioConfig[] configs = root.GetComponentsInChildren<UIAudioConfig>(includeInactive);
            foreach (UIAudioConfig config in configs)
            {
                BindClick(scene, config);
            }
        }

        public static void PlayUICloseSound(this Scene scene, GameObject root)
        {
            if (scene == null || scene.IsDisposed || root == null)
            {
                return;
            }

            UIAudioConfig config = root.GetComponent<UIAudioConfig>();
            if (config == null)
            {
                return;
            }

            PlayConfiguredSound(scene, config.CloseSound, config.GroupName, config.Priority);
        }

        private static void BindClick(Scene scene, UIAudioConfig config)
        {
            if (config == null || !config.BindClick || string.IsNullOrWhiteSpace(config.ClickSound))
            {
                return;
            }

            Button button = config.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            if (config.BoundClickHandler != null)
            {
                button.onClick.RemoveListener(config.BoundClickHandler);
            }

            EntityRef<Scene> sceneRef = scene;
            string assetName = config.ClickSound;
            string groupName = config.GroupName;
            int priority = config.Priority;
            UnityAction handler = () =>
            {
                Scene currentScene = sceneRef;
                if (currentScene == null || currentScene.IsDisposed)
                {
                    return;
                }

                PlayConfiguredSound(currentScene, assetName, groupName, priority);
            };
            config.BoundClickHandler = handler;
            button.onClick.AddListener(handler);
        }

        private static void PlayConfiguredSound(Scene scene, string assetName, string groupName, int priority)
        {
            if (scene == null || scene.IsDisposed || string.IsNullOrWhiteSpace(assetName))
            {
                return;
            }

            AudioPlayParams playParams = AudioPlayParams.Create();
            playParams.Priority = priority;
            AudioHelper.Play(scene, assetName, groupName, playParams).Coroutine();
        }
    }
}
