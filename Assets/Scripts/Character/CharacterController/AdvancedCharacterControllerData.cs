using UnityEngine;

[System.Serializable]
public struct AdvancedCharacterControllerData
{
    [field:Header("Movement")]
    [field:SerializeField] public float RotationSpeed {get; set;}
    [field:SerializeField] public float SpeedChangeRate {get; set;}
    
    [field:Header("Jump")]
    [field:SerializeField] public float GroundCheckDistance {get; set;}
    [field:SerializeField] public LayerMask GroundMask {get; set;}
    
    [field:Header("Slope Handling")]
    [field:SerializeField] public float SlopeForce {get; set;}
    
    [field:Header("Climbing")]
    [field:SerializeField] public bool EnableClimbing {get; set;}
    [field:SerializeField] public float ClimbSpeedMultipler {get; set;}
    [field:SerializeField] public float ClimbCheckDistance {get; set;}
    [field:SerializeField] public float MaxClimbAngle {get; set;}
    [field:SerializeField] public float ClimbRayOffset {get; set;}
    [field:SerializeField] public LayerMask ClimbMask {get; set;}
}
