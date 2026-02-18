using UnityEngine;

public interface IInteractable
{
    public AnimationClip InteractClip { get; set; }
}

public class Interactable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public AnimationClip InteractClip { get; set; }
    
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
        if (_characterCore)
        {
            //и произведено взаимодействие
            //передаем анимацию
        }
    }
}
