using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSkin", menuName = "Scriptable Objects/Skins/CharacterSkin")]
public class CharacterSkin : ScriptableObject
{
    [field: SerializeField] public string SkinName { get; private set; }
    [field: SerializeField] public GameObject CharacterSkinPrefab { get; private set; }
    [field: SerializeField] public Avatar Avatar { get; private set; }
    [field: SerializeField] public SkinItemSlots SkinItemSlots { get; private set; }
    [field: SerializeField] public Vector3 SkinSize { get; private set; }
    [field: SerializeField] public Vector3 SkinPositionOffset { get; private set; }
}
