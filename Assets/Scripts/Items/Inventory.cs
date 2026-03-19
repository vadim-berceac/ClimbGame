using UnityEngine;

public class Inventory
{
    public void Equip(GameObject prefab, SlotSettings slotSettings, CharacterSlots slots)
    {
        var item = GameObject.Instantiate(prefab);
        
        var slotTransform = slots.GetSlot(slotSettings.SlotType);
        slotTransform.AttachSource(item.transform, slotSettings);
    }
}
