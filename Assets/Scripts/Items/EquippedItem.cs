using UnityEngine;

[CreateAssetMenu(fileName = "EquippedItem", menuName = "Scriptable Objects/EquippedItem")]
public class EquippedItem : SimpleItem
{
    [field: SerializeField] public GameObject EquippedItemPrefab { get; set; }
    [field: SerializeField] public SlotSettings EquippedItemSlot { get; set; }
    [field: SerializeField] public SlotSettings ActiveItemSlot { get; set; }
}
