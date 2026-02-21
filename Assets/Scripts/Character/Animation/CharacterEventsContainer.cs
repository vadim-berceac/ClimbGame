using UnityEngine;
using Zenject;

public class CharacterEventsContainer : MonoBehaviour
{
   [SerializeField] private GameObject stepVfxPrefab;
   private PlayablesAnimatorController _playablesAnimatorController;
   private Animator _animator;
   
   private ParticleSystem _stepL;
   private ParticleSystem _stepR;

   [Inject]
   private void Construct(CoreController characterCore, Animator animator)
   {
      _playablesAnimatorController = characterCore.PlayablesAnimatorController;
      _animator = animator;
      
      CreateFootStepsVFX();
   }

   private void CreateFootStepsVFX()
   {
      _stepL = Instantiate(stepVfxPrefab, _animator.GetBoneTransform(HumanBodyBones.LeftFoot)).GetComponent<ParticleSystem>();
      _stepR = Instantiate(stepVfxPrefab, _animator.GetBoneTransform(HumanBodyBones.RightFoot)).GetComponent<ParticleSystem>();
      _stepL.gameObject.transform.localPosition = new Vector3(-0.076f, 0f, -0.0480000004f);
      _stepR.gameObject.transform.localPosition = new Vector3(0.076f, 0f, 0.0480000004f);
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
