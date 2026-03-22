namespace Lifey.Attributes
{
#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Disable GUI so field is visible but not editable
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true; // Re-enable GUI after drawing
        }
    }
#endif
}
