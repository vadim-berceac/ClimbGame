using UnityEngine;
using Zenject;

public class CharacterPresetLoader : MonoBehaviour
{
    [SerializeField] private CharacterData characterData;

    private DiContainer _container;
    private GameObject _currentSkin;
    
    private Animator _animator;
    private CharacterSlots _characterSlots;
    private AnimatedModelTag _animatedModelTag;
    private CharacterEventsContainer _characterEventsContainer;
    private Inventory _inventory;
    
    [Inject]
    private IInstantiator _instantiator;

    [Inject]
    private void Construct(
        DiContainer diContainer,
        Animator animator,
        CharacterSlots characterSlots,
        AnimatedModelTag animatedModelTag,
        CharacterEventsContainer characterEventsContainer,
        EquipmentManager equipmentManager,
        Inventory inventory
        )
    {
        _container = diContainer;
        _animator = animator;
        _characterSlots = characterSlots;
        _animatedModelTag = animatedModelTag;
        _characterEventsContainer = characterEventsContainer;
        _inventory = inventory;

        InitializeModel(characterData);
        InitializeItems(characterData.CurrentWeapon);
    }

    private void InitializeModel(CharacterData data)
    {
        SetModel(data.Skin);
        SetSkinItemSlots(data.Skin);
        SetSize(data.Skin);
        SetFootStepsVFX(data.Skin);
        SetName(data);
        CreateNamePlate(data);
    }

    private void InitializeItems(EquippedItem item)
    {
        if (item == null) return;
        
        _inventory.AddInstance(item);
        _inventory.SetPrimaryWeapon(_inventory.GetItemInstance(item));
        _inventory.DrawWeapon();
    }
    
    private void SetModel(CharacterSkin data)
    {
        if (data.CharacterSkinPrefab == null || data.Avatar == null ) return;
        
        var modelTransform = _animatedModelTag.ModelTagTransform;
        
        if(_currentSkin != null) Destroy(_currentSkin);
        
        _currentSkin = Instantiate(data.CharacterSkinPrefab, modelTransform);
        
        _currentSkin.TryGetComponent<Animator>(out var skinAnimator);
        
        if (skinAnimator) Destroy(skinAnimator);
        
        _currentSkin.transform.localPosition = data.SkinPositionOffset;
        
        _animator.avatar = data.Avatar;
    }

    private void SetSkinItemSlots(CharacterSkin data)
    {
        if (data.SkinItemSlots == null || _animator == null) return;

        foreach (var slotData in data.SkinItemSlots.ItemSlots)
        {
            var slot = new GameObject(slotData.SlotType.ToString());
            slot.transform.SetParent(_animator.GetBoneTransform(slotData.ParentBone));
            slot.transform.localPosition = slotData.TransformData.Position;
            slot.transform.localRotation = slotData.TransformData.Rotation;
            slot.transform.localScale = Vector3.one * slotData.TransformData.Scale;
            slot.SetActive(slotData.TransformData.Active);
            
            _characterSlots.AddSlot(slotData.SlotType, slot.transform);
        }
    }
    
    private void SetFootStepsVFX(CharacterSkin data)
    {
        var stepL = Instantiate(data.StepVfxPrefab,
            _animator.GetBoneTransform(data.LFoot.Bone)).GetComponent<ParticleSystem>();
        var stepR = Instantiate(data.StepVfxPrefab,
            _animator.GetBoneTransform(data.RFoot.Bone)).GetComponent<ParticleSystem>();
        stepL.gameObject.transform.localPosition = data.LFoot.PositionOffset;
        stepR.gameObject.transform.localPosition = data.RFoot.PositionOffset;
        
        _characterEventsContainer.SetupFootSteps(stepL, stepR);
    }

    private void SetSize(CharacterSkin data)
    {
        gameObject.transform.localScale = data.SkinSize;
    }

    private void SetName(CharacterData data)
    {
        gameObject.name = data.Name;
    }

    private void CreateNamePlate(CharacterData data)
    {
        if(data.Skin.NamePlatePrefab == null) return;
    
        var namePlateObj = _container.InstantiatePrefab(data.Skin.NamePlatePrefab, transform);
        var namePlate = namePlateObj.GetComponent<Nameplate>();
    
        namePlate.SetName(data.Name);
        namePlate.SetOffset(data.Skin.NamePlateOffset);
    }
}
