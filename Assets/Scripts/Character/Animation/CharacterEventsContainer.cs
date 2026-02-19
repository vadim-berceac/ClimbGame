using UnityEngine;
using Zenject;

public class CharacterEventsContainer : MonoBehaviour
{
   //Интегрировать в граф проигрывание партиклей?
   [SerializeField] private ParticleSystem stepL;
   [SerializeField] private ParticleSystem stepR;
   private PlayablesAnimatorController _playablesAnimatorController;

   [Inject]
   private void Construct(CharacterCore characterCore)
   {
      _playablesAnimatorController = characterCore.PlayablesAnimatorController;
   }
   
   public void PlayFootStepL()
   {
      _playablesAnimatorController.OnFootsteps();
      stepL.Play();
   }
   public void PlayFootStepR()
   {
      _playablesAnimatorController.OnFootsteps();
      stepR.Play();
   }
}
