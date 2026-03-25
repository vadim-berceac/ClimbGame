
using UnityEngine;

public interface IInteractCondition
{
    public bool Check(CharacterCore character, Interactable interactable);
}

public abstract class InteractCondition : ScriptableObject, IInteractCondition
{
    public abstract bool Check(CharacterCore character, Interactable interactable);
}
