using UnityEngine;

[CreateAssetMenu(fileName = "HasLumberAxe", menuName = "Scriptable Objects/InteractConditions/HasLumberAxe")]
public class HasLumberAxe : InteractCondition
{
    public override bool Check(CharacterCore character, Interactable interactable)
    {
        return character.HasLumberAxe;
    }
}
