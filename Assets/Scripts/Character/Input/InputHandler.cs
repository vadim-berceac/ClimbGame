using UnityEngine;

public class InputHandler
{
    private readonly IInputSource _playerSource;
    private readonly IInputSource _aiSource;
    public Vector2 MoveInput => CurrentInputSource?.OnMove ?? Vector2.zero;
    public Vector2 LookInput => CurrentInputSource?.OnLook ?? Vector2.zero;
    public Vector3 Rotation => CurrentInputSource?.Rotation ?? Vector3.zero;
    public bool JumpPressed => CurrentInputSource?.OnJump ?? false;
    public bool RunPressed => CurrentInputSource?.OnRun ?? false;
    public bool CrouchPressed => CurrentInputSource?.OnCrouch ?? false;
    public bool InteractPressed => CurrentInputSource?.OnInteract ?? false;
    
    public IInputSource CurrentInputSource { get; private set; }
    public InputSourceMode CurrentInputSourceMode { get; private set; }
    
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
                CurrentInputSource = null;
                CurrentInputSourceMode = InputSourceMode.None;
                break;
            
            case InputSourceMode.AI:
                CurrentInputSource = _aiSource;
                CurrentInputSourceMode = InputSourceMode.AI;
                break;
            
            case InputSourceMode.Player:
                CurrentInputSource = _playerSource;
                CurrentInputSourceMode = InputSourceMode.Player;
                break;
            
            case InputSourceMode.Vehicle:
                CurrentInputSource = null;
                CurrentInputSourceMode = InputSourceMode.Vehicle;
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
