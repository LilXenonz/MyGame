using UnityEngine.Rendering.Universal;
using System;


[Serializable]
public abstract class EffectFeature
{
    public ScriptableRenderPass RenderPass;
    public bool Enabled = true;

    public abstract string Name { get; }
    public abstract void OnCreate();

    public virtual ScriptableRenderPass OnGetRenderPass()
    {
        if (RenderPass == null)
            return null;

        RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
        return RenderPass;
    }
}
