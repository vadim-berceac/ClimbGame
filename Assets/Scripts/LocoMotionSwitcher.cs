using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class LocoMotionSwitcher : MonoBehaviour, IInteractableAction
{
    [SerializeField] private LocomotionType sittingLocomotion;
    
    private Interactable _interactable;
    
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();
    }

    public void Execute()
    {
        if (_interactable.OccupyingCharacter.CurrentLocomotionType != sittingLocomotion)
        {
            _interactable.OccupyingCharacter.Interact(true, sittingLocomotion);
            return;
        }
        
        _interactable.OccupyingCharacter.Interact(false, LocomotionType.Walk0);
        _interactable.ResetInteraction(_interactable.OccupyingCharacter); 
    }
}
