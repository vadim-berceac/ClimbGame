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

    private static void Equip(GameObject prefab, SlotSettings slotSettings, CharacterSlots slots)
    {
        var item = Instantiate(prefab);
        
        var slotTransform = slots.GetSlot(slotSettings.SlotType);
        slotTransform.AttachSource(item.transform, slotSettings);
    }
}
