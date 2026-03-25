using UnityEngine;

[CreateAssetMenu(fileName = "IsNotDestroyed", menuName = "Scriptable Objects/InteractConditions/IsNotDestroyed")]
public class IsNotDestroyed : InteractCondition
{
    public override bool Check(CharacterCore character, Interactable interactable)
    {
        return !interactable.Damageable?.IsDestroyed() ?? true;
    }
}
