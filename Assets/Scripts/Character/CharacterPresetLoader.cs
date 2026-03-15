using UnityEngine;
using Zenject;

public class CharacterPresetLoader : MonoBehaviour
{
    [SerializeField] private SkinItemSlots skinItemSlots;

    [Inject]
    private void Construct(Animator animator, CharacterSlots characterSlots)
    {
        InitializeSkinItemSlots(animator, characterSlots);
    }

    private void InitializeSkinItemSlots(Animator animator, CharacterSlots characterSlots)
    {
        if (skinItemSlots == null || animator == null) return;

        foreach (var data in skinItemSlots.ItemSlots)
        {
            var slot = new GameObject(data.SlotType.ToString());
            slot.transform.SetParent(animator.GetBoneTransform(data.ParentBone));
            slot.transform.localPosition = data.TransformData.Position;
            slot.transform.localRotation = data.TransformData.Rotation;
            slot.transform.localScale = new Vector3(data.TransformData.Scale, data.TransformData.Scale, data.TransformData.Scale);
            slot.SetActive(data.TransformData.Active);
            
            characterSlots.AddSlot(data.SlotType, slot.transform);
        }
    }
}
