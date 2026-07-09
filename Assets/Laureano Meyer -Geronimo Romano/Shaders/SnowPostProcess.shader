Shader "Hidden/SnowPostProcess"
{
    Properties
    {
        _MainTex ("Screen", 2D) = "white" {}
        _SnowColor ("Snow Color", Color) = (1,1,1,1)
        _SnowThreshold ("Snow Threshold (dot up)", Range(0,1)) = 0.6
        _SnowSmoothness ("Snow Edge Smoothness", Range(0.001,1)) = 0.2
        _SnowOpacity ("Snow Opacity", Range(0,1)) = 1
        _NoiseScale ("Noise Scale", Range(0.01, 5)) = 0.5
        _NoiseStrength ("Noise Threshold Variation", Range(0,1)) = 0.3
        _NoiseColorVariation ("Noise Color Variation", Range(0,1)) = 0.25
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthNormalsTexture;

            fixed4 _SnowColor;
            float _SnowThreshold;
            float _SnowSmoothness;
            float _SnowOpacity;
            float _NoiseScale;
            float _NoiseStrength;
            float _NoiseColorVariation;

            float4x4 _CamToWorld;
            float4x4 _FrustumCorners; // filas: 0=BL,1=TL,2=TR,3=BR

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                int index = 0;
                if (v.uv.x < 0.5 && v.uv.y < 0.5) index = 0;      // bottom-left
                else if (v.uv.x < 0.5 && v.uv.y >= 0.5) index = 1; // top-left
                else if (v.uv.x >= 0.5 && v.uv.y >= 0.5) index = 2; // top-right
                else index = 3;                                    // bottom-right

                o.ray = _FrustumCorners[index].xyz;
                return o;
            }

            // --- Ruido 3D (value noise + fbm) ---
            float hash(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float noise3D(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);

                float n000 = hash(i + float3(0,0,0));
                float n100 = hash(i + float3(1,0,0));
                float n010 = hash(i + float3(0,1,0));
                float n110 = hash(i + float3(1,1,0));
                float n001 = hash(i + float3(0,0,1));
                float n101 = hash(i + float3(1,0,1));
                float n011 = hash(i + float3(0,1,1));
                float n111 = hash(i + float3(1,1,1));

                float nx00 = lerp(n000, n100, f.x);
                float nx10 = lerp(n010, n110, f.x);
                float nx01 = lerp(n001, n101, f.x);
                float nx11 = lerp(n011, n111, f.x);

                float nxy0 = lerp(nx00, nx10, f.y);
                float nxy1 = lerp(nx01, nx11, f.y);

                return lerp(nxy0, nxy1, f.z);
            }

            float fbm(float3 p)
            {
                float f = 0;
                f += 0.5   * noise3D(p);
                f += 0.25  * noise3D(p * 2.0);
                f += 0.125 * noise3D(p * 4.0);
                return f; // ~0..0.875
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 sceneColor = tex2D(_MainTex, i.uv);

                float depth01;
                float3 viewNormal;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv), depth01, viewNormal);

                float3 worldNormal = normalize(mul((float3x3)_CamToWorld, viewNormal));
                float upDot = dot(worldNormal, float3(0,1,0));

                // Reconstruccion de world position via depth + rayo de frustum
                float eyeDepth = depth01 * _ProjectionParams.z; // far plane
                float3 worldPos = _WorldSpaceCameraPos + eyeDepth * i.ray;

                float n = fbm(worldPos * _NoiseScale);

                // El ruido modula el umbral -> bordes irregulares en vez de una linea perfecta
                float threshold = _SnowThreshold + (n - 0.5) * _NoiseStrength;
                float snowMask = smoothstep(threshold - _SnowSmoothness, threshold + _SnowSmoothness, upDot);
                snowMask *= _SnowOpacity;

                // El ruido tambien varia un poco el brillo del color -> textura visible
                fixed3 snowColorFinal = _SnowColor.rgb * lerp(1.0 - _NoiseColorVariation, 1.0, n);

                fixed4 finalColor = lerp(sceneColor, fixed4(snowColorFinal,1), snowMask);
                return finalColor;
            }
            ENDCG
        }
    }
}