using UnityEngine;
using Zenject;

public class SoundEvents : MonoBehaviour
{
   private PlayablesAnimatorController _playablesAnimatorController;

   [Inject]
   private void Construct(CharacterCore characterCore)
   {
      _playablesAnimatorController = characterCore.PlayablesAnimatorController;
   }
   
   
   public void PlayFootStep()
   {
      _playablesAnimatorController.OnFootsteps();
   }
}
