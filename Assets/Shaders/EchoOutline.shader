Shader "Pizdec/EchoOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 0.025
        _EchoPoint ("Echo Point", Vector) = (0,0,0,0)
        _EchoRadius ("Echo Radius", Float) = 0
        _EchoBand ("Echo Reveal Band", Float) = 1.25
        _EchoFadeRadius ("Echo Fade Radius", Float) = 0
        _EchoFadeBand ("Echo Fade Band", Float) = 1.0
        _EchoOpacity ("Echo Opacity", Range(0,1)) = 1
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

            // Classic inverted-hull outline:
            // the mesh is expanded and only its back faces are drawn.
            // The target's depth-only pass hides the expanded shell everywhere
            // except the outside silhouette.
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
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float4 _EchoPoint;
                float _EchoRadius;
                float _EchoBand;
                float _EchoFadeRadius;
                float _EchoFadeBand;
                float _EchoOpacity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS);
                VertexNormalInputs normal = GetVertexNormalInputs(input.normalOS);

                float3 expandedPositionWS =
                    pos.positionWS + normal.normalWS * _OutlineWidth;

                output.positionWS = expandedPositionWS;
                output.positionHCS = TransformWorldToHClip(expandedPositionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float distanceFromEcho =
                    distance(input.positionWS, _EchoPoint.xyz);

                // Cumulative reveal:
                // everything behind the wave front remains visible.
                float revealBand = max(_EchoBand, 0.001);
                float reveal = 1.0 - smoothstep(
                    _EchoRadius - revealBand,
                    _EchoRadius,
                    distanceFromEcho
                );

                // Fade happens only after the whole contour is revealed.
                // The fade front starts from the original contact point and
                // moves outward, leaving already-unreached areas hidden.
                float fade = 1.0;

                if (_EchoFadeRadius > 0.0)
                {
                    float fadeBand = max(_EchoFadeBand, 0.001);

                    // Fade from the original contact point outward.
                    // Near the contact point the outline disappears first,
                    // while farther parts remain visible until the fade front
                    // reaches them.
                    fade = smoothstep(
                        _EchoFadeRadius - fadeBand,
                        _EchoFadeRadius,
                        distanceFromEcho
                    );
                }

                float alpha = reveal * fade * _OutlineColor.a * _EchoOpacity;

                if (alpha <= 0.001)
                    discard;

                return half4(_OutlineColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}