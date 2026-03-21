using System;
using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    public AnimationClip InteractClip { get; set; }
    public FrameEventConfig InteractEvent { get; set; }
}

public class Interactable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public AnimationClip InteractClip { get; set; }
    [field: SerializeField] public FrameEventConfigField InteractEventField { get; set; } = new();
    
    public FrameEventConfig InteractEvent { get; set; }
    
    private CharacterCore _characterCore;

    private void Start()
    {
        InteractEvent = InteractEventField.ToFrameEventConfig();
    }

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
            _characterCore.PlayInteractAnimation(InteractClip, InteractEvent);
        }
    }
}

[System.Serializable]
public class FrameEventConfigField
{
    [SerializeField] public int Begin;
    [SerializeField] public int End;
    [SerializeField] public UnityEvent OnEnter;
    [SerializeField] public UnityEvent OnExit;
    [SerializeField] public UnityEvent OnTick;
    [SerializeField] public float  WeightThreshold = 0.5f;

    public FrameEventConfig ToFrameEventConfig() => new FrameEventConfig(
        Begin, 
        End,
        () => OnEnter?.Invoke(), 
        () => OnExit?.Invoke(),
        () => OnTick?.Invoke(),
        WeightThreshold
    );
}