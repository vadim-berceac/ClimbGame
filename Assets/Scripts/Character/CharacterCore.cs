using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [SerializeField] private InputSourceMode mode;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private AdvancedCharacterControllerData controllerData;
    [SerializeField] private JumpConfigs jumpConfigs;
    [SerializeField] private AudioSet footSteps;
    
    public AdvancedCharacterController Controller { get; private set; }
    public PlayablesAnimatorController PlayablesAnimatorController { get; private set; }
    private AnimationContainer _animationContainer;
    private MoveSpeed _moveSpeed;
    
    public InputHandler InputHandler { get; private set; }
    
    private LocomotionType _currentLocomotion;
    private bool _isInteracting;
    
    [Inject]
    private void Construct(PlayerInput playerInput, AIInput aiInput, CharacterController controller,
        Animator animator, AnimationContainer animationContainer, AudioSource audioSource)
    {
        InputHandler = new InputHandler(playerInput, aiInput);
        InputHandler.SetupInput(mode);
        
        Controller = new AdvancedCharacterController(controller, controllerData);
        _animationContainer = animationContainer;
        PlayablesAnimatorController = 
            new PlayablesAnimatorController(this, animator, audioSource, _animationContainer.LocomotionConfigs);
        _currentLocomotion = _animationContainer.DefaultLocomotion;
        PlayablesAnimatorController.SetLocomotion(_currentLocomotion);
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
        var moveSpeed = _moveSpeed.GetSpeed(_animationContainer.GetMoveSpeedData(_currentLocomotion));
        
        Controller.Move(InputHandler.MoveInput, moveSpeed, 1f);
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