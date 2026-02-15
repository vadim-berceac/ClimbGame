using UnityEngine;

public class InputHandler
{
    private readonly IInputSource _playerSource;
    private readonly IInputSource _aiSource;
    public Vector2 MoveInput => _currentInputSource?.OnMove ?? Vector2.zero;
    public bool JumpPressed => _currentInputSource?.OnJump ?? false;
    
    private IInputSource _currentInputSource;
    
    public InputHandler(IInputSource playerSource, InputSource aiSource)
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
        }
    }
}

public enum InputSourceMode
{
    None = 0,
    AI = 1,
    Player = 2
}
