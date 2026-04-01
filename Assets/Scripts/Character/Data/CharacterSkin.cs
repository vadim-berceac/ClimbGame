using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSkin", menuName = "Scriptable Objects/Skins/CharacterSkin")]
public class CharacterSkin : ScriptableObject
{
    [field: Header("Character Skin")]
    [field: SerializeField] public string SkinName { get; private set; }
    [field: SerializeField] public GameObject CharacterSkinPrefab { get; private set; }
    [field: SerializeField] public Avatar Avatar { get; private set; }
    [field: SerializeField] public SkinItemSlots SkinItemSlots { get; private set; }
    [field: SerializeField] public Vector3 SkinSize { get; private set; }
    [field: SerializeField] public Vector3 SkinPositionOffset { get; private set; }
    
    [field: Header("Character FootSteps")]
    [field: SerializeField] public GameObject StepVfxPrefab { get; private set; }
    [field: SerializeField] public StepSetup LFoot { get; private set; }
    [field: SerializeField] public StepSetup RFoot { get; private set; }
    
    [field: Header("NamePlate Settings")]
    [field: SerializeField] public GameObject NamePlatePrefab { get; private set; }
    [field: SerializeField] public Vector3 NamePlateOffset { get; private set; }
}

[Serializable]
public struct StepSetup
{
    [field: SerializeField] public HumanBodyBones Bone { get; private set; }
    [field: SerializeField] public Vector3 PositionOffset { get; private set; }
}
