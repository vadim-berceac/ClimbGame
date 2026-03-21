using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public interface IInteractable
{
    public AnimationClip EnterClip { get; set; }
    public AnimationClip ExitClip { get; set; }
    public FrameEventConfig EnterInteractEvent { get; set; }
    public bool AllowMultipleInteractions { get; set; }
}

public class Interactable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public AnimationClip EnterClip { get; set; }
    [field: SerializeField] public AnimationClip ExitClip { get; set; }
    [field: SerializeField] public FrameEventConfigField InteractEnterEventField { get; set; } = new();
    [field: SerializeField] public FrameEventConfigField InteractExitEventField { get; set; } = new();
    [field: SerializeField] public bool AllowMultipleInteractions { get; set; } = true;
    
    public FrameEventConfig EnterInteractEvent { get; set; }
    public FrameEventConfig ExitInteractEvent { get; set; }
    
    public CharacterCore OccupyingCharacter { get; private set; }

    private readonly HashSet<CharacterCore> _charactersInZone = new();
    private readonly Dictionary<CharacterCore, bool> _interactedDict = new();
    private readonly Dictionary<CharacterCore, bool> _isExitingDict = new();

    private void Start()
    {
        EnterInteractEvent = InteractEnterEventField.ToFrameEventConfig();
        ExitInteractEvent = InteractExitEventField.ToFrameEventConfig();
    }

    public void ResetInteraction()
    {
        _interactedDict.Clear();
        _isExitingDict.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CharacterCore character))
        {
            _charactersInZone.Add(character);
            _interactedDict[character] = false;
            _isExitingDict[character] = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CharacterCore character) && _charactersInZone.Remove(character))
        {
            _interactedDict.Remove(character);
            _isExitingDict.Remove(character);

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
            if (!HasInteracted(character))
            {
                // Первый раз - проигрываем EnterClip
                Interact(character);
            }
            else if (HasInteracted(character) && !IsExiting(character) && !character.IsInteracting)
            {
                // Повторное нажатие после EnterClip - проигрываем ExitClip
                ExitInteract(character);
            }
            return;
        }
        
        // Очищаем состояние только после завершения ExitClip
        if (IsExiting(character) && !character.IsInteracting)
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
        _isExitingDict[character] = false;
        character.PlayInteractAnimation(EnterClip, EnterInteractEvent);
    }

    private void ExitInteract(CharacterCore character)
    {
        _isExitingDict[character] = true;
        character.PlayInteractAnimation(ExitClip, ExitInteractEvent);
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

    private bool IsExiting(CharacterCore character)
        => _isExitingDict.TryGetValue(character, out var isExiting) && isExiting;

    #endregion

    #region Cleanup

    private void RemoveDeadReferences()
    {
        var deadCharacters = _charactersInZone.Where(c => c == null).ToList();
        foreach (var dead in deadCharacters)
        {
            _charactersInZone.Remove(dead);
            _interactedDict.Remove(dead);
            _isExitingDict.Remove(dead);
        }
    }
  
    private void ResetCharacterInteraction(CharacterCore character)
    {
        _interactedDict[character] = false;
        _isExitingDict[character] = false;

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