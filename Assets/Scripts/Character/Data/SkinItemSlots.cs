using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinItemSlots", menuName = "Scriptable Objects/Skins/SkinItemSlots")]
public class SkinItemSlots : ScriptableObject
{
    [field: SerializeField] public ItemSlotData[] ItemSlots { get; private set; }
    
    private void OnValidate()
    {
        if (ItemSlots == null || ItemSlots.Length == 0)
            return;

        var usedSlotTypes = new HashSet<ItemSlotType>();
        var hasDuplicates = false;

        foreach (var itemSlot in ItemSlots)
        {
            if (!usedSlotTypes.Add(itemSlot.SlotType))
            {
                Debug.LogError($"Duplicate ItemSlotType detected: {itemSlot.SlotType} в {name}", this);
                hasDuplicates = true;
            }
        }

        if (hasDuplicates)
        {
            Debug.LogError($"{name} содержит дубликаты в ItemSlotType.", this);
        }
    }
}
