using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class GradientParameter : VolumeParameter<Gradient>
{
    public GradientParameter(Gradient value, bool overrideState = false)
        : base(value, overrideState)
    {
    }

    public override void Interp(Gradient from, Gradient to, float t)
    {
        // 梯度不能插值，我们只能使用目标梯度
        m_Value = t > 0 ? to : from;
    }
}

[System.Serializable]
public class GradientMappingSettings
{
    [Tooltip("是否启用梯度映射效果")]
    public BoolParameter active = new BoolParameter(false);

    [Tooltip("亮度映射梯度")]
    public GradientParameter gradient = new GradientParameter(new Gradient());

    [Range(0, 1), Tooltip("效果强度")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(1f, 0f, 1f);
}

public class GradientMappingOverride : VolumeComponent, IPostProcessComponent
{
    public GradientMappingSettings settings = new GradientMappingSettings();

    public bool IsActive() => settings.active.value && settings.intensity.value > 0;
    
    public bool IsTileCompatible() => true;
}
