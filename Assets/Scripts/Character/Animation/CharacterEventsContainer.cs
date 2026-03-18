using UnityEngine;
using Zenject;

public class CharacterEventsContainer : MonoBehaviour
{
   private PlayablesAnimatorController _playablesAnimatorController;
   
   private ParticleSystem _stepL;
   private ParticleSystem _stepR;

   [Inject]
   private void Construct(
      CoreController characterCore)
   {
      _playablesAnimatorController = characterCore.PlayablesAnimatorController;
   }

   public void SetupFootSteps(ParticleSystem leftFootSteps, ParticleSystem rightFootSteps)
   {
      _stepL = leftFootSteps;
      _stepR = rightFootSteps;
   }
   
   public void PlayFootStepL()
   {
      _playablesAnimatorController.OnFootsteps();
      _stepL.Play();
   }
   public void PlayFootStepR()
   {
      _playablesAnimatorController.OnFootsteps();
      _stepR.Play();
   }
}
