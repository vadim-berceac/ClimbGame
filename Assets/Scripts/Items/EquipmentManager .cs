using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager
{
    // нужно переписать на работу с инстансами, а не датой предметов!
    private readonly Dictionary<ItemSlotType, EquippedSlot> _equippedItems = new();

    public void Equip(EquippedItem item, CharacterSlots slots)
    {
        var slotTransform = slots.GetSlot(item.EquippedItemSlot.SlotType);
        
        if (!slotTransform || !item.EquippedItemPrefab)
            return;

        UnEquip(item.EquippedItemSlot.SlotType);

        var instance = GameObject.Instantiate(item.EquippedItemPrefab);
        slotTransform.AttachSource(instance.transform, item.EquippedItemSlot);
        
        _equippedItems[item.EquippedItemSlot.SlotType] = new EquippedSlot
        {
            Instance = instance,
            Item = item
        };
    }

    public bool SetItemActive(EquippedItem item, CharacterSlots slots)
    {
        return TryMoveItem(item.EquippedItemSlot, item.ActiveItemSlot, slots);
    }

    public bool SetItemEquipped(EquippedItem item, CharacterSlots slots)
    {
        return TryMoveItem(item.ActiveItemSlot, item.EquippedItemSlot, slots);
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