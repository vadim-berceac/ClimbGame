using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class LocoMotionSwitcher : MonoBehaviour, IInteractableAction
{
    [SerializeField] private LocomotionType interactLocomotion;
    [SerializeField] private LocomotionType defaultLocomotion;
    
    private Interactable _interactable;
    
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();
    }

    public void Execute()
    {
        if (_interactable.OccupyingCharacter.CurrentLocomotionType != interactLocomotion)
        {
            _interactable.OccupyingCharacter.Interact(true, interactLocomotion);
            return;
        }
        
        _interactable.OccupyingCharacter.Interact(false, defaultLocomotion);
        _interactable.ResetInteraction(_interactable.OccupyingCharacter); 
    }
}
