Shader "UI/Crawlers/CardHoloFoil"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FoilTexture ("Foil Texture", 2D) = "gray" {}
        _GlitterTexture ("Glitter Texture", 2D) = "gray" {}
        _HoloIntensity ("Holo Intensity", Range(0, 2)) = 1
        _LightIntensity ("Light Intensity", Range(0, 4)) = 1.4
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.08
        _ViewOffset ("View Offset", Vector) = (0,0,0,0)
        _ViewStrength ("View Strength", Range(0, 2)) = 0.9
        _EdgeStrength ("Edge Strength", Range(0, 2)) = 0.5
        _MinAlpha ("Minimum Alpha", Range(0, 1)) = 0.04
        _EffectOpacity ("Effect Opacity", Range(0, 1)) = 0
        _FlowSpeed ("Flow Speed", Range(-4, 4)) = 0.14

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _FoilTexture;
            sampler2D _GlitterTexture;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            half _HoloIntensity;
            half _LightIntensity;
            half _DistortionStrength;
            float4 _ViewOffset;
            half _ViewStrength;
            half _EdgeStrength;
            half _MinAlpha;
            half _EffectOpacity;
            half _FlowSpeed;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            half HoloBars(half t)
            {
                half stepIndex = floor(frac(t) * 10.0h);
                if (stepIndex < 1.0h) return 0.10h;
                if (stepIndex < 2.0h) return 0.20h;
                if (stepIndex < 3.0h) return 0.35h;
                if (stepIndex < 4.0h) return 0.425h;
                if (stepIndex < 5.0h) return 0.50h;
                if (stepIndex < 6.0h) return 0.425h;
                if (stepIndex < 7.0h) return 0.35h;
                if (stepIndex < 8.0h) return 0.20h;
                if (stepIndex < 9.0h) return 0.10h;
                return 0.0h;
            }

            fixed3 RadiantPalette(half t)
            {
                t = frac(t);
                fixed3 c0 = fixed3(1.0h, 0.36h, 0.76h);
                fixed3 c1 = fixed3(0.34h, 0.7h, 1.0h);
                fixed3 c2 = fixed3(1.0h, 0.74h, 0.32h);
                fixed3 c3 = fixed3(0.34h, 1.0h, 0.78h);
                fixed3 c4 = fixed3(1.0h, 0.34h, 0.96h);
                fixed3 c5 = fixed3(0.35h, 0.94h, 1.0h);

                fixed3 a = lerp(c0, c1, smoothstep(0.0h, 0.18h, t));
                fixed3 b = lerp(a, c2, smoothstep(0.18h, 0.34h, t));
                fixed3 c = lerp(b, c3, smoothstep(0.34h, 0.5h, t));
                fixed3 d = lerp(c, c4, smoothstep(0.5h, 0.68h, t));
                fixed3 e = lerp(d, c5, smoothstep(0.68h, 0.84h, t));
                return lerp(e, c0, smoothstep(0.84h, 1.0h, t));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 baseColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                float2 uv = IN.texcoord;
                float2 centered = uv - 0.5;
                float2 view = _ViewOffset.xy * _ViewStrength;
                float2 pointer01 = saturate(0.5 + view);
                float2 background01 = saturate(0.5 + view * 0.26);
                float2 shinePos = 0.5 + (background01 - 0.5) * 1.5;
                float2 foilPos = 0.5 - (background01 - 0.5) * 2.5;
                float2 radialCenter = saturate(pointer01 * 0.5 + 0.25);
                half viewAxis = dot(view, float2(0.75, -0.45));
                half viewActivity = saturate(0.35h + length(view) * 2.4h);

                float2 foilUv = uv * 4.0h + (foilPos - 0.5) * 2.0h + float2(0, _Time.y * _FlowSpeed * 0.025h);
                fixed3 foil = tex2D(_FoilTexture, foilUv).rgb;
                half foilLuma = dot(foil, fixed3(0.299h, 0.587h, 0.114h));
                half foilLine = saturate((1.0h - foilLuma) * 1.25h);

                float2 flowUv = uv + (shinePos - 0.5) * 0.42h + (foilLine - 0.5h) * _DistortionStrength * 0.08h;
                half glare = pow(1.0h - smoothstep(0.03h, 0.62h, distance(uv, radialCenter)), 2.0h);
                half edge = max(smoothstep(0.42, 0.5, abs(centered.x)), smoothstep(0.42, 0.5, abs(centered.y)));

                half barsA = HoloBars((flowUv.x + flowUv.y) * 0.72h + viewAxis * 0.65h + _Time.y * _FlowSpeed * 0.035h);
                half barsB = HoloBars((flowUv.x - flowUv.y) * 0.72h - viewAxis * 0.58h - _Time.y * _FlowSpeed * 0.03h);
                half crossBars = saturate(barsA * 0.54h + barsB * 0.46h);
                half radiantBars = saturate(crossBars * 0.82h + foilLine * 0.18h);

                float2 colorUv = uv + (foilPos - 0.5) * 1.18h;
                half paletteU = colorUv.x * 0.50h + colorUv.y * 0.34h - _Time.y * _FlowSpeed * 0.025h;
                half bandCoord = frac(paletteU * 0.72h);
                half broadA = 1.0h - smoothstep(0.035h, 0.18h, abs(frac((colorUv.x + colorUv.y) * 0.82h - viewAxis * 0.42h) - 0.5h));
                half broadB = 1.0h - smoothstep(0.035h, 0.17h, abs(frac((colorUv.x - colorUv.y) * 0.76h + viewAxis * 0.36h) - 0.5h));
                half wideBands = saturate(max(broadA, broadB * 0.92h));

                half glitter = tex2D(_GlitterTexture, uv * 6.67h + view * 0.35h + float2(_Time.y * _FlowSpeed * 0.02h, 0)).r;
                half sparkle = smoothstep(0.80h, 0.98h, glitter);
                half bandEnergy = pow(wideBands, 1.35h);
                half shine = saturate(glare * 0.18h + bandEnergy * 0.72h + sparkle * 0.24h + edge * _EdgeStrength * 0.025h);

                fixed3 radiantColor = RadiantPalette(bandCoord);
                fixed3 counterColor = RadiantPalette(bandCoord + 0.42h);
                fixed3 foilColor = lerp(radiantColor, abs(radiantColor - foil), saturate(foilLine * 0.55h));
                fixed3 etchColor = lerp(foilColor, counterColor, saturate(crossBars * 0.22h));
                fixed3 bandColor = lerp(etchColor, fixed3(1.0h, 0.38h, 0.96h), broadA * 0.42h);
                bandColor = lerp(bandColor, fixed3(0.36h, 0.92h, 1.0h), broadB * 0.38h);
                fixed3 glitterColor = fixed3(1.0h, 0.55h, 0.92h) * sparkle * 0.20h;
                fixed3 glareColor = lerp(fixed3(0.60h, 1.0h, 0.95h), RadiantPalette(bandCoord + 0.18h), 0.45h) * glare * 0.16h;

                fixed4 color;
                color.rgb = saturate(
                    bandColor * (bandEnergy * 0.95h + radiantBars * 0.05h) +
                    glareColor +
                    glitterColor) * IN.color.rgb;
                color.a = saturate((_MinAlpha + bandEnergy * 0.12h + shine * _LightIntensity * 0.035h + sparkle * 0.018h) * baseColor.a * _HoloIntensity);
                color.a *= viewActivity;
                color.a = min(color.a, 0.55h) * _EffectOpacity;

                #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
