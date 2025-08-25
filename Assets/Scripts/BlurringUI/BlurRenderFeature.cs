// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;

// public class BlurRenderFeature : ScriptableRendererFeature
// {
//     class CustomRenderPass : ScriptableRenderPass
//     {
//         private Material blurMaterial;
//         private RenderTargetIdentifier source;
//         private RenderTargetHandle tempTexture;
//         private string profilerTag;

//         public CustomRenderPass(Material mat, string tag)
//         {
//             blurMaterial = mat;
//             profilerTag = tag;
//             tempTexture.Init("_TemporaryBlurTexture");
//         }

//         public void Setup(RenderTargetIdentifier src)
//         {
//             source = src;
//         }

//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//         {
//             CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

//             RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
//             cmd.GetTemporaryRT(tempTexture.id, desc);

//             // Apply blur
//             cmd.Blit(source, tempTexture.Identifier(), blurMaterial);
//             cmd.Blit(tempTexture.Identifier(), source);

//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
//         }
//     }

//     [System.Serializable]
//     public class Settings
//     {
//         public Material blurMaterial;
//         public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
//     }

//     public Settings settings = new Settings();
//     CustomRenderPass customPass;

//     public override void Create()
//     {
//         if (settings.blurMaterial == null) return;
//         customPass = new CustomRenderPass(settings.blurMaterial, "BlurPass");
//         customPass.renderPassEvent = settings.renderPassEvent;
//     }

//     public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//     {
//         if (settings.blurMaterial == null) return;
//         customPass.Setup(renderer.cameraColorTarget);
//         renderer.EnqueuePass(customPass);
//     }
// }
