using System;

namespace UnityEngine.PostProcessing
{
    [Serializable]
    public class CanvasBlurModel : PostProcessingModel
    {
        [Serializable]
        public struct Settings
        {
            [Range(1, 5), Tooltip("Blur Iterations")]
            public int blurIterations;

            [Range(0, 1), Tooltip("Blur Amount")]
            public float blurAmount;

            public static Settings defaultSettings
            {
                get
                {
                    return new Settings
                    {
                        blurIterations = 2,
                        blurAmount = 1   
                    };
                }
            }
        }

        [SerializeField]
        Settings m_Settings = Settings.defaultSettings;
        public Settings settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }

        public override void Reset()
        {
            m_Settings = Settings.defaultSettings;
        }
    }
}
