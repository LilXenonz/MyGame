Shader "URP/ObjectSonar_Polished"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _RingColor ("Ring Color", Color) = (0,1,1,1)
        _RingWidth ("Ring Width", Float) = 0.25
        _RingSpeed ("Ring Speed", Float) = 4.0
        _RingIntensity ("Ring Intensity", Float) = 1.6
        _MaxRadius ("Max Radius (loop distance)", Float) = 8.0

        _Selected ("Selected (0/1)", Range(0,1)) = 0
        _SelectedColor ("Selected Color", Color) = (1,0.9,0.3,1)
        _SelectedPulseSpeed ("Selected Pulse Speed", Float) = 3.0
        _SelectedIntensity ("Selected Intensity", Float) = 0.6

        _EdgeSoftness ("Edge Softness", Range(0,1)) = 0.7
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // small fixed pulse count for seamless looping
            #define PULSE_COUNT 3

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseColor;
            float4 _RingColor;
            float _RingWidth;
            float _RingSpeed;
            float _RingIntensity;
            float _MaxRadius;

            float _Selected;
            float4 _SelectedColor;
            float _SelectedPulseSpeed;
            float _SelectedIntensity;
            float _EdgeSoftness;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            // narrow smooth ring band: returns 0..1 where 1 == center of band
            inline float ringBand(float dist, float radius, float width)
            {
                float a = smoothstep(radius - width, radius, dist);
                float b = smoothstep(radius, radius + width, dist);
                return saturate(a - b);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // object pivot in world space
                float3 origin = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;

                // sample base texture & apply base color
                float3 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;

                // safety clamps
                float maxR = max(0.0001, _MaxRadius);
                float w = max(0.00001, _RingWidth);

                // compute distance (one time)
                float dist = distance(IN.worldPos, origin);

                // base animation phase normalized to range 0..1
                float basePhase = frac( (_Time.y * _RingSpeed) / maxR );

                // combine a few staggered pulses so when one resets other pulses cover the gap
                float combined = 0.0;
                // unrolled pulses for best perf and no dynamic loops
                {
                    float phase0 = frac(basePhase + 0.0 / PULSE_COUNT);
                    float r0 = phase0 * maxR;
                    combined = max(combined, ringBand(dist, r0, w));
                }
                {
                    float phase1 = frac(basePhase + 1.0 / PULSE_COUNT);
                    float r1 = phase1 * maxR;
                    combined = max(combined, ringBand(dist, r1, w));
                }
                {
                    float phase2 = frac(basePhase + 2.0 / PULSE_COUNT);
                    float r2 = phase2 * maxR;
                    combined = max(combined, ringBand(dist, r2, w));
                }

                float ring = saturate(combined * _RingIntensity);

                // blend base -> ring color
                float3 ringTarget = _RingColor.rgb;
                float3 colorAfterRing = lerp(baseTex, ringTarget * _RingIntensity, ring);

                // soft vignette near edges to look polished
                float vign = 1.0 - smoothstep(maxR * 0.92, maxR, dist);
                vign = lerp(1.0, vign, saturate(_EdgeSoftness));
                colorAfterRing *= vign;

                // selected glow (soft additive pulse)
                float sel = saturate(_Selected);
                if (sel > 0.001)
                {
                    float pulse = (sin(_Time.y * _SelectedPulseSpeed) * 0.5 + 0.5); // 0..1
                    float selGlow = sel * pow(pulse, 0.7) * _SelectedIntensity;
                    colorAfterRing = saturate(colorAfterRing + _SelectedColor.rgb * selGlow * 0.6);
                }

                return float4(colorAfterRing, 1.0);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
