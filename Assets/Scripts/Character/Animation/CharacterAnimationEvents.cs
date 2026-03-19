using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterAnimationEvents : MonoBehaviour
{
    [Serializable]
    public struct TaggedEvent
    {
        [field: SerializeField] public string     Tag     { get; set; }
        [field: SerializeField] public UnityEvent OnEnter { get; set; }
        [field: SerializeField] public UnityEvent OnExit  { get; set; }
    }

    [SerializeField] private TaggedEvent[] _events;

    public (Action onEnter, Action onExit, Action onTick) Resolve(string tag)
    {
        foreach (var e in _events)
        {
            if (e.Tag != tag) continue;
            return (
                onEnter: () => e.OnEnter?.Invoke(),
                onExit:  () => e.OnExit?.Invoke(),
                onTick:  null);
        }
        return (null, null, null);
    }
}