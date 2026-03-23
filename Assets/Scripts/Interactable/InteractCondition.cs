
using UnityEngine;

public interface IInteractCondition
{
    public bool Check(CharacterCore character);
}

public abstract class InteractCondition : ScriptableObject, IInteractCondition
{
    public abstract bool Check(CharacterCore character);
}
