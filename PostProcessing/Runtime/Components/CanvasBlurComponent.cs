using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing
{
    using System;
    using DebugMode = BuiltinDebugViewsModel.Mode;

    public sealed class CanvasBlurComponent : PostProcessingComponentCommandBuffer<CanvasBlurModel>
    {
        static class Uniforms
        {
            internal static readonly int _BlurTex = Shader.PropertyToID("_BlurTex");
            internal static readonly int _DitheringTex = Shader.PropertyToID ("_CanvasBlurDitheringTex");
            internal static readonly int _DitheringCoords = Shader.PropertyToID ("_CanvasBlurDitheringCoords");
        }

        const string k_ShaderString = "Hidden/Post FX/Seperable Blur";
        Material m_Material;

        public override bool active
        {
            get
            {
                return model.enabled
                       && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2 // No movecs on GLES2 platforms
                       && !context.interrupted;
            }
        }

        // Holds 64 64x64 Alpha8 textures (256kb total)
        Texture2D[] noiseTextures;
        int textureIndex = 0;

        const int k_TextureCount = 64;

        void LoadNoiseTextures ()
        {
            noiseTextures = new Texture2D[k_TextureCount];

            for (int i = 0; i < k_TextureCount; i++)
                noiseTextures[i] = Resources.Load<Texture2D> ("Bluenoise64/LDR_LLL1_" + i);
        }

        public override CameraEvent GetCameraEvent ()
        {
            return CameraEvent.AfterEverything;
        }

        public override string GetName ()
        {
            return "Canvas Blur";
        }

        public override void PopulateCommandBuffer (CommandBuffer cb)
        {             
            if (!m_Material)
            {
                m_Material = new Material (Shader.Find (k_ShaderString));
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }

            if (++textureIndex >= k_TextureCount)
                textureIndex = 0;

            float rndOffsetX = UnityEngine.Random.value;
            float rndOffsetY = UnityEngine.Random.value;

            if (noiseTextures == null)
                LoadNoiseTextures ();

            var noiseTex = noiseTextures[textureIndex];
                            
            Shader.SetGlobalTexture (Uniforms._DitheringTex, noiseTex);
            Shader.SetGlobalVector (Uniforms._DitheringCoords, new Vector4 (
                (float)context.width / (float)noiseTex.width,
                (float)context.height / (float)noiseTex.height,
                rndOffsetX,
                rndOffsetY
            ));

            // copy screen into temporary RT
            int screenCopyID = Shader.PropertyToID ("_ScreenCopyTexture");
            cb.GetTemporaryRT (screenCopyID, -1, -1, 0, FilterMode.Bilinear);
            cb.Blit (BuiltinRenderTextureType.CameraTarget, screenCopyID);

            // get two smaller RTs
            int blurredID = Shader.PropertyToID ("_Temp1");
            int blurredID2 = Shader.PropertyToID ("_Temp2");
            cb.GetTemporaryRT (blurredID, -2, -2, 0, FilterMode.Bilinear);
            cb.GetTemporaryRT (blurredID2, -2, -2, 0, FilterMode.Bilinear);

            // downsample screen copy into smaller RT, release screen RT
            cb.Blit (screenCopyID, blurredID);
            cb.ReleaseTemporaryRT (screenCopyID);

            int blurIterations = model.settings.blurIterations;
            float blurAmount = model.settings.blurAmount;

            for (int i = 0; i < blurIterations; ++i)
            {
                int multiplier = (int) Mathf.Pow (2, i);
                // horizontal blur
                cb.SetGlobalVector ("offsets", new Vector4 (2.0f * multiplier / Screen.width, 0, 0, 0));
                cb.Blit (blurredID, blurredID2, m_Material);
                // vertical blur
                cb.SetGlobalVector ("offsets", new Vector4 (0, 2.0f * multiplier / Screen.height, 0, 0));
                cb.Blit (blurredID2, blurredID, m_Material);                    
            }                   

            cb.SetGlobalTexture ("_GrabBlurTexture", blurredID);
        }

        public override void OnDisable()
        {
            noiseTextures = null;
        }
    }
}
