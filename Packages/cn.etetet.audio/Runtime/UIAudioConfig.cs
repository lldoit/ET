using UnityEngine;
using UnityEngine.Events;

namespace ET.Client
{
    [DisallowMultipleComponent]
    [AddComponentMenu("ET/Audio/UI Audio Config")]
    public sealed class UIAudioConfig : MonoBehaviour
    {
        [SerializeField]
        private string clickSound;

        [SerializeField]
        private string openSound;

        [SerializeField]
        private string closeSound;

        [SerializeField]
        private string groupName = "Sound";

        [SerializeField]
        private int priority;

        [SerializeField]
        private bool bindClick = true;

        [SerializeField]
        private bool includeInactiveChildren = true;

        public string ClickSound => this.clickSound;
        public string OpenSound => this.openSound;
        public string CloseSound => this.closeSound;
        public string GroupName => string.IsNullOrWhiteSpace(this.groupName) ? "Sound" : this.groupName;
        public int Priority => this.priority;
        public bool BindClick => this.bindClick;
        public bool IncludeInactiveChildren => this.includeInactiveChildren;
        public UnityAction BoundClickHandler { get; set; }
    }
}
