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

    public float GetSpeed(MoveSpeedData currentMoveSpeedData)
    {
        _input = GetClampedInput(currentMoveSpeedData);
        _mag   = _input.magnitude;

        if (_mag < 0.01f)
            return currentMoveSpeedData.IdleSpeed;

        _invMag      = 1f / _mag;
        _strafeVal   = _input.x * _invMag;
        _forwardVal  = _input.y * _invMag;

        _absForwardVal = Mathf.Abs(_forwardVal);
        _absStrafeVal  = Mathf.Abs(_strafeVal);

        _totalContrib = _absForwardVal + _absStrafeVal;

        _weightedSpeed =
            (_absForwardVal * (_forwardVal > 0f ? currentMoveSpeedData.ForwardSpeed : currentMoveSpeedData.BackwardSpeed) +
             _absStrafeVal  * (_strafeVal  > 0f ? currentMoveSpeedData.StrafeRightSpeed : currentMoveSpeedData.StrafeLeftSpeed)) /
            _totalContrib;

        return _weightedSpeed;
    }

    public Vector3 GetClampedInput(MoveSpeedData currentMoveSpeedData)
    {
        var input = _handler.MoveInput;

        var strafeRightBlocked = input.x > 0f && currentMoveSpeedData.StrafeRightSpeed == 0f;
        var strafeLeftBlocked  = input.x < 0f && currentMoveSpeedData.StrafeLeftSpeed  == 0f;
        var forwardBlocked     = input.y > 0f && currentMoveSpeedData.ForwardSpeed     == 0f;
        var backwardBlocked    = input.y < 0f && currentMoveSpeedData.BackwardSpeed    == 0f;

        if (strafeRightBlocked || strafeLeftBlocked) input.x = 0f;
        if (forwardBlocked     || backwardBlocked)   input.y = 0f;

        return input;
    }
}

[System.Serializable]
public struct MoveSpeedData
{
    [field: SerializeField] public float IdleSpeed { get; set; }
    [field: SerializeField] public float ForwardSpeed { get; set; }
    [field: SerializeField] public float BackwardSpeed { get; set; }
    [field: SerializeField] public float StrafeLeftSpeed { get; set; }
    [field: SerializeField] public float StrafeRightSpeed { get; set; }
    [field: SerializeField] public float YSpeed { get; set; }
}
