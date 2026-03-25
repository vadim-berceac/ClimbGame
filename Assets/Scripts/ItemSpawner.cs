using UnityEngine;

public class ItemSpawner : MonoBehaviour, IInteractableAction
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private SlotSettings equippedItemSlot;
    
    private GameObject _itemInstance;
    private Interactable _interactable;
    
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();
    }
    
    public void Execute()
    {
        if (itemPrefab != null && _itemInstance == null)
        {
            _itemInstance = Instantiate(itemPrefab);
            var slotTransform = _interactable.OccupyingCharacter.CharacterSlots.GetSlot(equippedItemSlot.SlotType);
            slotTransform.AttachSource(_itemInstance.transform, equippedItemSlot);
            return;
        }

        if (_itemInstance)
        {
            Destroy(_itemInstance);
        }
    }
}
