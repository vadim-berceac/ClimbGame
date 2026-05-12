using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(FrameEventConfigField))]
public class FrameEventConfigFieldDrawer : PropertyDrawer
{
    private const float FPS = 30f;
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var lineHeight = EditorGUIUtility.singleLineHeight;
        var lineSpacing = EditorGUIUtility.standardVerticalSpacing;
        var height = lineHeight + lineSpacing;
        
        var foldoutKey = GetFoldoutKey(property);
        var isFoldoutOpen = EditorPrefs.GetBool(foldoutKey, true);
        
        if (isFoldoutOpen)
        {
            height += (lineHeight + lineSpacing) * 5; 
        }
        
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var lineHeight = EditorGUIUtility.singleLineHeight;
        var lineSpacing = EditorGUIUtility.standardVerticalSpacing;
        var currentY = position.y;
       
        var foldoutKey = GetFoldoutKey(property);
        var isFoldoutOpen = EditorPrefs.GetBool(foldoutKey, true);
        
        var clipProp = property.FindPropertyRelative(nameof(FrameEventConfigField.Clip));
        var clip = clipProp.objectReferenceValue as AnimationClip;
        
        var clipInfo = clip != null ? $" ({clip.name})" : " (Assign Clip)";
        
        var foldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
        isFoldoutOpen = EditorGUI.Foldout(foldoutRect, isFoldoutOpen, label.text + clipInfo, true);
        EditorPrefs.SetBool(foldoutKey, isFoldoutOpen);
        currentY += lineHeight + lineSpacing;
       
        if (isFoldoutOpen)
        {
            EditorGUI.indentLevel++;
            
            var maxFrame = 0;
            if (clip != null)
            {
                maxFrame = Mathf.FloorToInt(clip.length * FPS);
            }
            
            var beginProp = property.FindPropertyRelative(nameof(FrameEventConfigField.Begin));
            var endProp = property.FindPropertyRelative(nameof(FrameEventConfigField.End));
            var weightProp = property.FindPropertyRelative(nameof(FrameEventConfigField.WeightThreshold));
            
            if (maxFrame > 0)
            {
                beginProp.intValue = Mathf.Clamp(beginProp.intValue, 0, maxFrame);
                endProp.intValue = Mathf.Clamp(endProp.intValue, beginProp.intValue, maxFrame);
            }
           
            var clipRect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(clipRect, clipProp, new GUIContent("Clip"));
            currentY += lineHeight + lineSpacing;
          
            var beginRect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.BeginDisabledGroup(maxFrame == 0);
            beginProp.intValue = EditorGUI.IntSlider(beginRect, "Begin", beginProp.intValue, 0, maxFrame);
            EditorGUI.EndDisabledGroup();
            currentY += lineHeight + lineSpacing;
            
            var endRect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.BeginDisabledGroup(maxFrame == 0);
            endProp.intValue = EditorGUI.IntSlider(endRect, "End", endProp.intValue, beginProp.intValue, maxFrame);
            EditorGUI.EndDisabledGroup();
            currentY += lineHeight + lineSpacing;
           
            var weightRect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(weightRect, weightProp, new GUIContent("Weight Threshold"));
            currentY += lineHeight + lineSpacing;
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
  
    private string GetFoldoutKey(SerializedProperty property)
    {
        return $"FrameEventConfigField_Foldout_{property.propertyPath}";
    }
}

#endif