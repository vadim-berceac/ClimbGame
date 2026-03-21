using UnityEngine;

public interface IInteractable
{
    public AnimationClip InteractClip { get; set; }
    public bool CanMove { get; set; }
}

public class Interactable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public AnimationClip InteractClip { get; set; }
    [field: SerializeField] public bool CanMove { get; set; } = true;
    
    private CharacterCore _characterCore;

    private void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent(out _characterCore);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out _characterCore))
        {
            _characterCore = null;
        }
    }

    private void Update()
    {
        if (_characterCore != null && 
            !_characterCore.IsInteracting && 
            _characterCore.InputHandler.InteractPressed)
        {
            _characterCore.PlayInteractAnimation(InteractClip, CanMove);
        }
    }
}
