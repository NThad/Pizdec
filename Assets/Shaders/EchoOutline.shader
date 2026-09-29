Shader "Pizdec/EchoOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 0.025
        _EchoPoint ("Echo Point", Vector) = (0,0,0,0)
        _EchoRadius ("Echo Radius", Float) = 0
        _EchoBand ("Echo Band", Float) = 0.8
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
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
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

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normal = GetVertexNormalInputs(input.normalOS);

                float3 positionWS = pos.positionWS;
                positionWS += normal.normalWS * _OutlineWidth;

                output.positionWS = positionWS;
                output.normalWS = normal.normalWS;
                output.positionHCS = TransformWorldToHClip(positionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float distanceFromEcho = distance(input.positionWS, _EchoPoint.xyz);

                float band = max(_EchoBand, 0.01);
                float front = 1.0 - smoothstep(_EchoRadius, _EchoRadius + band, distanceFromEcho);
                float behind = smoothstep(_EchoRadius - band, _EchoRadius, distanceFromEcho);

                float waveMask = front * behind;
                float facing = 1.0 - saturate(dot(normalize(input.normalWS), normalize(GetWorldSpaceNormalizeViewDir(input.positionWS))));

                float alpha = waveMask * (0.35 + facing * 0.65) * _EchoFade * _OutlineColor.a;

                if (alpha <= 0.001)
                    discard;

                return half4(_OutlineColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
