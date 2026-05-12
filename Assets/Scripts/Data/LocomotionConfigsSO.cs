using UnityEngine;

[CreateAssetMenu(fileName = "LocomotionConfigsSO", menuName = "Scriptable Objects/Animation/LocomotionConfigsSO")]
public class LocomotionConfigsSO : ScriptableObject
{
    [field: SerializeField] public LocomotionConfigs LocomotionConfigs { get; set; }
}
