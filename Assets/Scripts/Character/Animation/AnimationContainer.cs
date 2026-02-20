using System.Linq;
using UnityEngine;

public class AnimationContainer : MonoBehaviour
{
   [Header("Locomotion")] // и подкрадывание, и плаванье, и карабканье - все сюда
   [SerializeField] private LocomotionConfigs[] locomotionConfigs;
   [SerializeField] private LocomotionType defaultLocomotion;

   public LocomotionConfigs[] LocomotionConfigs => locomotionConfigs;
   public LocomotionType DefaultLocomotion => defaultLocomotion;
 
   public MoveSpeedData GetMoveSpeedData(LocomotionType locomotionType)
   {
      return locomotionConfigs.FirstOrDefault(l => l.Locomotion == locomotionType).MoveSpeedData;
   }
}
