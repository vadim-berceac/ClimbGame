using UnityEngine;

public class InputHandler
{
    private readonly IInputSource _playerSource;
    private readonly IInputSource _aiSource;
    public Vector2 MoveInput => _currentInputSource?.OnMove ?? Vector2.zero;
    public Vector2 LookInput => _currentInputSource?.OnLook ?? Vector2.zero;
    public Vector3 Rotation => _currentInputSource?.Rotation ?? Vector3.zero;
    public bool JumpPressed => _currentInputSource?.OnJump ?? false;
    public bool RunPressed => _currentInputSource?.OnRun ?? false;
    public bool CrouchPressed => _currentInputSource?.OnCrouch ?? false;
    public bool InteractPressed => _currentInputSource?.OnInteract ?? false;
    
    private IInputSource _currentInputSource;
    
    public InputHandler(IInputSource playerSource, IInputSource aiSource)
    {
        _playerSource = playerSource;
        _aiSource = aiSource;
    }

    public void SetupInput(InputSourceMode mode)
    {
        switch (mode)
        {
            case InputSourceMode.None:
                _currentInputSource = null;
                break;
            
            case InputSourceMode.AI:
                _currentInputSource = _aiSource;
                break;
            
            case InputSourceMode.Player:
                _currentInputSource = _playerSource;
                break;
            
            case InputSourceMode.Vehicle:
                _currentInputSource = null;
                break;
        }
    }
}

public enum InputSourceMode
{
    None = 0,
    AI = 1,
    Player = 2,
    Vehicle = 3
}
