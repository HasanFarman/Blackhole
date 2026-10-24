Shader "Blackhole/Fresnel"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.72, 0.42, 1)
        _Power ("Power", Float) = 2.6
        _Intensity ("Intensity", Float) = 1.4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend One One
        ZWrite Off
        Cull Back

        Pass
        {
            Name "Fresnel"
            Tags { "LightMode" = "UniversalForward" }

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
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            half4 _Color;
            half _Power;
            half _Intensity;

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs positions = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = positions.positionCS;
                o.positionWS = positions.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 viewDir = normalize(GetWorldSpaceViewDir(i.positionWS));
                half facing = saturate(dot(normalize(i.normalWS), viewDir));
                half rim = pow(1.0h - facing, _Power);
                half3 rgb = _Color.rgb * rim * _Intensity;
                return half4(rgb, 0);
            }
            ENDHLSL
        }
    }
}
