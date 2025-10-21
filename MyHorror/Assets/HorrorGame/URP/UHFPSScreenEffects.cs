using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


    public class UHFPSScreenEffects : ScriptableRendererFeature
    {
        [SerializeReference]
        public List<EffectFeature> Features = new()
        {
            new FearTentanclesFeature(),
        };

        public override void Create()
        {
            Features.ForEach(feature => feature.OnCreate());
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            foreach (var feature in Features)
            {
                ScriptableRenderPass pass = feature.OnGetRenderPass();
                if (pass != null && feature.Enabled) renderer.EnqueuePass(pass);
            }
        }
    }
