using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public enum EditorKofHitBoxType
    {
        High,
        Low
    }

    public enum EditorKofHitBoxShape
    {
        Circle,
        Rectangle
    }

    [System.Serializable]
    public class KofHitBoxConfig
    {
        public EditorKofHitBoxType BoxType;
        public EditorKofHitBoxShape Shape;
        public float Radius;
        public Vector2 Offset;
        public string BoneName;
    }

    public class KofHitBoxesView : MonoBehaviour
    {
        public List<KofHitBoxConfig> BoxConfigs = new();
        public List<KofHitBoxConfig> RealTimeBoxes = new();
        
        private void OnDrawGizmos()
        {
            foreach (var box in BoxConfigs)
            {
                Gizmos.color = box.BoxType == EditorKofHitBoxType.High ? Color.red : Color.green;
                var center = transform.position + new Vector3(box.Offset.x, box.Offset.y, 0);
                if (box.Shape == EditorKofHitBoxShape.Circle)
                {
                    Gizmos.DrawWireSphere(center, box.Radius);
                }
            }

            foreach (var box in RealTimeBoxes)
            {
                Gizmos.color = Color.green;
                var center = transform.position + new Vector3(box.Offset.x, box.Offset.y, 0);
                if (box.Shape == EditorKofHitBoxShape.Circle)
                {
                    Gizmos.DrawWireSphere(center, box.Radius);
                }
            }
        }
    }
}
