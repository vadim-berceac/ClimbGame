using UnityEngine;

public class AdvancedCharacterController
{
    #region Constants
    
    private const float JumpVelocityThreshold      = 0.1f;
    private const float VerticalWallThreshold      = 0.5f;
    private const float MinClimbAngle              = 60f;
    private const float WallRotationSpeed          = 8f;
    private const float ClimbingRotationMultiplier = 5f;
    private const float ForcedFallSpeed            = -8f;
    private const int   ForcedFallFrames           = 10;
    private const float GroundSnapVelocity         = -2f;
    private const float LedgeClimbMultiplier       = 3.0f;
    private const float LedgeForwardPushMultiplier = 1.5f;
    private const float RotationSnapThreshold      = 0.1f;
    private const float TerminalVelocity           = -53f;
    private const float FallGravityMultiplier      = 2f;
    private const float WallSnapGap                = 0.02f;
    private const float CoyoteTimeDuration         = 0.1f;

    #endregion

    #region Configuration (Readonly)

    private readonly CharacterController _controller;
    private readonly Transform _transform;
    private readonly LayerMask _groundMask;
    private readonly LayerMask _climbMask;

    private readonly float _groundCheckDistance;
    private readonly float _slopeForce;
    private readonly float _climbCheckDistance;
    private readonly float _maxClimbAngle;
    private readonly float _climbRayOffset;
    private readonly float _climbSpeedMultiplier;
    private readonly float _speedChangeRate;
    private readonly bool  _enableClimbing;

    #endregion

    #region State

    private Vector3 _moveInput;
    private Vector3 _velocity;
    private Vector3 _climbNormal;
    private Vector3 _rotationInput;
    private RaycastHit _slopeHit;

    private float _targetSpeed;
    private float _smoothedTargetSpeed;
    private float _rotationSpeed;
    private float _jumpHeight;
    private float _gravity;
    private float _coyoteTimeCounter;

    private bool _isGrounded;
    private bool _isClimbing;
    private bool _isJumping;
    private bool _jumpRequested;
    private bool _isOnClimbableSurface;

    #endregion

    #region Wall Rotation State

    private bool _isRotatingToWall;
    private Quaternion _targetWallRotation;
    private Quaternion _startRotation;
    private float _wallRotationProgress;

    #endregion

    #region Forced Falling State

    private int _forcedFallingFrames;

    #endregion

    #region Public Properties

    public bool   IsGrounded()           => _isGrounded;
    public bool   IsClimbing()           => _isClimbing;
    public bool   IsOnClimbableSurface() => _isOnClimbableSurface;
    public bool   IsJumping()            => _isJumping;
    public bool   IsFalling()            => !_isGrounded && !_isOnClimbableSurface && _velocity.y < -JumpVelocityThreshold;
    public bool   IsAirborne()           => !_isGrounded && !_isClimbing && !_isOnClimbableSurface;
    public Vector3 Velocity              => _transform.InverseTransformDirection(_velocity);
    public Vector2 HorizontalVelocity    => new (Velocity.x, Velocity.z);

    #endregion

    #region Initialization

    public AdvancedCharacterController(CharacterController characterController, AdvancedCharacterControllerData data)
    {
        _controller           = characterController;
        _transform            = characterController.transform;

        _groundCheckDistance  = data.GroundCheckDistance;
        _groundMask           = data.GroundMask;
        _slopeForce           = data.SlopeForce;
        _enableClimbing       = data.EnableClimbing;
        _climbCheckDistance   = data.ClimbCheckDistance;
        _maxClimbAngle        = data.MaxClimbAngle;
        _climbRayOffset       = data.ClimbRayOffset;
        _climbSpeedMultiplier = data.ClimbSpeedMultipler;
        _speedChangeRate      = 10f;
        _climbMask            = data.ClimbMask;

        _smoothedTargetSpeed = 0f;
    }

    #endregion

    #region Public Methods

    public void Move(Vector3 motion, float inputSpeed, float? speedChangeRate = null)
    {
        if(!_controller.enabled) return;
        
        _moveInput   = motion;
        _targetSpeed = inputSpeed;

        var rate = speedChangeRate ?? _speedChangeRate;
        var dt   = Time.deltaTime;
        
        if (_moveInput.magnitude < 0.1f)
        {
            _smoothedTargetSpeed = Mathf.MoveTowards(_smoothedTargetSpeed, 0f, rate * 1.6f * dt);
        }
        else
        {
            _smoothedTargetSpeed = Mathf.MoveTowards(_smoothedTargetSpeed, _targetSpeed, rate * dt);
        }

        CheckGround();

        if (_enableClimbing)
        {
            CheckClimbing();
        }

        HandleMovement();

        _controller.Move(_velocity * dt);
    }

    public void JumpAndGravity(bool jumpPressed, float jumpHeight, float gravityMultiplier = 1f)
    {
        if(!_controller.enabled) return;
        
        _jumpHeight = jumpHeight;
        _gravity    = gravityMultiplier * Physics.gravity.y;

        HandleJump(jumpPressed);
        UpdateJumpState();
        ApplyGravity();
    }

    public void Rotation(Vector3 rotation, float rotationSpeed)
    {
        if(!_controller.enabled) return;
        
        _rotationInput = rotation;
        _rotationSpeed = rotationSpeed;

        if (_isRotatingToWall)
        {
            UpdateWallRotation();
            return;
        }

        if (_isClimbing || _isOnClimbableSurface)
        {
            AlignToSurface();
            return;
        }

        AlignToVertical();
        ApplyNormalRotation();
    }

    #endregion

    #region Ground Detection

    private void CheckGround()
    {
        if (_forcedFallingFrames > 0)
        {
            _forcedFallingFrames--;
            _isGrounded = false;
            _coyoteTimeCounter = 0f;
            return;
        }

        var origin   = _transform.position;
        var distance = _controller.height / 2f + _groundCheckDistance;

        if (Physics.SphereCast(origin, _controller.radius, Vector3.down, out var hit, distance, _groundMask))
        {
            var surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
            _isGrounded = surfaceAngle <= _controller.slopeLimit;

            if (_isGrounded && _velocity.y < 0 && _velocity.y > -10f)
            {
                _velocity.y = GroundSnapVelocity;
            }
            
            if (_isGrounded)
                _coyoteTimeCounter = CoyoteTimeDuration;

            _slopeHit = hit;
        }
        else
        {
            _isGrounded = false;
            _coyoteTimeCounter = Mathf.Max(0f, _coyoteTimeCounter - Time.deltaTime);
        }
    }

    #endregion

    #region Climbing Detection

    private void CheckClimbing()
    {
        if (_forcedFallingFrames > 0 || _isJumping)
        {
            ResetClimbingState();
            return;
        }

        if (ShouldDetachFromWall())
        {
            _isOnClimbableSurface = false;
            _isClimbing = false;
            return;
        }

        if (TryFindClimbableSurface(out var hit))
        {
            if (ShouldFallFromEdge(hit))
            {
                InitiateForcedFall();
                return;
            }

            AttachToSurface(hit);
        }
        else
        {
            _isOnClimbableSurface = false;
            _isClimbing = false;
        }
    }

    private bool ShouldDetachFromWall()
    {
        if (!_isOnClimbableSurface || _moveInput.y >= -0.1f) return false;

        var isVerticalWall = Mathf.Abs(_climbNormal.y) < VerticalWallThreshold;
        return isVerticalWall && _isGrounded;
    }

    private bool TryFindClimbableSurface(out RaycastHit hit)
    {
        var rayOrigin = _transform.position + Vector3.up * _climbRayOffset;

        if (!Physics.SphereCast(rayOrigin, _controller.radius * 0.5f, _transform.forward,
                out hit, _climbCheckDistance, _climbMask))
            return false;

        var angle = Vector3.Angle(Vector3.up, hit.normal);
        var deviationFromVertical = Mathf.Abs(90f - angle);

        return angle > MinClimbAngle && deviationFromVertical <= _maxClimbAngle;
    }

    private bool ShouldFallFromEdge(RaycastHit hit)
    {
        if (!_isGrounded || hit.point.y >= _transform.position.y || _moveInput.y >= -0.1f)
            return false;

        var backCheckPos = _transform.position - _transform.forward * (_controller.radius * 0.5f);
        return !Physics.Raycast(backCheckPos, Vector3.down, _controller.height, _groundMask);
    }

    private void InitiateForcedFall()
    {
        _isOnClimbableSurface = false;
        _isClimbing = false;
        _isGrounded = false;
        _forcedFallingFrames = ForcedFallFrames;
    }

    private void AttachToSurface(RaycastHit hit)
    {
        var wasNotAttached = !_isOnClimbableSurface;

        if (wasNotAttached)
        {
            InitiateWallRotation(hit.normal);
        }

        _isOnClimbableSurface = true;
        _climbNormal = hit.normal;
        _isClimbing = _moveInput.magnitude > 0.1f;
    }

    private void ResetClimbingState()
    {
        _isOnClimbableSurface = false;
        _isClimbing = false;
        _isRotatingToWall = false;
    }

    #endregion

    #region Movement Handling

    private void HandleMovement()
    {
        if (_forcedFallingFrames > 0)
        {
            ApplyForcedFallVelocity();
            return;
        }

        if (_isClimbing)
        {
            _velocity = CalculateClimbingVelocity();
            return;
        }

        if (_isOnClimbableSurface)
        {
            var snap    = CalculateWallSnapVelocity();
            _velocity.x = snap.x;
            _velocity.z = snap.z;
            return;
        }

        if (_moveInput.magnitude < 0.1f)
        {
            if (_isGrounded)
            {
                _velocity.x = 0f;
                _velocity.z = 0f;
            }
            else
            {
                _velocity.x = Mathf.Lerp(_velocity.x, 0f, 5f * Time.deltaTime);
                _velocity.z = Mathf.Lerp(_velocity.z, 0f, 5f * Time.deltaTime);
            }
        }
        else
        {
            var targetMove = OnSlope() ? CalculateSlopeVelocity() : CalculateNormalVelocity();
            _velocity.x = targetMove.x;
            _velocity.z = targetMove.z;
        }
    }

    private void ApplyForcedFallVelocity()
    {
        var backward = -_transform.forward;
        _velocity.x = backward.x * _smoothedTargetSpeed * 0.8f;
        _velocity.z = backward.z * _smoothedTargetSpeed * 0.8f;
        _velocity.y = ForcedFallSpeed;
    }

    private Vector3 CalculateClimbingVelocity()
    {
        var desired = Vector3.zero;

        if (Mathf.Abs(_moveInput.y) > 0.1f)
            desired += _transform.up * _moveInput.y;

        if (Mathf.Abs(_moveInput.x) > 0.1f)
            desired += _transform.right * _moveInput.x;

        var climbSpeed = _smoothedTargetSpeed * _climbSpeedMultiplier;
        var projected  = ProjectOnSurface(desired.normalized, _climbNormal);
        var velocity   = projected * climbSpeed;

        velocity += CalculateWallSnapVelocity();

        if (_moveInput.y > 0.1f && IsAtLedge())
        {
            velocity = CalculateLedgeClimbVelocity(climbSpeed);
        }

        return velocity;
    }

    /// <summary>
    /// Считает скорость прижатия к стене на основе реального зазора между капсулой и поверхностью.
    ///
    /// SphereCast с радиусом R возвращает hit.distance = расстояние, пройденное центром сферы до касания.
    /// Значит стена находится на расстоянии (hit.distance + R) от начала луча вдоль forward.
    /// Капсула персонажа имеет радиус _controller.radius, поэтому идеальное расстояние до стены — это тоже _controller.radius.
    /// Реальный зазор между поверхностью капсулы и стеной:
    ///   gap = (hit.distance + castRadius) - _controller.radius
    ///       = hit.distance - (_controller.radius - castRadius)
    ///       = hit.distance - _controller.radius * 0.5f   (т.к. castRadius = _controller.radius * 0.5f)
    /// Чтобы закрыть этот зазор за один кадр, нужна скорость = gap / deltaTime.
    /// </summary>
    private Vector3 CalculateWallSnapVelocity()
    {
        var castRadius = _controller.radius * 0.5f;
        var rayOrigin  = _transform.position + Vector3.up * _climbRayOffset;

        if (!Physics.SphereCast(rayOrigin, castRadius, _transform.forward,
                out var hit, _climbCheckDistance, _climbMask)) 
            return Vector3.zero;

        var gap = hit.distance - (_controller.radius - castRadius);

        if (gap <= WallSnapGap)
            return Vector3.zero;

        var snapSpeed = Mathf.Min((gap - WallSnapGap) / Time.deltaTime, 20f);
        return _transform.forward * snapSpeed;
    }

    private bool IsAtLedge()
    {
        var upper = _transform.position + Vector3.up * _climbRayOffset;
        var lower = _transform.position + Vector3.up * (_climbRayOffset * 0.5f);

        var noUpperHit  = !Physics.Raycast(upper, _transform.forward, _climbCheckDistance, _climbMask);  
        var hasLowerHit =  Physics.Raycast(lower, _transform.forward, _climbCheckDistance, _climbMask); 

        return noUpperHit && hasLowerHit;
    }

    private Vector3 CalculateLedgeClimbVelocity(float climbSpeed)
    {
        return climbSpeed * LedgeClimbMultiplier       * _transform.up +
               climbSpeed * LedgeForwardPushMultiplier * _transform.forward;
    }

    private Vector3 CalculateNormalVelocity()
    {
        var forward = _moveInput.y * _transform.forward;
        var strafe  = _moveInput.x * _transform.right;
        return _smoothedTargetSpeed * (forward + strafe).normalized;
    }

    private Vector3 CalculateSlopeVelocity()
    {
        var forward = _transform.forward * _moveInput.y;
        var strafe  = _transform.right * _moveInput.x;
        var dir     = (forward + strafe).normalized;

        var slopeDir = Vector3.ProjectOnPlane(dir, _slopeHit.normal).normalized;
        var vel      = _smoothedTargetSpeed * slopeDir;

        if (_isGrounded) vel.y -= _slopeForce;

        return vel;
    }

    #endregion

    #region Jump Handling

    private void HandleJump(bool jumpPressed)
    {
        if (!jumpPressed)
        {
            _jumpRequested = false;
            return;
        }

        if (_isOnClimbableSurface || _isClimbing)
        {
            JumpFromWall();
        }
        else if ((_isGrounded || _coyoteTimeCounter > 0f) && !_jumpRequested)
        {
            _coyoteTimeCounter = 0f; // сжигаем окно сразу при прыжке
            JumpFromGround();
        }
    }

    private void JumpFromWall()
    {
        _isOnClimbableSurface = false;
        _isClimbing = false;
        _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        _velocity  += _smoothedTargetSpeed * 0.5f * -_transform.forward;
        _isJumping  = true;
        _jumpRequested = true;
    }

    private void JumpFromGround()
    {
        _velocity.y    = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        _isJumping     = true;
        _jumpRequested = true;
    }

    private void UpdateJumpState()
    {
        if (_isClimbing || _isOnClimbableSurface)
        {
            _isJumping = false;
            return;
        }

        if (_isJumping && _velocity.y < -JumpVelocityThreshold)
        {
            _isJumping = false;
        }

        if (_isGrounded && _velocity.y <= 0f)
        {
            _isJumping     = false;
            _jumpRequested = false;
        }
    }

    private void ApplyGravity()
    {
        if (_isClimbing) return;

        if (_isOnClimbableSurface)
        {
            _velocity.y = 0f;
        }
        else
        {
            var mult = (_velocity.y < 0f && !_isJumping) ? FallGravityMultiplier : 1f;
            _velocity.y += _gravity * mult * Time.deltaTime;

            if (_velocity.y < TerminalVelocity)
                _velocity.y = TerminalVelocity;
        }
    }

    #endregion

    #region Rotation Handling

    private void UpdateWallRotation()
    {
        _wallRotationProgress += Time.deltaTime * WallRotationSpeed;
        _transform.rotation = Quaternion.Slerp(_startRotation, _targetWallRotation, _wallRotationProgress);

        if (_wallRotationProgress >= 1f)
        {
            _isRotatingToWall = false;
        }
    }

    private void AlignToSurface()
    {
        var targetForward = -_climbNormal;
        var targetUp      = CalculateSurfaceUp();

        var targetRot = Quaternion.LookRotation(targetForward, targetUp);
        _transform.rotation = Quaternion.Slerp(
            _transform.rotation,
            targetRot,
            _rotationSpeed * ClimbingRotationMultiplier * Time.deltaTime
        );
    }

    private void AlignToVertical()
    {
        var fwd = _transform.forward;
        fwd.y = 0f;

        if (fwd.sqrMagnitude < 0.01f)
            fwd = Vector3.ProjectOnPlane(_transform.forward, Vector3.up);

        fwd.Normalize();

        var targetRot = Quaternion.LookRotation(fwd, Vector3.up);

        _transform.rotation = Quaternion.Slerp(
            _transform.rotation,
            targetRot,
            _rotationSpeed * 3f * Time.deltaTime
        );
    }

    private Vector3 CalculateSurfaceUp()
    {
        var up = Vector3.ProjectOnPlane(Vector3.up, _climbNormal).normalized;

        if (up.sqrMagnitude < 0.01f)
            up = Vector3.ProjectOnPlane(Vector3.forward, _climbNormal).normalized;

        return up;
    }

    private void ApplyNormalRotation()
    {
        var targetRot = Quaternion.Euler(0f, _rotationInput.y, 0f);

        var angleDiff = Quaternion.Angle(_transform.rotation, targetRot);

        if (angleDiff < RotationSnapThreshold)
        {
            _transform.rotation = targetRot;
            return;
        }

        _transform.rotation = Quaternion.Slerp(
            _transform.rotation,
            targetRot,
            Time.deltaTime * _rotationSpeed
        );
    }

    private void InitiateWallRotation(Vector3 surfaceNormal)
    {
        var fwd = -surfaceNormal;
        fwd.y = 0f;

        if (fwd.sqrMagnitude < 0.01f) return;

        fwd.Normalize();
        _targetWallRotation   = Quaternion.LookRotation(fwd);
        _startRotation        = _transform.rotation;
        _wallRotationProgress = 0f;
        _isRotatingToWall     = true;
    }

    #endregion

    #region Slope Detection

    private bool OnSlope()
    {
        var origin = _transform.position + Vector3.up * 0.1f;
        var dist   = _controller.height / 2f + 0.5f;

        if (!Physics.Raycast(origin, Vector3.down, out _slopeHit, dist, _groundMask))
            return false;

        var angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
        return angle > 0.1f && angle <= _controller.slopeLimit;
    }

    #endregion

    #region Utility

    private Vector3 ProjectOnSurface(Vector3 direction, Vector3 surfaceNormal)
    {
        return direction - Vector3.Dot(direction, surfaceNormal) * surfaceNormal;
    }

    #endregion
}