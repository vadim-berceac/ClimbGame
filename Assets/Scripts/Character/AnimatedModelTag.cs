using UnityEngine;
using Zenject;

public class AnimatedModelTag : MonoBehaviour
{
   public Transform ModelTagTransform { get; set; }

   [Inject]
   private void Construct()
   {
      ModelTagTransform = transform;
   }
}
