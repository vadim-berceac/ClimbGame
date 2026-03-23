using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public interface IInteractable
{
    public FrameEventConfig EnterInteractEvent { get; set; }
    public bool AllowMultipleInteractions { get; set; }
}

public class Interactable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public RotationSettings Rotation { get; set; }
    [field: SerializeField] public MoveSettings Move { get; set; }
    private enum InteractionState
    {
        None,       
        Entering,   
        Idle,      
        Exiting    
    }

    [field: SerializeField] public FrameEventConfigField InteractEnterEventField { get; set; } = new();
    [field: SerializeField] public FrameEventConfigField InteractExitEventField { get; set; } = new();
    [field: SerializeField] public bool AllowMultipleInteractions { get; set; } = true;
    public FrameEventConfig EnterInteractEvent { get; set; }
    public FrameEventConfig ExitInteractEvent { get; set; }
    
    public CharacterCore OccupyingCharacter { get; private set; }

    private readonly HashSet<CharacterCore> _charactersInZone = new();
    private readonly Dictionary<CharacterCore, InteractionState> _stateDict = new();
    private readonly HashSet<CharacterCore> _justExitedDict = new();
    private readonly Dictionary<CharacterCore, InteractionState> _lastAnimationStateDict = new(); 

    private void Start()
    {
        EnterInteractEvent = InteractEnterEventField.ToFrameEventConfig();
        ExitInteractEvent = InteractExitEventField.ToFrameEventConfig();
    }

    public void ResetInteraction()
    {
        _stateDict.Clear();
        _justExitedDict.Clear();
        _lastAnimationStateDict.Clear();
    }

    public void ResetInteraction(CharacterCore character)
    {
        if (character != null)
        {
            _stateDict.Remove(character);
            _justExitedDict.Remove(character);
            _lastAnimationStateDict.Remove(character);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CharacterCore character))
        {
            _charactersInZone.Add(character);
            _stateDict[character] = InteractionState.None;
            _lastAnimationStateDict[character] = InteractionState.None;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CharacterCore character) && _charactersInZone.Remove(character))
        {
            _stateDict.Remove(character);
            _justExitedDict.Remove(character);
            _lastAnimationStateDict.Remove(character);

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

        _justExitedDict.Clear();
    }

    #region Interaction Logic

    private void ProcessCharacterInteraction(CharacterCore character)
    {
        if (IsBlockedByOccupancy(character))
            return;

        var currentState = GetState(character);

        if (!character.IsInteracting)
        {
            if (currentState == InteractionState.Entering)
            {
                SetState(character, InteractionState.Idle);
            }
            else if (currentState == InteractionState.Exiting)
            {
                SetState(character, InteractionState.None);
                _justExitedDict.Add(character);
            }
        }

        currentState = GetState(character);

        if (CanInteract(character))
        {
            if (currentState == InteractionState.None && !_justExitedDict.Contains(character))
            {
                Interact(character);
            }
            else if (currentState == InteractionState.Idle)
            {
                ExitInteract(character);
            }
        }

        SyncAnimationWithState(character);
    }

    private void Interact(CharacterCore character)
    {
        OccupyingCharacter = character;
        SetState(character, InteractionState.Entering);
        MoveTo(character);
        Rotate(character);
    }

    private void Rotate(CharacterCore character)
    {
        if(!Rotation.RotateToCenter) return;
    
        var worldTargetPoint = transform.TransformPoint(Rotation.RotateToCenterPoint);
        var direction = worldTargetPoint - character.transform.position;
    
        var yAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        yAngle += Rotation.YOffset; 
    
        character.transform.DORotate(new Vector3(0, yAngle, 0), Rotation.RotateTime);
    }
    
    private void MoveTo(CharacterCore character)
    {
        if (!Move.NeedToMove) return;
    
        var worldTargetPoint = transform.TransformPoint(Move.MovePoint);
        character.transform.DOMove(worldTargetPoint, Move.MoveTime);
    }
    
    private void ExitInteract(CharacterCore character)
    {
        OccupyingCharacter = character;
        SetState(character, InteractionState.Exiting);
    }

    private void SyncAnimationWithState(CharacterCore character)
    {
        var currentState = GetState(character);
        var lastState = _lastAnimationStateDict.TryGetValue(character, out var state) ? state : InteractionState.None;

        if (currentState != lastState)
        {
            bool isValidTransition = IsValidStateTransition(lastState, currentState);
            
            if (!isValidTransition)
            {
                SetState(character, lastState);
                return;
            }

            if (currentState == InteractionState.Entering)
            {
                if (InteractEnterEventField.Clip != null)
                {
                    character.PlayInteractAnimation(InteractEnterEventField.Clip, EnterInteractEvent);
                }
            }
            else if (currentState == InteractionState.Exiting)
            {
                if (InteractExitEventField.Clip != null)
                {
                    character.PlayInteractAnimation(InteractExitEventField.Clip, ExitInteractEvent);
                }
            }

            _lastAnimationStateDict[character] = currentState;
        }
    }

    private bool IsValidStateTransition(InteractionState from, InteractionState to)
    {
        return (from, to) switch
        {
            (InteractionState.None, InteractionState.Entering) => true,
            (InteractionState.Entering, InteractionState.Idle) => true,
            (InteractionState.Idle, InteractionState.Exiting) => true,
            (InteractionState.Exiting, InteractionState.None) => true,
            _ => false
        };
    }

    #endregion

    #region State Management

    private InteractionState GetState(CharacterCore character)
    {
        return _stateDict.TryGetValue(character, out var state) ? state : InteractionState.None;
    }

    private void SetState(CharacterCore character, InteractionState state)
    {
        _stateDict[character] = state;
    }

    private bool IsBlockedByOccupancy(CharacterCore character)
        => !AllowMultipleInteractions 
            && OccupyingCharacter != null 
            && OccupyingCharacter != character;

    private bool CanInteract(CharacterCore character)
        => !character.IsInteracting && character.InputHandler.InteractPressed;

    #endregion

    #region Cleanup

    private void RemoveDeadReferences()
    {
        var deadCharacters = _charactersInZone.Where(c => c == null).ToList();
        foreach (var dead in deadCharacters)
        {
            _charactersInZone.Remove(dead);
            _stateDict.Remove(dead);
            _justExitedDict.Remove(dead);
            _lastAnimationStateDict.Remove(dead);
        }
    }

    #endregion
}

[System.Serializable]
public class FrameEventConfigField
{
    [SerializeField] public AnimationClip Clip;
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

[System.Serializable]
public struct RotationSettings
{
    [field: SerializeField] public bool RotateToCenter { get; set; }
    [field: SerializeField] public Vector3 RotateToCenterPoint { get; set; }
    [field: SerializeField] public float YOffset { get; set; } 
    [field: SerializeField] public float RotateTime { get; set; }
}

[System.Serializable]
public struct MoveSettings
{
    [field: SerializeField] public bool NeedToMove { get; set; }
    [field: SerializeField] public Vector3 MovePoint { get; set; }
    [field: SerializeField] public float MoveTime { get; set; }
}