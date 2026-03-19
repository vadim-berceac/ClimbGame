using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/Skins/CharacterData")]
public class CharacterData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    
    [field: SerializeField] public EquippedItem CurrentWeapon { get; private set; }
    
    [field: SerializeField] public CharacterSkin Skin { get; private set; }
}
