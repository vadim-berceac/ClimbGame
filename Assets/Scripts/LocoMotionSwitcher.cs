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
                PlayEnterAnimation(character);
                break;

            case InteractionState.Idle:
                break;

            case InteractionState.Exiting:
                Execute();
                PlayExitAnimation(character);
                break;

            case InteractionState.None:
                break;
        }

        _lastProcessedState = newState;
    }

    private void PlayEnterAnimation(CharacterCore character)
    {
        var enterEventField = interactLocomotionConfig.LocomotionConfigs.EnterEventField;
        
        if (enterEventField == null || enterEventField.Clip == null)
            return;

        var eventConfig = enterEventField.ToFrameEventConfig();
        
        var originalOnEnter = eventConfig.OnEnter;
        eventConfig.OnEnter = () => {
            originalOnEnter?.Invoke();
            character.Interact(true, interactLocomotionConfig.LocomotionConfigs.Locomotion);
        };
        
        character.PlayInteractAnimation(enterEventField.Clip, eventConfig);
    }

    private void PlayExitAnimation(CharacterCore character)
    {
        var exitEventField = interactLocomotionConfig.LocomotionConfigs.ExitEventField;
        
        if (exitEventField == null || exitEventField.Clip == null)
            return;

        var eventConfig = exitEventField.ToFrameEventConfig();
        
        var originalOnEnter = eventConfig.OnEnter;
        eventConfig.OnEnter = () => {
            character.Interact(false, defaultLocomotionConfig.LocomotionConfigs.Locomotion);
            originalOnEnter?.Invoke();
        };
        
        character.PlayInteractAnimation(exitEventField.Clip, eventConfig);
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