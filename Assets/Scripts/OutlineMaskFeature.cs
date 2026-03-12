using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.Universal.FullScreenPassRendererFeature;

public class OutlineMaskFeature : ScriptableRendererFeature
{
    class MaskPass : ScriptableRenderPass
    {
        public LayerMask outlineLayer;
        public Material maskMaterial;
        private static readonly int maskTargetID = Shader.PropertyToID("_Mask_Texture");

        // The new Render Graph system requires a data struct to pass information around
        private class PassData
        {
            public RendererListHandle rendererList;
        }

        public MaskPass(LayerMask layer, Material material)
        {
            outlineLayer = layer;
            maskMaterial = material;
        }

        // This is Unity 6's brand new method for rendering!
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // 1. Grab camera and rendering data
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            // 2. Set up the texture settings
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.colorFormat = RenderTextureFormat.ARGB32;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            // 3. Ask the Render Graph to allocate a texture for us
            TextureHandle maskTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_Mask_Texture", true);

            // 4. Set up the drawing rules (Paint them red!)
            SortingCriteria sortingCriteria = cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(new ShaderTagId("UniversalForward"), renderingData, cameraData, lightData, sortingCriteria);
            drawingSettings.overrideMaterial = maskMaterial;
            drawingSettings.SetShaderPassName(1, new ShaderTagId("SRPDefaultUnlit"));

            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all, outlineLayer);

            // 5. Create a list of objects to draw
            RendererListParams listParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
            RendererListHandle rendererList = renderGraph.CreateRendererList(listParams);

            // 6. Build and execute the Raster Pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline Mask Pass", out var passData))
            {
                passData.rendererList = rendererList;

                // Set our texture as the target, and clear it to black
                builder.SetRenderAttachment(maskTexture, 0, AccessFlags.Write);

                // Tell the graph we need to use this list of objects
                builder.UseRendererList(rendererList);
                builder.AllowPassCulling(false); // Force it to run

                // CRUCIAL: Expose this texture globally so your Shader Graph can catch it!
                builder.SetGlobalTextureAfterPass(maskTexture, maskTargetID);

                // Draw it!
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    // Clear the texture to pure black right before we draw the red objects
                    context.cmd.ClearRenderTarget(false, true, Color.black);

                    // Draw the list
                    context.cmd.DrawRendererList(data.rendererList);

                });
            }
        }

        // We leave the old Execute method blank but present to prevent Unity from throwing legacy errors
        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }

    public LayerMask outlineLayer;
    public Material maskMaterial;
    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
    private MaskPass maskPass;

    public override void Create()
    {
        maskPass = new MaskPass(outlineLayer, maskMaterial);
        maskPass.renderPassEvent = injectionPoint;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (maskMaterial != null)
        {
            renderer.EnqueuePass(maskPass);
        }
    }
}