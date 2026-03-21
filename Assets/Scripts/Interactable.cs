using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public interface IInteractable
{
    public AnimationClip InteractClip { get; set; }
    public FrameEventConfig InteractEvent { get; set; }
    public bool AllowMultipleInteractions { get; set; }
}

public class Interactable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public AnimationClip InteractClip { get; set; }
    [field: SerializeField] public FrameEventConfigField InteractEventField { get; set; } = new();
    [field: SerializeField] public bool AllowMultipleInteractions { get; set; } = true;
    
    public FrameEventConfig InteractEvent { get; set; }
    
    public CharacterCore OccupyingCharacter { get; private set; }

    private readonly HashSet<CharacterCore> _charactersInZone = new();
    private readonly Dictionary<CharacterCore, bool> _interactedDict = new();

    private void Start()
    {
        InteractEvent = InteractEventField.ToFrameEventConfig();
    }

    public void ResetInteraction()
    {
        _interactedDict.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CharacterCore character))
        {
            _charactersInZone.Add(character);
            _interactedDict[character] = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CharacterCore character) && _charactersInZone.Remove(character))
        {
            _interactedDict.Remove(character);

            if (OccupyingCharacter == character)
            {
                OccupyingCharacter = null;
            }
        }
    }

    private void Update()
    {
        RemoveDeadReferences();

        foreach (var character in _charactersInZone)
        {
            ProcessCharacterInteraction(character);
        }
    }

    #region Interaction Logic

    private void ProcessCharacterInteraction(CharacterCore character)
    {
        if (IsBlockedByOccupancy(character))
            return;

        if (!AllowMultipleInteractions && HasInteracted(character))
            return;

        if (CanInteract(character))
        {
            Interact(character);
            return;
        }
        
        if (HasInteracted(character) && !character.IsInteracting)
        {
            ResetCharacterInteraction(character);
        }
    }

    private void Interact(CharacterCore character)
    {
        if (OccupyingCharacter == null)
        {
            OccupyingCharacter = character;
        }

        _interactedDict[character] = true;
        character.PlayInteractAnimation(InteractClip, InteractEvent);
    }

    #endregion

    #region State Checks

    private bool IsBlockedByOccupancy(CharacterCore character)
        => !AllowMultipleInteractions 
            && OccupyingCharacter != null 
            && OccupyingCharacter != character;

    private bool CanInteract(CharacterCore character)
        => !character.IsInteracting && character.InputHandler.InteractPressed;

    
    private bool HasInteracted(CharacterCore character)
        => _interactedDict.TryGetValue(character, out var interacted) && interacted;

    #endregion

    #region Cleanup

    private void RemoveDeadReferences()
    {
        var deadCharacters = _charactersInZone.Where(c => c == null).ToList();
        foreach (var dead in deadCharacters)
        {
            _charactersInZone.Remove(dead);
            _interactedDict.Remove(dead);
        }
    }
  
    private void ResetCharacterInteraction(CharacterCore character)
    {
        _interactedDict[character] = false;

        if (OccupyingCharacter == character)
        {
            OccupyingCharacter = null;
        }
    }

    #endregion
}

[System.Serializable]
public class FrameEventConfigField
{
    [SerializeField] public int Begin;
    [SerializeField] public int End;
    [SerializeField] public UnityEvent OnEnter;
    [SerializeField] public UnityEvent OnExit;
    [SerializeField] public UnityEvent OnTick;
    [SerializeField] public float WeightThreshold = 0.5f;

    public FrameEventConfig ToFrameEventConfig() => new FrameEventConfig(
        Begin, 
        End,
        () => OnEnter?.Invoke(), 
        () => OnExit?.Invoke(),
        () => OnTick?.Invoke(),
        WeightThreshold
    );
}