using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class SitChair : MonoBehaviour
{
    private Interactable _interactable;
    
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();
    }

    public void Sit()
    {
        if (_interactable.OccupyingCharacter.CurrentLocomotionType != LocomotionType.Sit0)
        {
            _interactable.OccupyingCharacter.Sit(true);
            return;
        }
        
        _interactable.OccupyingCharacter.Sit(false);
        _interactable.ResetInteraction();
    }
}
