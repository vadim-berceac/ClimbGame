using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class LocoMotionSwitcher : MonoBehaviour, IInteractableAction
{
    [SerializeField] private LocomotionConfigsSO interactLocomotionConfig;
    [SerializeField] private LocomotionConfigsSO defaultLocomotionConfig;
    
    [Header("Item Spawning (Optional)")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private SlotSettings equippedItemSlot;
    
    private Interactable _interactable;
    private InteractionState _lastProcessedState = InteractionState.None;
    private GameObject _itemInstance;
    
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();
    }

    private void OnEnable()
    {
        if (_interactable != null)
        {
            _interactable.OnStateTransitioned += HandleStateTransition;
        }
    }

    private void OnDisable()
    {
        if (_interactable != null)
        {
            _interactable.OnStateTransitioned -= HandleStateTransition;
        }
    }

    private void HandleStateTransition(CharacterCore character, InteractionState newState)
    {
        if (character == null)
            return;

        switch (newState)
        {
            case InteractionState.Entering:
                Execute();
                character.SwitchLocomotion(interactLocomotionConfig.LocomotionConfigs, true);
                break;

            case InteractionState.Idle:
                break;

            case InteractionState.Exiting:
                Execute();
                character.SwitchLocomotion(defaultLocomotionConfig.LocomotionConfigs, false);
                break;

            case InteractionState.None:
                break;
        }

        _lastProcessedState = newState;
    }

    public void Execute()
    {
        if (itemPrefab == null || _interactable.OccupyingCharacter == null)
            return;

        if (_itemInstance == null)
        {
            _itemInstance = Instantiate(itemPrefab);
            var slotTransform = _interactable.OccupyingCharacter.CharacterSlots.GetSlot(equippedItemSlot.SlotType);
            slotTransform.AttachSource(_itemInstance.transform, equippedItemSlot);
            return;
        }

        if (_itemInstance)
        {
            Destroy(_itemInstance);
            _itemInstance = null;
        }
    }
}