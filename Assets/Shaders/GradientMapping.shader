Shader "Hidden/GradientMapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_GradientTex);
            SAMPLER(sampler_GradientTex);
            float _Intensity;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float Luminance(float3 color)
            {
                return dot(color, float3(0.2126, 0.7152, 0.0722));
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 originalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // 计算亮度值 (0-1)
                float luminance = Luminance(originalColor.rgb);
                
                // 从梯度纹理中采样颜色
                half4 gradientColor = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(luminance, 0));
                
                // 根据强度参数混合原始颜色和梯度颜色
                half4 finalColor = lerp(originalColor, gradientColor, _Intensity);
                finalColor.a = originalColor.a; // 保持原始alpha值
                
                return finalColor;
            }
            ENDHLSL
        }
    }
} 