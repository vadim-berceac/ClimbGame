using UnityEngine;

public class AdvancedCharacterController
{
    private readonly CharacterController _controller;
    private readonly float _groundCheckDistance;
    private readonly LayerMask _groundMask;
    private readonly float _slopeForce;
    private readonly bool _enableClimbing;
    private readonly float _climbCheckDistance;
    private readonly float _maxClimbAngle;
    private readonly float _climbRayOffset;
    private readonly float _climbSpeedMultiplier;
    private readonly Transform _transform;
    private const float JumpVelocityThreshold = 0.1f;
    
    private Vector3 _moveInput;
    private float _moveSpeed;
    private float _rotationSpeed;
    private float _jumpHeight;
    private float _gravity;
    private float _climbSpeed;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _isClimbing;
    private bool _isJumping;
    private bool _jumpRequested;
    private bool _isOnClimbableSurface;
    private Vector3 _climbNormal;
    private RaycastHit _slopeHit;
    
    private bool _isForcedFalling; 
    private int _forcedFallingFrames;
    private bool _isRotatingToWall;
    private Quaternion _targetWallRotation;
    private Quaternion _startRotation;
    private float _wallRotationProgress;
    
    public bool IsGrounded() => _isGrounded;
    public bool IsClimbing() => _isClimbing;
    public bool IsOnClimbableSurface() => _isOnClimbableSurface;
    public bool IsJumping() => _isJumping;
    public bool IsFalling() => !_isGrounded && !_isOnClimbableSurface && _velocity.y < -JumpVelocityThreshold;
    public bool IsAirborne() => !_isGrounded && !_isClimbing && !_isOnClimbableSurface; 
    public Vector3 Velocity => _velocity;
    
    
    public AdvancedCharacterController(CharacterController characterController,
        AdvancedCharacterControllerData data)
    {
        _controller = characterController;
        _groundCheckDistance = data.GroundCheckDistance;
        _groundMask = data.GroundMask;
        _slopeForce = data.SlopeForce;
        _enableClimbing = data.EnableClimbing;
        _climbCheckDistance = data.ClimbCheckDistance;
        _maxClimbAngle = data.MaxClimbAngle;
        _climbRayOffset = data.ClimbRayOffset;
        _climbSpeedMultiplier = data.ClimbSpeedMultipler;
        
        _transform = characterController.transform;
    }
    
    public void Move(Vector3 motion, float speed)
    {
        _moveInput = motion;
        _moveSpeed = speed;
        _climbSpeed = speed * _climbSpeedMultiplier;
        
        CheckGround();
        
        if (_enableClimbing)
        {
            CheckClimbing();
        }
        
        HandleMovement();
        
        _controller.Move(_velocity * Time.fixedDeltaTime);
    }
    
    public void JumpAndGravity(bool jumpPressed, float jumpHeight, float gravityMultiplier = 1f)
    {
        _jumpHeight = jumpHeight;
        _gravity = gravityMultiplier * Physics.gravity.y;

        if (jumpPressed)
        {
            if (_isOnClimbableSurface || _isClimbing)
            {
                _isOnClimbableSurface = false;
                _isClimbing = false;
                _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                _velocity +=  _moveSpeed * 0.5f * -_transform.forward;
                _isJumping = true;
                _jumpRequested = true;
            }
            else if (_isGrounded && !_jumpRequested)
            {
                _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                _isJumping = true;
                _jumpRequested = true;
            }
        }
        else
        {
            _jumpRequested = false;
        }

        if (_isClimbing || _isOnClimbableSurface)
        {
            _isJumping = false;
        }
    
        if (_isJumping)
        {
            if (_velocity.y < -JumpVelocityThreshold)
            {
                _isJumping = false;
            }
        }

        if (_isGrounded && _velocity.y <= 0f)
        {
            _isJumping = false;
            _jumpRequested = false;
        }
  
        if (!_isClimbing && !_isOnClimbableSurface)
        {
            _velocity.y += _gravity * Time.fixedDeltaTime;
        }
        else if (!_isClimbing)
        {
            _velocity.y = 0f;
        }
    }
    
    public void Rotation(float rotationSpeed)
    {
        _rotationSpeed = rotationSpeed;
    
        if (_isRotatingToWall)
        {
            _wallRotationProgress += Time.fixedDeltaTime * 8f;
            _transform.rotation = Quaternion.Slerp(_startRotation, _targetWallRotation, _wallRotationProgress);
        
            if (_wallRotationProgress >= 1f)
            {
                _isRotatingToWall = false;
            }
            return;
        }
    
        if (_isClimbing || _isOnClimbableSurface) 
        {
            var targetForward = -_climbNormal;
        
            var targetUp = Vector3.ProjectOnPlane(Vector3.up, _climbNormal).normalized;
        
            if (targetUp.sqrMagnitude < 0.01f)
            {
                targetUp = Vector3.ProjectOnPlane(Vector3.forward, _climbNormal).normalized;
            }
        
            var targetRotation = Quaternion.LookRotation(targetForward, targetUp);
        
            _transform.rotation = Quaternion.Slerp(
                _transform.rotation,
                targetRotation,
                _rotationSpeed * 5f * Time.fixedDeltaTime
            );
        
            return;
        }
    
        if (Mathf.Abs(_moveInput.x) > 0.1f)
        {
            _transform.Rotate(0f, _moveInput.x * _rotationSpeed * Time.fixedDeltaTime, 0f);
        }
    }
    
    private void CheckGround()
    {
        if (_forcedFallingFrames > 0)
        {
            _forcedFallingFrames--;
            _isGrounded = false;
            return;
        }
    
        if (Physics.SphereCast(_transform.position, _controller.radius, Vector3.down, out var hit, 
                _controller.height / 2f + _groundCheckDistance, _groundMask))
        {
            var angle = Vector3.Angle(Vector3.up, hit.normal);
            _isGrounded = angle <= _controller.slopeLimit;
        
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
        }
        else
        {
            _isGrounded = false;
        }
    }
    
    private void CheckClimbing()
{
    if (_forcedFallingFrames > 0)
    {
        return;
    }

    if (_isJumping)
    {
        _isOnClimbableSurface = false;
        _isClimbing = false;
        _isRotatingToWall = false;
        return;
    }

    if (_isOnClimbableSurface && _moveInput.y < -0.1f)
    {
        var isVerticalWall = Mathf.Abs(_climbNormal.y) < 0.5f;
    
        if (isVerticalWall && _isGrounded)
        {
            _isOnClimbableSurface = false;
            _isClimbing = false;
            return;
        }
    }
   
    var rayOrigin = _transform.position + Vector3.up * _climbRayOffset;

    if (Physics.Raycast(rayOrigin, _transform.forward, out var hit, _climbCheckDistance, _groundMask))
    {
        var angle = Vector3.Angle(Vector3.up, hit.normal);
    
        // КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: карабканье только на крутых поверхностях (> 60 градусов)
        // Наклоны 45-60 градусов обрабатываются как склоны
        if (angle > 60f && angle <= _maxClimbAngle + 90f)
        {
            var hitBelowCharacter = hit.point.y < _transform.position.y;
        
            if (_isGrounded && hitBelowCharacter && _moveInput.y < -0.1f)
            {
                var backCheckPos = _transform.position - _transform.forward * (_controller.radius * 0.5f);
            
                if (!Physics.Raycast(backCheckPos, Vector3.down, _controller.height, _groundMask))
                {
                    _isOnClimbableSurface = false;
                    _isClimbing = false;
                    _isGrounded = false;
                    _forcedFallingFrames = 10;
                    return;
                }
            }
            
            if (!_isOnClimbableSurface)
            {
                Vector3 targetForward = -hit.normal;
                targetForward.y = 0f;
                
                if (targetForward.sqrMagnitude > 0.01f)
                {
                    targetForward.Normalize();
                    _targetWallRotation = Quaternion.LookRotation(targetForward);
                    _startRotation = _transform.rotation;
                    _wallRotationProgress = 0f;
                    _isRotatingToWall = true;
                }
            }
        
            _isOnClimbableSurface = true;
            _climbNormal = hit.normal;
            _isClimbing = _moveInput.magnitude > 0.1f;
            return;
        }
    }

    _isOnClimbableSurface = false;
    _isClimbing = false;
}
    
    private void HandleMovement()
    {
        if (_forcedFallingFrames > 0)
        {
            _velocity.x = -_transform.forward.x * _moveSpeed * 0.8f;
            _velocity.z = -_transform.forward.z * _moveSpeed * 0.8f;
            _velocity.y = -8f;
            return;
        }
    
        if (_isClimbing)
        {
            _velocity = HandleClimbingMovement();
        }
        else if (_isOnClimbableSurface)
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
        }
        else if (Mathf.Abs(_moveInput.y) > 0.1f)
        {
            var targetMove = OnSlope() ? HandleSlopeMovement() : _moveInput.y * _moveSpeed * _transform.forward;
            _velocity.x = targetMove.x;
            _velocity.z = targetMove.z;
        }
        else
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
        }
    }
    
    private Vector3 HandleClimbingMovement()
    {
        var climbMove = Vector3.zero;
    
        // Определяем насколько поверхность наклонная (Y компонент нормали)
        float surfaceTilt = Mathf.Abs(_climbNormal.y);
        bool isSlopedSurface = surfaceTilt > 0.2f; // Наклонная если Y > 0.2
    
        if (Mathf.Abs(_moveInput.y) > 0.1f)
        {
            var rayOrigin = _transform.position + Vector3.up * _climbRayOffset;
            var lowerCheck = _transform.position + Vector3.up * (_climbRayOffset * 0.5f);
        
            var atLedge = !Physics.Raycast(rayOrigin, _transform.forward, _climbCheckDistance, _groundMask) &&
                          Physics.Raycast(lowerCheck, _transform.forward, _climbCheckDistance, _groundMask) &&
                          _moveInput.y > 0.1f;
        
            if (atLedge)
            {
                climbMove.y = _climbSpeed * _moveInput.y * 3.0f;
                var forwardPush =  _climbSpeed * 1.5f * _transform.forward;
                climbMove.x = forwardPush.x;
                climbMove.z = forwardPush.z;
            }
            else
            {
                climbMove.y = _climbSpeed * _moveInput.y;
            
                // КЛЮЧЕВОЕ: для наклонных поверхностей при движении вниз - больше отталкиваемся
                float pushMultiplier = 0.3f;
                if (isSlopedSurface && _moveInput.y < -0.1f)
                {
                    // При спуске по наклону - сильнее отталкиваемся от поверхности
                    pushMultiplier = 1.2f;
                }
            
                var stickyMove = _climbSpeed * pushMultiplier * -_climbNormal;
                climbMove.x = stickyMove.x;
                climbMove.z = stickyMove.z;
            }
        }
    
        if (Mathf.Abs(_moveInput.x) > 0.1f && climbMove.y < _climbSpeed * 2f)
        {
            var rightMove = _climbSpeed * _moveInput.x * _transform.right;
            climbMove.x += rightMove.x;
            climbMove.z += rightMove.z;
        }
    
        return climbMove;
    }
    
    private bool OnSlope()
    {
        var rayOrigin = _transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out _slopeHit, _controller.height / 2f + 0.5f, _groundMask))
        {
            var angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle > 0.1f && angle <= _controller.slopeLimit;
        }
        
        return false;
    }
    
    private Vector3 HandleSlopeMovement()
    {
        var moveDirection = _transform.forward * _moveInput.y;
        var slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, _slopeHit.normal).normalized;
        var slopeMove = _moveSpeed * Mathf.Abs(_moveInput.y) * slopeMoveDirection;
    
        if (_isGrounded)
        {
            slopeMove.y -= _slopeForce;
        }
    
        return slopeMove;
    }
}
