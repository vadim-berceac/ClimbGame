
using UnityEngine.Serialization;
using Zenject;

public class AIInput : MonoInputSource
{
    public CoreController CharacterCore { get; private set; }
    private CharacterSelector _characterSelector;

    [Inject]
    private void Construct(CoreController characterCore, CharacterSelector characterSelector)
    {
        CharacterCore = characterCore;
        _characterSelector = characterSelector;
        
        _characterSelector.Connect(this);
    }

    public void EnablePlayerInput()
    {
        DisableAIInput();
        CharacterCore.InputHandler.SetupInput(InputSourceMode.Player);
    }

    public void DisablePlayerInput()
    {
        EnableAIInput();
        CharacterCore.InputHandler.SetupInput(InputSourceMode.AI);
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
