using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS 关卡配置
    /// 用于可视化配置准星移动范围和相机安全区域
    /// </summary>
    public class TpsLevelConfig : MonoBehaviour
    {
        [Header("Aim Restrictions (Screen Pixels)")]
        [Tooltip("准星在屏幕上的最大偏移量 (X/Y 像素)")]
        public Vector2 MaxAimScreenOffset = new Vector2(600f, 350f);

        [Header("Camera Visualization Settings")]
        [Tooltip("像素到世界单位的转换比例 (需与 TpsCameraComponent 保持一致)")]
        public float PixelToWorldRatio = 0.05f;

        [Tooltip("相机跟随比例 (需与 TpsCameraComponent 保持一致)")]
        public float CameraFollowRatio = 0.1f;

        [Tooltip("安全区域显示颜色")]
        public Color SafeZoneColor = new Color(0, 1, 0, 0.3f);

        [Header("Camera World Bounds")]
        [Tooltip("相机在世界坐标下的最大偏移限制 (X/Y 世界单位)\n用于防止相机超出背景图范围")]
        public Vector2 MaxCameraWorldOffset = new Vector2(3f, 5f);

        private void OnDrawGizmos()
        {
            Vector3 center = transform.position;

            // 1. 绘制准星对应的相机理论活动范围 (黄色线框)
            // CameraOffset = ScreenOffset * PixelToWorld * FollowRatio
            float theoryCamX = MaxAimScreenOffset.x * PixelToWorldRatio * CameraFollowRatio;
            float theoryCamY = MaxAimScreenOffset.y * PixelToWorldRatio * CameraFollowRatio;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, new Vector3(theoryCamX * 2, theoryCamY * 2, 1f));

            // 2. 绘制相机实际限制范围 (红色线框 - 背景图安全区)
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, new Vector3(MaxCameraWorldOffset.x * 2, MaxCameraWorldOffset.y * 2, 1.1f));

            // 3. 绘制重叠的安全区域 (绿色填充)
            // 实际生效的是两者的交集
            float safeX = Mathf.Min(theoryCamX, MaxCameraWorldOffset.x);
            float safeY = Mathf.Min(theoryCamY, MaxCameraWorldOffset.y);

            Gizmos.color = SafeZoneColor;
            Gizmos.DrawCube(center, new Vector3(safeX * 2, safeY * 2, 1f));
        }
    }
}
