
using UnityEngine;

[CreateAssetMenu(fileName = "LocomotionConfigsSO", menuName = "Scriptable Objects/Animation/LocomotionConfigsSO")]
public class LocomotionConfigsSO : ScriptableObject
{
    [field: SerializeField] public LocomotionConfigs LocomotionConfigs { get; set; }
    //[field: SerializeField] public CurveExtractor СurveExtractor { get; set; } = new();
}

// [System.Serializable]
// public class CurveExtractor
// {
//     [field: SerializeField] public AnimationClip Target { get; set; }
//     
//     [field: Header("Extracting results")]
//     
//     [field: SerializeField] public AnimationCurve IdleSpeedCurve { get; set; }
//     [field: SerializeField] public AnimationCurve ForwardSpeedCurve { get; set; }
//     [field: SerializeField] public AnimationCurve BackwardSpeedCurve { get; set; }
//     [field: SerializeField] public AnimationCurve StrafeLeftSpeedCurve { get; set; }
//     [field: SerializeField] public AnimationCurve StrafeRightSpeedCurve { get; set; }
//     [field: SerializeField] public AnimationCurve YSpeedCurve { get; set; }
//
//     public void OnValidate()
//     {
//         //извлечь все кривые движения клипа в соответствующие поля
//         //если кривой для поля нет - выставлять новую кривую с value 0
//         //можно добавить недостающие поля - если есть еще информация
//         //нужно чтобы кривые можно было копировать в обычный SO - как инструмент редактора не подойдет
//     }
// }