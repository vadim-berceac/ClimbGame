using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CurveExtractorSO", menuName = "Scriptable Objects/Animation/CurveExtractorSO")]
public class CurveExtractorSO : ScriptableObject
{
    [SerializeField] public AnimationClip Target;

    [SerializeField] public float Multiplier = 1f;

    [SerializeField] [ReadOnly] public AnimationCurve ForwardSpeedCurve;
    [SerializeField] [ReadOnly] public AnimationCurve BackwardSpeedCurve;
    [SerializeField] [ReadOnly] public AnimationCurve StrafeLeftSpeedCurve;
    [SerializeField] [ReadOnly] public AnimationCurve StrafeRightSpeedCurve;
    [SerializeField] [ReadOnly] public AnimationCurve YSpeedCurve;

    private void OnValidate()
    {
        Clear();
        Extract();
    }

    private void Extract()
    {
        if (Target == null)
            return;

        var frameRate = Target.frameRate;
        var frameTime = 1f / frameRate;
        var frameCount = Mathf.RoundToInt(Target.length * frameRate);

        var forwardSpeeds = new System.Collections.Generic.List<Keyframe>();
        var backwardSpeeds = new System.Collections.Generic.List<Keyframe>();
        var strafeLeftSpeeds = new System.Collections.Generic.List<Keyframe>();
        var strafeRightSpeeds = new System.Collections.Generic.List<Keyframe>();
        var ySpeeds = new System.Collections.Generic.List<Keyframe>();

        var prevPosition = Vector3.zero;

        for (var i = 0; i <= frameCount; i++)
        {
            var time = i * frameTime;
            var currentPosition = SampleRootMotionPosition(time);

            if (i > 0)
            {
                var velocity = (currentPosition - prevPosition) / frameTime;

                var forward = velocity.z * Multiplier;
                var strafe = velocity.x * Multiplier;
                var vertical = velocity.y * Multiplier;

                if (forward >= 0)
                {
                    forwardSpeeds.Add(new Keyframe(time, forward));
                    backwardSpeeds.Add(new Keyframe(time, 0));
                }
                else
                {
                    forwardSpeeds.Add(new Keyframe(time, 0));
                    backwardSpeeds.Add(new Keyframe(time, -forward));
                }

                if (strafe >= 0)
                {
                    strafeRightSpeeds.Add(new Keyframe(time, strafe));
                    strafeLeftSpeeds.Add(new Keyframe(time, 0));
                }
                else
                {
                    strafeRightSpeeds.Add(new Keyframe(time, 0));
                    strafeLeftSpeeds.Add(new Keyframe(time, -strafe));
                }

                ySpeeds.Add(new Keyframe(time, vertical));
            }

            prevPosition = currentPosition;
        }

        ForwardSpeedCurve = new AnimationCurve(forwardSpeeds.ToArray());
        BackwardSpeedCurve = new AnimationCurve(backwardSpeeds.ToArray());
        StrafeLeftSpeedCurve = new AnimationCurve(strafeLeftSpeeds.ToArray());
        StrafeRightSpeedCurve = new AnimationCurve(strafeRightSpeeds.ToArray());
        YSpeedCurve = new AnimationCurve(ySpeeds.ToArray());
    }

    private void Clear()
    {
        ForwardSpeedCurve = null;
        BackwardSpeedCurve = null;
        StrafeLeftSpeedCurve = null;
        StrafeRightSpeedCurve = null;
        YSpeedCurve = null;
    }

#if UNITY_EDITOR
    private Vector3 SampleRootMotionPosition(float time)
    {
        float x = 0, y = 0, z = 0;

        var xCurve = UnityEditor.AnimationUtility.GetEditorCurve(Target, 
            new UnityEditor.EditorCurveBinding { path = "", type = typeof(Animator), propertyName = "RootT.x" });
        var yCurve = UnityEditor.AnimationUtility.GetEditorCurve(Target, 
            new UnityEditor.EditorCurveBinding { path = "", type = typeof(Animator), propertyName = "RootT.y" });
        var zCurve = UnityEditor.AnimationUtility.GetEditorCurve(Target, 
            new UnityEditor.EditorCurveBinding { path = "", type = typeof(Animator), propertyName = "RootT.z" });

        if (xCurve != null) x = xCurve.Evaluate(time);
        if (yCurve != null) y = yCurve.Evaluate(time);
        if (zCurve != null) z = zCurve.Evaluate(time);

        return new Vector3(x, y, z);
    }
#endif
}
