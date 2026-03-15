using System.Collections.Generic;
using UnityEngine;

public class CharacterSlots
{
    private readonly Dictionary<ItemSlotType, Transform> _itemSlots = new();
    
    public IReadOnlyDictionary<ItemSlotType, Transform> ItemSlots => _itemSlots;

    public Transform GetSlot(ItemSlotType slotType)
    {
        if (_itemSlots.TryGetValue(slotType, out var slot))
            return slot;
        
        Debug.LogError($"Item slot not found: {slotType}");
        return null;
    }

    internal void AddSlot(ItemSlotType slotType, Transform slotTransform)
    {
        if (_itemSlots.ContainsKey(slotType))
        {
            Debug.LogWarning($"Slot {slotType} already exists, overwriting", slotTransform);
        }
        
        _itemSlots[slotType] = slotTransform;
    }
}
