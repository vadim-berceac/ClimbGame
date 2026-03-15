using UnityEngine;
using Zenject;

public class TestInventory : MonoBehaviour
{
    [SerializeField] private EquippedItem equippedItem;

    [Inject]
    private void Construct(CharacterSlots slots)
    {
        if (equippedItem == null) return;
        
        Equip(equippedItem.EquippedItemPrefab, equippedItem.EquippedItemSlot, slots);
        Equip(equippedItem.EquippedItemPrefab, equippedItem.ActiveItemSlot, slots);
    }

    private void Equip(GameObject prefab, SlotSettings slotSettings, CharacterSlots slots)
    {
        var item = Instantiate(prefab);
        var scale = slotSettings.TransformData.Scale;
        item.transform.localScale = new Vector3(scale, scale, scale);
        item.SetActive(slotSettings.TransformData.Active);
        
        var slotTransform = slots.GetSlot(slotSettings.SlotType);
        item.transform.SetParent(slotTransform, false);
        item.transform.localPosition = slotSettings.TransformData.Position;
        item.transform.localRotation = slotSettings.TransformData.Rotation;
    }
}
