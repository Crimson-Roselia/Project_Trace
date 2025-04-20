using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GradientMappingRendererFeature : ScriptableRendererFeature
{
    private GradientMappingRenderPass gradientMappingPass;
    private Material gradientMappingMaterial;

    [SerializeField, HideInInspector]
    private Shader gradientMappingShader;

    public override void Create()
    {
        // 加载Shader
        if (gradientMappingShader == null)
        {
            gradientMappingShader = Shader.Find("Hidden/GradientMapping");
        }

        if (gradientMappingShader == null)
        {
            Debug.LogError("无法找到GradientMapping着色器。请确保着色器已创建并且在项目中。");
            return;
        }

        // 创建材质和渲染通道
        gradientMappingMaterial = CoreUtils.CreateEngineMaterial(gradientMappingShader);
        gradientMappingPass = new GradientMappingRenderPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 检查是否应该处理后处理
        if (renderingData.cameraData.postProcessEnabled && gradientMappingMaterial != null)
        {
            // 查找场景中的体积组件
            VolumeStack stack = VolumeManager.instance.stack;
            GradientMappingOverride gradientMapping = stack.GetComponent<GradientMappingOverride>();

            // 如果组件处于活动状态，设置通道并添加它
            if (gradientMapping != null && gradientMapping.IsActive())
            {
                gradientMappingPass.SetSource(renderer.cameraColorTarget);
                gradientMappingPass.Setup(gradientMappingMaterial, gradientMapping.settings);
                renderer.EnqueuePass(gradientMappingPass);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CoreUtils.Destroy(gradientMappingMaterial);
        }
    }
} 