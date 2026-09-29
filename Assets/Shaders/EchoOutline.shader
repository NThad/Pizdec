Shader "Pizdec/EchoOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 0.025
        _EchoPoint ("Echo Point", Vector) = (0,0,0,0)
        _EchoRadius ("Echo Radius", Float) = 0
        _EchoBand ("Echo Reveal Band", Float) = 1.25
        _EchoFade ("Echo Fade", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent+10"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "EchoOutline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float4 _EchoPoint;
                float _EchoRadius;
                float _EchoBand;
                float _EchoFade;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS);
                VertexNormalInputs normal = GetVertexNormalInputs(input.normalOS);

                float3 positionWS = pos.positionWS + normal.normalWS * _OutlineWidth;
                output.positionWS = positionWS;
                output.normalWS = normal.normalWS;
                output.positionHCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float distanceFromEcho = distance(input.positionWS, _EchoPoint.xyz);
                float band = max(_EchoBand, 0.01);

                // Reveal the already-reached part of the object.
                float reveal = 1.0 - smoothstep(_EchoRadius - band, _EchoRadius, distanceFromEcho);

                // Keep only the silhouette/edge of the expanded hull.
                float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
                float edge = 1.0 - abs(dot(normalize(input.normalWS), viewDir));
                edge = smoothstep(0.72, 0.94, edge);

                float alpha = reveal * edge * _EchoFade * _OutlineColor.a;
                if (alpha <= 0.001)
                    discard;

                return half4(_OutlineColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}