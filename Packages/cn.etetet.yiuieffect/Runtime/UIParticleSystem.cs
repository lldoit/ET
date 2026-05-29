using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework
{
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasRenderer), typeof(ParticleSystem))]
    [AddComponentMenu("UI/Effects/Extensions/UIParticleSystem")]
    public class UIParticleSystem : MaskableGraphic
    {
        private const int MaxParticles = 14000;
        private const string DefaultShaderName = "UI Extensions/Particles/Additive";
        private static readonly Vector4 FullImageUV = new(0f, 0f, 1f, 1f);

        [Tooltip("Having this enabled run the system in LateUpdate rather than in Update making it faster but less precise (more clunky)")]
        public bool fixedTime = true;

        [Tooltip("Enables 3d rotation for the particles")]
        public bool use3dRotation = false;

        private readonly UIVertex[] quad = new UIVertex[4];
        private Transform cachedTransform;
        private ParticleSystem particleSystemInstance;
        private ParticleSystem.Particle[] particles;
        private ParticleSystem.MainModule mainModule;
        private ParticleSystem.TextureSheetAnimationModule textureSheetAnimation;
        private int textureSheetAnimationFrames;
        private Vector2 textureSheetAnimationFrameSize;
        private bool isInitialised;
        private Material currentMaterial;
        private Texture currentTexture;

        public override Texture mainTexture => currentTexture != null ? currentTexture : Texture2D.whiteTexture;

        protected override void Awake()
        {
            base.Awake();
            enabled = Initialize();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!gameObject.activeInHierarchy || !Initialize()) return;

            StopInitialEmissionIfNeeded();
            int count = particleSystemInstance.GetParticles(particles);
            for (int i = 0; i < count; ++i)
            {
                PopulateParticleQuad(particles[i]);
                vh.AddUIVertexQuad(quad);
            }
        }

        private void Update()
        {
            if (!fixedTime && Application.isPlaying) SimulateAndRefresh();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) SetAllDirty();
            else if (fixedTime) SimulateAndRefresh();
            RefreshWhenMaterialChanged();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            currentMaterial = null;
            currentTexture = null;
        }

        public void StartParticleEmission()
        {
            if (Initialize()) particleSystemInstance.Play();
        }

        public void StopParticleEmission()
        {
            if (Initialize()) particleSystemInstance.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void PauseParticleEmission()
        {
            if (Initialize()) particleSystemInstance.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        private bool Initialize()
        {
            cachedTransform ??= transform;
            if (particleSystemInstance == null)
            {
                particleSystemInstance = GetComponent<ParticleSystem>();
                if (particleSystemInstance == null) return false;
                ConfigureParticleSystem();
            }

            EnsureParticleBuffer();
            RefreshTextureSheetAnimation();
            return true;
        }

        private void ConfigureParticleSystem()
        {
            mainModule = particleSystemInstance.main;
            if (mainModule.maxParticles > MaxParticles) mainModule.maxParticles = MaxParticles;

            ParticleSystemRenderer particleRenderer = particleSystemInstance.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null) particleRenderer.enabled = false;

            EnsureMaterial();
            mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
            particles = null;
        }

        private void EnsureMaterial()
        {
            if (material == null)
            {
                Shader foundShader = Shader.Find(DefaultShaderName);
                if (foundShader != null) material = new Material(foundShader);
            }

            currentMaterial = material;
            currentTexture = ResolveMainTexture(currentMaterial);
            material = currentMaterial;
        }

        private static Texture ResolveMainTexture(Material sourceMaterial)
        {
            if (sourceMaterial != null && sourceMaterial.HasProperty("_MainTex"))
            {
                return sourceMaterial.mainTexture != null ? sourceMaterial.mainTexture : Texture2D.whiteTexture;
            }

            return Texture2D.whiteTexture;
        }

        private void EnsureParticleBuffer()
        {
            int maxParticles = particleSystemInstance.main.maxParticles;
            if (particles == null || particles.Length < maxParticles)
            {
                particles = new ParticleSystem.Particle[maxParticles];
            }
        }

        private void RefreshTextureSheetAnimation()
        {
            textureSheetAnimation = particleSystemInstance.textureSheetAnimation;
            textureSheetAnimationFrames = 0;
            textureSheetAnimationFrameSize = Vector2.zero;
            if (!textureSheetAnimation.enabled || textureSheetAnimation.numTilesX <= 0 || textureSheetAnimation.numTilesY <= 0) return;

            textureSheetAnimationFrames = textureSheetAnimation.numTilesX * textureSheetAnimation.numTilesY;
            textureSheetAnimationFrameSize = new Vector2(1f / textureSheetAnimation.numTilesX, 1f / textureSheetAnimation.numTilesY);
        }

        private void StopInitialEmissionIfNeeded()
        {
            if (isInitialised || mainModule.playOnAwake) return;
            particleSystemInstance.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            isInitialised = true;
        }

        private void PopulateParticleQuad(ParticleSystem.Particle particle)
        {
            Vector2 position = GetParticlePosition(particle);
            float rotation = -particle.rotation * Mathf.Deg2Rad;
            float size = particle.GetCurrentSize(particleSystemInstance) * 0.5f;
            ApplyShapeScale(ref position);
            SetQuadUVAndColor(GetParticleUV(particle), particle.GetCurrentColor(particleSystemInstance));

            if (Mathf.Approximately(rotation, 0f)) SetQuadWithoutRotation(position, size);
            else if (use3dRotation) SetQuadWith3DRotation(particle, size);
            else SetQuadWith2DRotation(position, rotation, size);
        }

        private Vector2 GetParticlePosition(ParticleSystem.Particle particle)
        {
            return mainModule.simulationSpace == ParticleSystemSimulationSpace.Local
                ? particle.position
                : cachedTransform.InverseTransformPoint(particle.position);
        }

        private void ApplyShapeScale(ref Vector2 position)
        {
            if (mainModule.scalingMode == ParticleSystemScalingMode.Shape && canvas != null)
            {
                position /= canvas.scaleFactor;
            }
        }

        private Vector4 GetParticleUV(ParticleSystem.Particle particle)
        {
            if (!textureSheetAnimation.enabled || textureSheetAnimationFrames <= 0) return FullImageUV;

            float lifetimeProgress = 1f - particle.remainingLifetime / particle.startLifetime;
            float frameProgress = textureSheetAnimation.frameOverTime.Evaluate(lifetimeProgress, 0f);
            int frame = GetTextureSheetFrame(Mathf.Repeat(frameProgress * textureSheetAnimation.cycleCount, 1f));
            float x = frame % textureSheetAnimation.numTilesX * textureSheetAnimationFrameSize.x;
            float y = 1f - Mathf.FloorToInt(frame / textureSheetAnimation.numTilesX) * textureSheetAnimationFrameSize.y;
            return new Vector4(x, y, x + textureSheetAnimationFrameSize.x, y + textureSheetAnimationFrameSize.y);
        }

        private int GetTextureSheetFrame(float frameProgress)
        {
            int frame = textureSheetAnimation.animation switch
            {
                ParticleSystemAnimationType.SingleRow => Mathf.FloorToInt(frameProgress * textureSheetAnimation.numTilesX)
                                                         + textureSheetAnimation.rowIndex * textureSheetAnimation.numTilesX,
                _ => Mathf.FloorToInt(frameProgress * textureSheetAnimationFrames)
            };
            return frame % textureSheetAnimationFrames;
        }

        private void SetQuadUVAndColor(Vector4 particleUV, Color32 color)
        {
            SetVertex(0, color, particleUV.x, particleUV.y);
            SetVertex(1, color, particleUV.x, particleUV.w);
            SetVertex(2, color, particleUV.z, particleUV.w);
            SetVertex(3, color, particleUV.z, particleUV.y);
        }

        private void SetVertex(int index, Color32 color, float u, float v)
        {
            quad[index] = UIVertex.simpleVert;
            quad[index].color = color;
            quad[index].uv0 = new Vector2(u, v);
        }

        private void SetQuadWithoutRotation(Vector2 position, float size)
        {
            quad[0].position = new Vector2(position.x - size, position.y - size);
            quad[1].position = new Vector2(position.x - size, position.y + size);
            quad[2].position = new Vector2(position.x + size, position.y + size);
            quad[3].position = new Vector2(position.x + size, position.y - size);
        }

        private void SetQuadWith2DRotation(Vector2 position, float rotation, float size)
        {
            float rotation90 = rotation + Mathf.PI / 2f;
            Vector2 right = new(Mathf.Cos(rotation), Mathf.Sin(rotation));
            Vector2 up = new(Mathf.Cos(rotation90), Mathf.Sin(rotation90));
            right *= size;
            up *= size;
            quad[0].position = position - right - up;
            quad[1].position = position - right + up;
            quad[2].position = position + right + up;
            quad[3].position = position + right - up;
        }

        private void SetQuadWith3DRotation(ParticleSystem.Particle particle, float size)
        {
            Vector3 position = mainModule.simulationSpace == ParticleSystemSimulationSpace.Local
                ? particle.position
                : cachedTransform.InverseTransformPoint(particle.position);
            Quaternion particleRotation = Quaternion.Euler(particle.rotation3D);
            quad[0].position = position + particleRotation * new Vector3(-size, -size, 0f);
            quad[1].position = position + particleRotation * new Vector3(-size, size, 0f);
            quad[2].position = position + particleRotation * new Vector3(size, size, 0f);
            quad[3].position = position + particleRotation * new Vector3(size, -size, 0f);
        }

        private void SimulateAndRefresh()
        {
            if (!Initialize()) return;

            particleSystemInstance.Simulate(Time.unscaledDeltaTime, false, false, true);
            SetAllDirty();
            RefreshWhenMaterialChanged();
        }

        private void RefreshWhenMaterialChanged()
        {
            Texture resolvedTexture = ResolveMainTexture(currentMaterial);
            if (material == currentMaterial && currentTexture == resolvedTexture) return;

            particleSystemInstance = null;
            Initialize();
        }
    }
}
