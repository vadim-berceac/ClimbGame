using UnityEngine;

public class AdvancedCharacterController
{
    #region Constants
    
    private const float JumpVelocityThreshold = 0.1f;
    private const float VerticalWallThreshold = 0.5f;
    private const float MinClimbAngle = 60f;
    private const float WallRotationSpeed = 8f;
    private const float ClimbingRotationMultiplier = 5f;
    private const float ForcedFallSpeed = -8f;
    private const int ForcedFallFrames = 10;
    private const float GroundSnapVelocity = -2f;
    private const float LedgeClimbMultiplier = 3.0f;
    private const float LedgeForwardPushMultiplier = 1.5f;
    private const float RotationSnapThreshold = 0.1f;
    private const float TerminalVelocity = -53f; 
    private const float FallGravityMultiplier = 2f; 
    
    #endregion

    #region Configuration (Readonly)
    
    private readonly CharacterController _controller;
    private readonly Transform _transform;
    private readonly LayerMask _groundMask;
    
    private readonly float _groundCheckDistance;
    private readonly float _slopeForce;
    private readonly float _climbCheckDistance;
    private readonly float _maxClimbAngle;
    private readonly float _climbRayOffset;
    private readonly float _climbSpeedMultiplier;
    private readonly float _speedChangeRate;
    private readonly bool _enableClimbing;
    
    #endregion

    #region State
    
    private Vector3 _moveInput;
    private Vector3 _velocity;
    private Vector3 _climbNormal;
    private Vector3 _rotationInput; 
    private RaycastHit _slopeHit;
    
    private float _moveSpeed;
    private float _currentSpeed;
    private float _actualSpeed; 
    private float _rotationSpeed;
    private float _jumpHeight;
    private float _gravity;
    
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
    
    public bool IsGrounded() => _isGrounded;
    public bool IsClimbing() => _isClimbing;
    public bool IsOnClimbableSurface() => _isOnClimbableSurface;
    public bool IsJumping() => _isJumping;
    public bool IsFalling() => !_isGrounded && !_isOnClimbableSurface && _velocity.y < -JumpVelocityThreshold;
    public bool IsAirborne() => !_isGrounded && !_isClimbing && !_isOnClimbableSurface;
    public Vector3 Velocity => _velocity;
    public float CurrentSpeed => _actualSpeed; 
    
    #endregion

    #region Initialization
    
    public AdvancedCharacterController(CharacterController characterController, AdvancedCharacterControllerData data)
    {
        _controller = characterController;
        _transform = characterController.transform;
        
        _groundCheckDistance = data.GroundCheckDistance;
        _groundMask = data.GroundMask;
        _slopeForce = data.SlopeForce;
        _enableClimbing = data.EnableClimbing;
        _climbCheckDistance = data.ClimbCheckDistance;
        _maxClimbAngle = data.MaxClimbAngle;
        _climbRayOffset = data.ClimbRayOffset;
        _climbSpeedMultiplier = data.ClimbSpeedMultipler;
        _speedChangeRate = 10f;
    }
    
    #endregion

    #region Public Methods
    
    /// <summary>
    /// Обрабатывает движение персонажа с плавным изменением скорости
    /// </summary>
    /// <param name="motion">Направление движения (X - влево/вправо, Y - вперёд/назад)</param>
    /// <param name="speed">Целевая скорость движения</param>
    /// <param name="speedChangeRate">Скорость изменения скорости (по умолчанию 10)</param>
    public void Move(Vector3 motion, float speed, float? speedChangeRate = null)
    {
        _moveInput = motion;
        _moveSpeed = speed;
        
        var actualSpeedChangeRate = speedChangeRate ?? _speedChangeRate;
        _currentSpeed = Mathf.Lerp(_currentSpeed, _moveSpeed, actualSpeedChangeRate * Time.deltaTime);
        
        CheckGround();
        
        if (_enableClimbing)
        {
            CheckClimbing();
        }
        
        HandleMovement();
        
        _controller.Move(_velocity * Time.deltaTime);
        
        // Вычисляем реальную скорость движения после применения velocity
        UpdateActualSpeed();
    }
    
    /// <summary>
    /// Обрабатывает прыжок и применение гравитации
    /// </summary>
    public void JumpAndGravity(bool jumpPressed, float jumpHeight, float gravityMultiplier = 1f)
    {
        _jumpHeight = jumpHeight;
        _gravity = gravityMultiplier * Physics.gravity.y;

        HandleJump(jumpPressed);
        UpdateJumpState();
        ApplyGravity();
    }
    
    /// <summary>
    /// Обрабатывает поворот персонажа
    /// </summary>
    /// <param name="rotation">Вектор вращения (обычно от камеры, используется rotation.y)</param>
    /// <param name="rotationSpeed">Скорость поворота</param>
    public void Rotation(Vector3 rotation, float rotationSpeed)
    {
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
            return;
        }
        
        var sphereCastOrigin = _transform.position;
        var sphereCastDistance = _controller.height / 2f + _groundCheckDistance;
        
        if (Physics.SphereCast(sphereCastOrigin, _controller.radius, Vector3.down, out var hit, sphereCastDistance, _groundMask))
        {
            var surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
            _isGrounded = surfaceAngle <= _controller.slopeLimit;
            
            if (_isGrounded && _velocity.y < 0 && _velocity.y > -10f)
            {
                _velocity.y = GroundSnapVelocity;
            }
        }
        else
        {
            _isGrounded = false;
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
        if (!_isOnClimbableSurface || _moveInput.y >= -0.1f)
            return false;
        
        var isVerticalWall = Mathf.Abs(_climbNormal.y) < VerticalWallThreshold;
        return isVerticalWall && _isGrounded;
    }
    
    private bool TryFindClimbableSurface(out RaycastHit hit)
    {
        var rayOrigin = _transform.position + Vector3.up * _climbRayOffset;
    
        if (!Physics.SphereCast(rayOrigin, _controller.radius * 0.5f, _transform.forward, 
                out hit, _climbCheckDistance, _groundMask))
        {
            return false;
        }
    
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
        }
        else if (_isOnClimbableSurface)
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
        }
        else if (_moveInput.magnitude > 0.1f)
        {
            var targetMove = OnSlope() ? CalculateSlopeVelocity() : CalculateNormalVelocity();
            _velocity.x = targetMove.x;
            _velocity.z = targetMove.z;
        }
        else
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
        }
    }
    
    private void ApplyForcedFallVelocity()
    {
        var backwardDirection = -_transform.forward;
        _velocity.x = backwardDirection.x * _currentSpeed * 0.8f;
        _velocity.z = backwardDirection.z * _currentSpeed * 0.8f;
        _velocity.y = ForcedFallSpeed;
    }
    
    private Vector3 CalculateClimbingVelocity()
    {
        var desiredDirection = Vector3.zero;
        
        if (Mathf.Abs(_moveInput.y) > 0.1f)
        {
            desiredDirection += _transform.up * _moveInput.y;
        }
        
        if (Mathf.Abs(_moveInput.x) > 0.1f)
        {
            desiredDirection += _transform.right * _moveInput.x;
        }
        
        var currentClimbSpeed = _currentSpeed * _climbSpeedMultiplier;
        var projectedDirection = ProjectOnSurface(desiredDirection.normalized, _climbNormal);
        var climbVelocity = projectedDirection * currentClimbSpeed;
        
        if (_moveInput.y > 0.1f && IsAtLedge())
        {
            climbVelocity = CalculateLedgeClimbVelocity(currentClimbSpeed);
        }
        
        return climbVelocity;
    }
    
    private bool IsAtLedge()
    {
        var upperRayOrigin = _transform.position + Vector3.up * _climbRayOffset;
        var lowerRayOrigin = _transform.position + Vector3.up * (_climbRayOffset * 0.5f);
        
        var noUpperHit = !Physics.Raycast(upperRayOrigin, _transform.forward, _climbCheckDistance, _groundMask);
        var hasLowerHit = Physics.Raycast(lowerRayOrigin, _transform.forward, _climbCheckDistance, _groundMask);
        
        return noUpperHit && hasLowerHit;
    }
    
    private Vector3 CalculateLedgeClimbVelocity(float climbSpeed)
    {
        return climbSpeed * LedgeClimbMultiplier * _transform.up  + 
               climbSpeed * LedgeForwardPushMultiplier * _transform.forward;
    }
    
    private Vector3 CalculateNormalVelocity()
    {
        var forwardMovement = _moveInput.y * _transform.forward;
        var strafeMovement = _moveInput.x * _transform.right;
        return _currentSpeed * (forwardMovement + strafeMovement);
    }
    
    private Vector3 CalculateSlopeVelocity()
    {
        var forwardMovement = _transform.forward * _moveInput.y;
        var strafeMovement = _transform.right * _moveInput.x;
        var moveDirection = forwardMovement + strafeMovement;
        
        var slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, _slopeHit.normal).normalized;
        var slopeVelocity = _currentSpeed * _moveInput.magnitude * slopeMoveDirection;
        
        if (_isGrounded)
        {
            slopeVelocity.y -= _slopeForce;
        }
        
        return slopeVelocity;
    }
    
    /// <summary>
    /// Вычисляет реальную скорость движения на основе горизонтальной составляющей velocity
    /// </summary>
    private void UpdateActualSpeed()
    {
        var horizontalVelocity = new Vector3(_velocity.x, 0f, _velocity.z);
        _actualSpeed = horizontalVelocity.magnitude;
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
        else if (_isGrounded && !_jumpRequested)
        {
            JumpFromGround();
        }
    }
    
    private void JumpFromWall()
    {
        _isOnClimbableSurface = false;
        _isClimbing = false;
        _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        _velocity += _moveSpeed * 0.5f * -_transform.forward;
        _isJumping = true;
        _jumpRequested = true;
    }
    
    private void JumpFromGround()
    {
        _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        _isJumping = true;
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
            _isJumping = false;
            _jumpRequested = false;
        }
    }
    
    private void ApplyGravity()
    {
        if (_isClimbing)
        {
            return;
        }
        
        if (_isOnClimbableSurface)
        {
            _velocity.y = 0f;
        }
        else
        {
            var gravityMultiplier = (_velocity.y < 0f && !_isJumping) ? FallGravityMultiplier : 1f;
            _velocity.y += _gravity * gravityMultiplier * Time.deltaTime;
           
            if (_velocity.y < TerminalVelocity)
            {
                _velocity.y = TerminalVelocity;
            }
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
        var targetUp = CalculateSurfaceUp();
    
        var targetRotation = Quaternion.LookRotation(targetForward, targetUp);
        _transform.rotation = Quaternion.Slerp(
            _transform.rotation,
            targetRotation,
            _rotationSpeed * ClimbingRotationMultiplier * Time.deltaTime
        );
    }
    
    /// <summary>
    /// Плавно выравнивает капсулу в вертикальное положение при обычном движении
    /// </summary>
    private void AlignToVertical()
    {
        var currentForward = _transform.forward;
        currentForward.y = 0f;
    
        if (currentForward.sqrMagnitude < 0.01f)
        {
            currentForward = Vector3.ProjectOnPlane(_transform.forward, Vector3.up);
        }
    
        currentForward.Normalize();
        
        var targetRotation = Quaternion.LookRotation(currentForward, Vector3.up);
    
        _transform.rotation = Quaternion.Slerp(
            _transform.rotation,
            targetRotation,
            _rotationSpeed * 3f * Time.deltaTime
        );
    }
    
    private Vector3 CalculateSurfaceUp()
    {
        var targetUp = Vector3.ProjectOnPlane(Vector3.up, _climbNormal).normalized;
    
        if (targetUp.sqrMagnitude < 0.01f)
        {
            targetUp = Vector3.ProjectOnPlane(Vector3.forward, _climbNormal).normalized;
        }
    
        return targetUp;
    }
    
    /// <summary>
    /// Применяет вращение персонажа по направлению камеры на земле
    /// </summary>
    private void ApplyNormalRotation()
    {
        var targetRotation = Quaternion.Euler(0f, _rotationInput.y, 0f);
        
        var angleDifference = Quaternion.Angle(_transform.rotation, targetRotation);
        
        if (angleDifference < RotationSnapThreshold)
        {
            _transform.rotation = targetRotation;
            return;
        }
       
        _transform.rotation = Quaternion.Slerp(
            _transform.rotation, 
            targetRotation, 
            Time.deltaTime * _rotationSpeed
        );
    }
    
    private void InitiateWallRotation(Vector3 surfaceNormal)
    {
        var targetForward = -surfaceNormal;
        targetForward.y = 0f;
    
        if (targetForward.sqrMagnitude < 0.01f)
        {
           return;
        }
        
        targetForward.Normalize();
        _targetWallRotation = Quaternion.LookRotation(targetForward);
        _startRotation = _transform.rotation;
        _wallRotationProgress = 0f;
        _isRotatingToWall = true;
    }
    
    #endregion

    #region Slope Detection
    
    private bool OnSlope()
    {
        var rayOrigin = _transform.position + Vector3.up * 0.1f;
        var rayDistance = _controller.height / 2f + 0.5f;
        
        if (!Physics.Raycast(rayOrigin, Vector3.down, out _slopeHit, rayDistance, _groundMask))
            return false;
        
        var angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
        return angle > 0.1f && angle <= _controller.slopeLimit;
    }
    
    #endregion

    #region Utility Methods
    
    /// <summary>
    /// Проецирует вектор направления на плоскость поверхности
    /// </summary>
    private Vector3 ProjectOnSurface(Vector3 direction, Vector3 surfaceNormal)
    {
        return direction - Vector3.Dot(direction, surfaceNormal) * surfaceNormal;
    }
    
    #endregion
}