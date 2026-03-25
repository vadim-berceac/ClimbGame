using UnityEngine;

[CreateAssetMenu(fileName = "HasPick", menuName = "Scriptable Objects/InteractConditions/HasPick")]
public class HasPick : InteractCondition
{
    public override bool Check(CharacterCore character, Interactable interactable)
    {
        return character.HasPick;
    }
}
