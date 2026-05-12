using UnityEngine;

public class MoveSpeed
{
    private readonly InputHandler _handler;

    private Vector3 _input;
    private float _mag;
    private float _invMag;
    private float _strafeVal;
    private float _forwardVal;
    private float _absForwardVal;
    private float _absStrafeVal;
    private float _totalContrib;
    private float _weightedSpeed;

    public MoveSpeed(InputHandler handler)
    {
        _handler = handler;
    }

    public float GetSpeed(MoveSpeedData currentMoveSpeedData, float t)
    {
        _input = GetClampedInput(currentMoveSpeedData, t);
        _mag   = _input.magnitude;

        if (_mag < 0.01f)
            return currentMoveSpeedData.GetIdle(t);

        _invMag      = 1f / _mag;
        _strafeVal   = _input.x * _invMag;
        _forwardVal  = _input.y * _invMag;

        _absForwardVal = Mathf.Abs(_forwardVal);
        _absStrafeVal  = Mathf.Abs(_strafeVal);

        _totalContrib = _absForwardVal + _absStrafeVal;

        _weightedSpeed =
            (_absForwardVal * (_forwardVal > 0f ? currentMoveSpeedData.GetForward(t) : currentMoveSpeedData.GetBackward(t)) +
             _absStrafeVal  * (_strafeVal  > 0f ? currentMoveSpeedData.GetStrafeRight(t) : currentMoveSpeedData.GetStrafeLeft(t))) /
            _totalContrib;

        return _weightedSpeed;
    }

    public Vector3 GetClampedInput(MoveSpeedData currentMoveSpeedData, float t)
    {
        var input = _handler.MoveInput;

        var strafeRightBlocked = input.x > 0f && currentMoveSpeedData.GetStrafeRight(t) == 0f;
        var strafeLeftBlocked  = input.x < 0f && currentMoveSpeedData.GetStrafeLeft(t)  == 0f;
        var forwardBlocked     = input.y > 0f && currentMoveSpeedData.GetForward(t)     == 0f;
        var backwardBlocked    = input.y < 0f && currentMoveSpeedData.GetBackward(t)    == 0f;

        if (strafeRightBlocked || strafeLeftBlocked) input.x = 0f;
        if (forwardBlocked     || backwardBlocked)   input.y = 0f;

        return input;
    }
}

[System.Serializable]
public struct MoveSpeedData
{
    [field: SerializeField] public AnimationCurve IdleSpeedCurve { get; set; }
    [field: SerializeField] public AnimationCurve ForwardSpeedCurve { get; set; }
    [field: SerializeField] public AnimationCurve BackwardSpeedCurve { get; set; }
    [field: SerializeField] public AnimationCurve StrafeLeftSpeedCurve { get; set; }
    [field: SerializeField] public AnimationCurve StrafeRightSpeedCurve { get; set; }
    [field: SerializeField] public AnimationCurve YSpeedCurve { get; set; }

    public float GetIdle(float t) => IdleSpeedCurve.Evaluate(t);
    public float GetForward(float t) => ForwardSpeedCurve.Evaluate(t);
    public float GetBackward(float t) => BackwardSpeedCurve.Evaluate(t);
    public float GetStrafeLeft(float t) => StrafeLeftSpeedCurve.Evaluate(t);
    public float GetStrafeRight(float t) => StrafeRightSpeedCurve.Evaluate(t);
    public float GetY(float t) => YSpeedCurve.Evaluate(t);
}
