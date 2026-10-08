Shader "MisionAedes/RiverWaterSubtle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WaterIntensity ("Water Intensity", Range(0, 1)) = 0.72
        _WaterSpeed ("Water Speed", Range(0, 2)) = 0.68
        [PerRendererData] _WaterTime ("Water Time", Float) = 0
        [PerRendererData] _WaterPhase ("Water Phase", Float) = 0
        [PerRendererData] _FallCount ("Waterfall Count", Float) = 0
        [PerRendererData] _FallRect1 ("Waterfall 1", Vector) = (0,0,0,0)
        [PerRendererData] _FallRect2 ("Waterfall 2", Vector) = (0,0,0,0)
        [PerRendererData] _FallRect3 ("Waterfall 3", Vector) = (0,0,0,0)
        [PerRendererData] _BaseRect1 ("Splash Base 1", Vector) = (0,0,0,0)
        [PerRendererData] _BaseRect2 ("Splash Base 2", Vector) = (0,0,0,0)
        [PerRendererData] _BaseRect3 ("Splash Base 3", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="Unlit"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SpriteUnlit"
            Tags { "LightMode"="Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _WaterIntensity;
                half _WaterSpeed;
                float _WaterTime;
                float _WaterPhase;
                half _FallCount;
                float4 _FallRect1;
                float4 _FallRect2;
                float4 _FallRect3;
                float4 _BaseRect1;
                float4 _BaseRect2;
                float4 _BaseRect3;
            CBUFFER_END

            half RectMask(float2 uv, float4 rect)
            {
                return step(rect.x, uv.x) * step(rect.y, uv.y) *
                    step(uv.x, rect.z) * step(uv.y, rect.w);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 source = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // Máscara conservadora: solo tonos azules/cian saturados. Las rocas,
                // orillas y plantas quedan matemáticamente sin cambios.
                half blueOverRed = saturate((source.b - source.r - 0.035h) * 8.0h);
                half coolColor = saturate((source.b - source.g + 0.16h) * 5.0h);
                half saturation = saturate((max(source.g, source.b) - source.r - 0.025h) * 7.0h);
                half rawWaterMask = blueOverRed * coolColor * saturation;
                // Endurece el borde de la máscara para que tonos ambiguos de roca/musgo
                // no reciban el efecto, aunque el movimiento del agua sea más visible.
                half waterMask = smoothstep(0.22h, 0.62h, rawWaterMask) * source.a;

                float travel = _WaterTime * _WaterSpeed + _WaterPhase;
                // Coordenadas levemente cuantizadas para conservar el carácter pixel art.
                float2 pixelUv = floor(input.uv * 220.0) / 220.0;

                // Corriente oblicua para los cauces y bandas horizontales que descienden
                // por las cascadas (uv.y + tiempo desplaza la banda hacia abajo).
                half currentA = sin(pixelUv.y * 31.0 + pixelUv.x * 12.0 - travel * 3.8);
                half currentB = sin(pixelUv.x * 43.0 - pixelUv.y * 9.0 + travel * 2.7);
                half falling = sin(pixelUv.y * 58.0 + travel * 6.4) * 0.5h + 0.5h;
                half fallingStreak = pow(saturate(falling), 7.0h);

                half rollingLight = (currentA * 0.68h + currentB * 0.32h) * 0.5h;
                half3 variation = half3(0.045h, 0.085h, 0.115h) * rollingLight;
                variation += half3(0.035h, 0.075h, 0.105h) * fallingStreak;
                source.rgb = saturate(source.rgb + variation * waterMask * _WaterIntensity);

                half enabled1 = step(0.5h, _FallCount);
                half enabled2 = step(1.5h, _FallCount);
                half enabled3 = step(2.5h, _FallCount);
                half fallRegion = saturate(RectMask(input.uv, _FallRect1) * enabled1 +
                    RectMask(input.uv, _FallRect2) * enabled2 + RectMask(input.uv, _FallRect3) * enabled3);
                half baseRegion = saturate(RectMask(input.uv, _BaseRect1) * enabled1 +
                    RectMask(input.uv, _BaseRect2) * enabled2 + RectMask(input.uv, _BaseRect3) * enabled3);

                // Hilos verticales segmentados que descienden. Continúan usando la
                // máscara acuática, de modo que nunca se dibujan sobre las rocas.
                half lane = pow(saturate(sin(pixelUv.x * 118.0h + _WaterPhase * 1.7h) * 0.5h + 0.5h), 9.0h);
                half descendingDash = pow(saturate(sin(pixelUv.y * 74.0h + travel * 11.0h) * 0.5h + 0.5h), 5.0h);
                half fallDetail = fallRegion * waterMask * (lane * 0.72h + descendingDash * 0.28h);
                source.rgb = saturate(source.rgb + half3(0.10h, 0.18h, 0.22h) * fallDetail * 0.72h);

                // Pulso de espuma y pequeñas crestas en la base, limitado al agua
                // azul/celeste de las zonas marcadas para cada cascada.
                half splashWave = pow(saturate(sin((pixelUv.x * 83.0h - pixelUv.y * 29.0h) - travel * 8.0h) * 0.5h + 0.5h), 7.0h);
                half foamPulse = sin(pixelUv.x * 47.0h + travel * 6.0h) * 0.5h + 0.5h;
                half splashDetail = baseRegion * waterMask * saturate(splashWave * 0.70h + foamPulse * 0.30h);
                source.rgb = saturate(source.rgb + half3(0.075h, 0.14h, 0.18h) * splashDetail * 0.64h);
                return source;
            }
            ENDHLSL
        }
    }
}
