
using Zenject;

public class AIInput : InputSource
{
    private CharacterCore _characterCore;
    private CharacterSelector _characterSelector;

    [Inject]
    private void Construct(CharacterCore characterCore, CharacterSelector characterSelector)
    {
        _characterCore = characterCore;
        _characterSelector = characterSelector;
        
        _characterSelector.Connect(this);
    }

    public void EnablePlayerInput()
    {
        DisableAIInput();
        _characterCore.InputHandler.SetupInput(InputSourceMode.Player);
    }

    public void DisablePlayerInput()
    {
        EnableAIInput();
        _characterCore.InputHandler.SetupInput(InputSourceMode.AI);
    }

    private void EnableAIInput()
    {
        
    }

    private void DisableAIInput()
    {
        
    }

    private void OnDisable()
    {
        if (_characterSelector == null)
        {
            return;
        }
        _characterSelector.Disconnect(this);
    }
}
