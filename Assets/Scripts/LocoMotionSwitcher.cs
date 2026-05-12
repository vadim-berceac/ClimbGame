using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class LocoMotionSwitcher : MonoBehaviour, IInteractableAction
{
    [SerializeField] private LocomotionConfigsSO interactLocomotionConfig;
    [SerializeField] private LocomotionConfigsSO defaultLocomotionConfig;
    
    private Interactable _interactable;
    
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();
    }

    public void Execute()
    {
        if (_interactable.OccupyingCharacter.CurrentLocomotionType != interactLocomotionConfig.LocomotionConfigs.Locomotion)
        {
            _interactable.OccupyingCharacter.Interact(true, interactLocomotionConfig.LocomotionConfigs.Locomotion);
            return;
        }
        
        _interactable.OccupyingCharacter.Interact(false, defaultLocomotionConfig.LocomotionConfigs.Locomotion);
        _interactable.ResetInteraction(_interactable.OccupyingCharacter); 
    }
}
