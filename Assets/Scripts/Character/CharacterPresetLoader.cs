using UnityEngine;
using Zenject;

public class CharacterPresetLoader : MonoBehaviour
{
    [SerializeField] private CharacterSkin skinData;

    private GameObject _currentSkin;
    
    private Animator _animator;
    private CharacterSlots _characterSlots;
    private AnimatedModelTag _animatedModelTag;

    [Inject]
    private void Construct(Animator animator, CharacterSlots characterSlots, AnimatedModelTag animatedModelTag)
    {
        _animator = animator;
        _characterSlots = characterSlots;
        _animatedModelTag = animatedModelTag;

        InitializeModel(skinData);
    }

    private void InitializeModel(CharacterSkin data)
    {
        SetModel(data);
        SetSkinItemSlots(data);
        SetSize(data);
        SetName(data);
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

    private void SetSize(CharacterSkin data)
    {
        gameObject.transform.localScale = data.SkinSize;
    }

    private void SetName(CharacterSkin data)
    {
        gameObject.name = data.SkinName;
    }
}
