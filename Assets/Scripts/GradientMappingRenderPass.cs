using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GradientMappingRenderPass : ScriptableRenderPass
{
    private const string ProfilerTag = "Gradient Mapping";
    private Material gradientMappingMaterial;
    private RenderTargetIdentifier source;
    private RenderTargetHandle tempTexture;
    private GradientMappingSettings settings;
    private Texture2D gradientTexture;

    public GradientMappingRenderPass()
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        tempTexture.Init("_TempGradientMappingTexture");
        gradientTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
    }
    
    public void Setup(Material material, GradientMappingSettings gradientMappingSettings)
    {
        this.gradientMappingMaterial = material;
        this.settings = gradientMappingSettings;
        
        // 将梯度更新到纹理
        UpdateGradientTexture();
    }
    
    public void SetSource(RenderTargetIdentifier source)
    {
        this.source = source;
    }

    private void UpdateGradientTexture()
    {
        if (settings == null || settings.gradient.value == null)
            return;
            
        for (int i = 0; i < 256; i++)
        {
            float t = i / 255f;
            Color color = settings.gradient.value.Evaluate(t);
            gradientTexture.SetPixel(i, 0, color);
        }
        
        gradientTexture.Apply();
        gradientMappingMaterial.SetTexture("_GradientTex", gradientTexture);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (gradientMappingMaterial == null || !settings.active.value || settings.intensity.value <= 0)
            return;

        CommandBuffer cmd = CommandBufferPool.Get(ProfilerTag);

        // 获取描述符并创建临时渲染纹理
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        cmd.GetTemporaryRT(tempTexture.id, descriptor);

        // 设置材质属性
        gradientMappingMaterial.SetFloat("_Intensity", settings.intensity.value);
        UpdateGradientTexture();

        // 执行后处理
        cmd.Blit(source, tempTexture.Identifier(), gradientMappingMaterial, 0);
        cmd.Blit(tempTexture.Identifier(), source);

        // 清理
        cmd.ReleaseTemporaryRT(tempTexture.id);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempTexture.id);
    }
} 