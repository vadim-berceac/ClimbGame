using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [SerializeField] private InputSourceMode mode;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private AdvancedCharacterControllerData controllerData;
    [SerializeField] private LocomotionConfigs locomotionConfigs;
    [SerializeField] private MoveSpeedData moveSpeedData;
    [SerializeField] private JumpConfigs jumpConfigs;
    [SerializeField] private AudioSet footSteps;
    
    public AdvancedCharacterController Controller { get; private set; }
    public PlayablesAnimatorController PlayablesAnimatorController { get; private set; }
    private MoveSpeed _moveSpeed;
    
    public InputHandler InputHandler { get; private set; }
    
    private bool _isInteracting;
    
    [Inject]
    private void Construct(PlayerInput playerInput, AIInput aiInput, CharacterController controller,
        Animator animator, AudioSource audioSource)
    {
        InputHandler = new InputHandler(playerInput, aiInput);
        InputHandler.SetupInput(mode);
        
        Controller = new AdvancedCharacterController(controller, controllerData);
        PlayablesAnimatorController = new PlayablesAnimatorController(this, animator, audioSource);
        PlayablesAnimatorController.ConnectLocomotion(locomotionConfigs);
        PlayablesAnimatorController.ConnectFootSteps(footSteps);
        _moveSpeed = new MoveSpeed(InputHandler);
    }

    public void PlayInteractAnimation(AnimationClip animationClip)
    {
        if(_isInteracting) {return;}
        _isInteracting = true;
        PlayablesAnimatorController.PlayOneShotAnimationClip(animationClip);
    }

    private void OnValidate()
    {
        InputHandler?.SetupInput(mode);
    }

    private void Update()
    {
        Controller.Move(InputHandler.MoveInput, _moveSpeed.GetSpeed(moveSpeedData), 1f);
        Controller.JumpAndGravity(InputHandler.JumpPressed, jumpHeight);
        Controller.Rotation(InputHandler.Rotation, rotationSpeed);
        
        PlayablesAnimatorController.UpdateLocomotion(Controller.HorizontalVelocity.normalized);

        if (Controller.IsJumping())
        {
            PlayablesAnimatorController.PlayOneShotAnimationClip(jumpConfigs.Jump0);
        }
    }

    private void OnDestroy()
    {
        PlayablesAnimatorController.Destroy();
    }
}