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
        
        var height = (lineHeight + lineSpacing) * 4;
       
        var onEnterProp = property.FindPropertyRelative(nameof(FrameEventConfigField.OnEnter));
        var onExitProp = property.FindPropertyRelative(nameof(FrameEventConfigField.OnExit));
        var onTickProp = property.FindPropertyRelative(nameof(FrameEventConfigField.OnTick));
        
        height += EditorGUI.GetPropertyHeight(onEnterProp) + lineSpacing;
        height += EditorGUI.GetPropertyHeight(onExitProp) + lineSpacing;
        height += EditorGUI.GetPropertyHeight(onTickProp) + lineSpacing;
        
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var targetObject = property.serializedObject.targetObject as Interactable;
        var maxFrame = 0;
        var clipInfo = "";

        if (targetObject != null && targetObject.InteractClip != null)
        {
            var clip = targetObject.InteractClip;
            maxFrame = Mathf.FloorToInt(clip.length * FPS);
            clipInfo = $" (0-{maxFrame} frames @ {FPS}fps)";
        }
        else
        {
            clipInfo = " (Assign InteractClip)";
        }

        var beginProp = property.FindPropertyRelative(nameof(FrameEventConfigField.Begin));
        var endProp = property.FindPropertyRelative(nameof(FrameEventConfigField.End));
        var weightProp = property.FindPropertyRelative(nameof(FrameEventConfigField.WeightThreshold));
        var onEnterProp = property.FindPropertyRelative(nameof(FrameEventConfigField.OnEnter));
        var onExitProp = property.FindPropertyRelative(nameof(FrameEventConfigField.OnExit));
        var onTickProp = property.FindPropertyRelative(nameof(FrameEventConfigField.OnTick));

        if (maxFrame > 0)
        {
            beginProp.intValue = Mathf.Clamp(beginProp.intValue, 0, maxFrame);
            endProp.intValue = Mathf.Clamp(endProp.intValue, beginProp.intValue, maxFrame);
        }

        var lineHeight = EditorGUIUtility.singleLineHeight;
        var lineSpacing = EditorGUIUtility.standardVerticalSpacing;
        var currentY = position.y;

        var labelRect = new Rect(position.x, currentY, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, label.text + clipInfo, EditorStyles.label);
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

        var onEnterHeight = EditorGUI.GetPropertyHeight(onEnterProp);
        var onEnterRect = new Rect(position.x, currentY, position.width, onEnterHeight);
        EditorGUI.PropertyField(onEnterRect, onEnterProp, new GUIContent("On Enter"), true);
        currentY += onEnterHeight + lineSpacing;

        var onExitHeight = EditorGUI.GetPropertyHeight(onExitProp);
        var onExitRect = new Rect(position.x, currentY, position.width, onExitHeight);
        EditorGUI.PropertyField(onExitRect, onExitProp, new GUIContent("On Exit"), true);
        currentY += onExitHeight + lineSpacing;

        var onTickHeight = EditorGUI.GetPropertyHeight(onTickProp);
        var onTickRect = new Rect(position.x, currentY, position.width, onTickHeight);
        EditorGUI.PropertyField(onTickRect, onTickProp, new GUIContent("On Tick"), true);

        EditorGUI.EndProperty();
    }
}

#endif