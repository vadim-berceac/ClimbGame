using UnityEngine;

public class MoveSpeed
{
    private readonly InputHandler _handler;

    private Vector3 _currentInput = Vector3.zero;
    private float _currentForwardSpeed;
    private float _currentBackwardSpeed;
    private float _currentStrafeLeftSpeed;
    private float _currentStrafeRightSpeed;

    private const float _inputSmoothTime = 0.22f;
    private const float _speedSmoothTime = 0.12f; 

    private Vector3 _inputVelocity = Vector3.zero;
    private float _fwdVel, _bwdVel, _leftVel, _rightVel;

    public MoveSpeed(InputHandler handler)
    {
        _handler = handler;
    }

    public float GetSpeed(MoveSpeedData data, float t)
    {
        var rawInput = _handler.MoveInput;
        var targetInput = ClampInput(rawInput, data, t);

        _currentInput = Vector3.SmoothDamp(_currentInput, targetInput, ref _inputVelocity, _inputSmoothTime);

        var mag = _currentInput.magnitude;
        if (mag < 0.01f)
        {
            _currentForwardSpeed = Mathf.SmoothDamp(_currentForwardSpeed, 0f, ref _fwdVel, _speedSmoothTime);
            _currentBackwardSpeed = Mathf.SmoothDamp(_currentBackwardSpeed, 0f, ref _bwdVel, _speedSmoothTime);
            _currentStrafeLeftSpeed = Mathf.SmoothDamp(_currentStrafeLeftSpeed, 0f, ref _leftVel, _speedSmoothTime);
            _currentStrafeRightSpeed = Mathf.SmoothDamp(_currentStrafeRightSpeed, 0f, ref _rightVel, _speedSmoothTime);

            return data.GetIdle(t);
        }

        var dir = _currentInput / mag;
        var forwardVal = dir.y;
        var strafeVal  = dir.x;

        var targetFwd  = forwardVal > 0f ? data.GetForward(t)   : 0f;
        var targetBwd  = forwardVal < 0f ? data.GetBackward(t)  : 0f;
        var targetLeft = strafeVal  < 0f ? data.GetStrafeLeft(t): 0f;
        var targetRight= strafeVal  > 0f ? data.GetStrafeRight(t): 0f;

        _currentForwardSpeed  = Mathf.SmoothDamp(_currentForwardSpeed,  targetFwd,  ref _fwdVel,  _speedSmoothTime);
        _currentBackwardSpeed = Mathf.SmoothDamp(_currentBackwardSpeed, targetBwd,  ref _bwdVel,  _speedSmoothTime);
        _currentStrafeLeftSpeed = Mathf.SmoothDamp(_currentStrafeLeftSpeed, targetLeft, ref _leftVel, _speedSmoothTime);
        _currentStrafeRightSpeed = Mathf.SmoothDamp(_currentStrafeRightSpeed, targetRight,ref _rightVel, _speedSmoothTime);

        var absFwd = Mathf.Abs(forwardVal);
        var absStr = Mathf.Abs(strafeVal);

        var total = absFwd + absStr;
        if (total < 0.001f) return data.GetIdle(t);

        var weightedSpeed =
            (absFwd * (_currentForwardSpeed + _currentBackwardSpeed) +
             absStr * (_currentStrafeLeftSpeed + _currentStrafeRightSpeed)) / total;

        return weightedSpeed;
    }

    private Vector3 ClampInput(Vector3 input, MoveSpeedData data, float t)
    {
        var blockRight = input.x > 0.01f && data.GetStrafeRight(t) <= 0f;
        var blockLeft  = input.x < -0.01f && data.GetStrafeLeft(t)  <= 0f;
        var blockFwd   = input.y > 0.01f && data.GetForward(t)      <= 0f;
        var blockBwd   = input.y < -0.01f && data.GetBackward(t)    <= 0f;

        if (blockRight || blockLeft) input.x = 0f;
        if (blockFwd   || blockBwd)  input.y = 0f;

        return input;
    }

    public Vector3 GetCurrentInput() => _currentInput;

    public void Reset()
    {
        _currentInput = Vector3.zero;
        _currentForwardSpeed = _currentBackwardSpeed = _currentStrafeLeftSpeed = _currentStrafeRightSpeed = 0f;
        _inputVelocity = Vector3.zero;
        _fwdVel = _bwdVel = _leftVel = _rightVel = 0f;
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
