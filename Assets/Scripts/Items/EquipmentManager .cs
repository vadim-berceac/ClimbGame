using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager
{
    private readonly Dictionary<ItemSlotType, EquippedSlot> _equippedItems = new();

    public void Equip(ItemInstance item, CharacterSlots slots)
    {
        var equippedItem = item.GetData<EquippedItem>();
        
        if (equippedItem == null || !equippedItem.EquippedItemPrefab)
        {
            Debug.LogWarning("[EquipmentManager] Предмет не является экипируемым или не имеет префаба");
            return;
        }

        UnEquip(equippedItem.EquippedItemSlot.SlotType);

        var instance = GameObject.Instantiate(equippedItem.EquippedItemPrefab);
        var slotTransform = slots.GetSlot(equippedItem.EquippedItemSlot.SlotType);
        slotTransform.AttachSource(instance.transform, equippedItem.EquippedItemSlot);
        
        _equippedItems[equippedItem.EquippedItemSlot.SlotType] = new EquippedSlot
        {
            Instance = instance,
            Item = equippedItem
        };
    }

    public bool SetItemActive(ItemInstance item, CharacterSlots slots)
    {
        var equippedItem = item.GetData<EquippedItem>();
        if (equippedItem == null)
            return false;

        return TryMoveItem(equippedItem.EquippedItemSlot, equippedItem.ActiveItemSlot, slots);
    }

    public bool SetItemEquipped(ItemInstance item, CharacterSlots slots)
    {
        var equippedItem = item.GetData<EquippedItem>();
        if (equippedItem == null)
            return false;

        return TryMoveItem(equippedItem.ActiveItemSlot, equippedItem.EquippedItemSlot, slots);
    }

    private bool TryMoveItem(SlotSettings fromSlot, SlotSettings toSlot, CharacterSlots slots)
    {
        if (!_equippedItems.TryGetValue(fromSlot.SlotType, out var equippedSlot))
        {
            Debug.LogWarning($"[EquipmentManager] Нет предмета в слоте {fromSlot.SlotType}");
            return false;
        }

        var targetSlot = slots.GetSlot(toSlot.SlotType);
        if (!targetSlot)
        {
            Debug.LogWarning($"[EquipmentManager] Целевой слот {toSlot.SlotType} не найден");
            return false;
        }

        targetSlot.AttachSource(equippedSlot.Instance.transform, toSlot);

        _equippedItems.Remove(fromSlot.SlotType);
        _equippedItems[toSlot.SlotType] = equippedSlot;

        return true;
    }

    public bool UnEquip(ItemSlotType slotType)
    {
        if (!_equippedItems.TryGetValue(slotType, out var item))
            return false;

        GameObject.Destroy(item.Instance);
        _equippedItems.Remove(slotType);
        return true;
    }

    public bool TryGetEquipped(ItemSlotType slotType, out EquippedSlot item) 
        => _equippedItems.TryGetValue(slotType, out item);
}

public struct EquippedSlot
{
    public GameObject Instance { get; set; }
    public EquippedItem Item { get; set; }
}