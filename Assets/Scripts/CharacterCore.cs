using System;
using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [SerializeField] private InputSourceMode mode;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private AdvancedCharacterControllerData controllerData;
    [SerializeField] private AnimationPlayablesConfigs animationConfigs;
    
    public AdvancedCharacterController Controller { get; set; }
    private PlayablesAnimatorController _animatorController;
    
    public InputHandler InputHandler { get; set; }
    
    private bool _isInteracting;
    
    [Inject]
    private void Construct(PlayerInput playerInput, AIInput aiInput, CharacterController controller, Animator animator)
    {
        InputHandler = new InputHandler(playerInput, aiInput);
        InputHandler.SetupInput(mode);
        
        Controller = new AdvancedCharacterController(controller, controllerData);
        _animatorController = new PlayablesAnimatorController(this, animator, animationConfigs);
    }

    public void PlayInteractAnimation(AnimationClip animationClip)
    {
        if(_isInteracting) {return;}
        _isInteracting = true;
        _animatorController.PlayOneShotAnimationClip(animationClip);
        
    }

    private void OnValidate()
    {
        InputHandler?.SetupInput(mode);
    }

    private void Update()
    {
        Controller.Move(InputHandler.MoveInput, walkSpeed);
        Controller.JumpAndGravity(InputHandler.JumpPressed, jumpHeight);
        Controller.Rotation(InputHandler.Rotation, rotationSpeed);
        
        _animatorController.UpdateLocomotion(Controller.CurrentSpeed);

        if (Controller.IsJumping())
        {
            _animatorController.PlayOneShotAnimationClip(animationConfigs.Jump0);
        }
    }

    private void OnDestroy()
    {
        _animatorController.Destroy();
    }
}