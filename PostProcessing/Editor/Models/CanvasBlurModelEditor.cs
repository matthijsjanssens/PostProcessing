using UnityEngine.PostProcessing;

namespace UnityEditor.PostProcessing
{
    using Settings = CanvasBlurModel.Settings;

    [PostProcessingModelEditor(typeof(CanvasBlurModel))]
    public class CanvasBlurModelEditor : PostProcessingModelEditor
    {
        SerializedProperty m_BlurIterations;
        SerializedProperty m_BlurAmount;

        public override void OnEnable()
        {
            m_BlurIterations = FindSetting((Settings x) => x.blurIterations);
            m_BlurAmount = FindSetting ((Settings x) => x.blurAmount);           
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox ("After all the image processing this component generates a blurred texture for UI elements using a blur material.", MessageType.Info);
            EditorGUILayout.PropertyField (m_BlurIterations);

            // TODO
            // Add a blur amount slider that interpolates smoothly between blur iteration steps
            //EditorGUILayout.PropertyField (m_BlurAmount);
        }
    }
}
