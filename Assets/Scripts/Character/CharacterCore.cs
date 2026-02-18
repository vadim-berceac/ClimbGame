using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [SerializeField] private InputSourceMode mode;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private AdvancedCharacterControllerData controllerData;
    [SerializeField] private AudioSet footSteps;
    [SerializeField] private GameObject testObject;
    
    public AdvancedCharacterController Controller { get; private set; }
    public PlayablesAnimatorController PlayablesAnimatorController { get; private set; }
    private AnimationContainer _animationContainer;
    private MoveSpeed _moveSpeed;
    private LocomotionSelector _locomotionSelector;
    private LocomotionType _currentLocomotionType;
    
    public InputHandler InputHandler { get; private set; }
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
        _currentLocomotionType = _animationContainer.DefaultLocomotion;
        _locomotionSelector = new LocomotionSelector(Controller, InputHandler);
        PlayablesAnimatorController.SetLocomotion(_currentLocomotionType);
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
        var locomotionType = _locomotionSelector.GetLocomotionType();
    
        if (locomotionType != _currentLocomotionType)
        {
            _currentLocomotionType = locomotionType;
            PlayablesAnimatorController.SetLocomotion(_currentLocomotionType);
        }

        var moveSpeed = _moveSpeed.GetSpeed(_animationContainer.GetMoveSpeedData(_locomotionSelector.GetLocomotionType()));
        
        Controller.Move(InputHandler.MoveInput, moveSpeed, 1f);
        Controller.JumpAndGravity(InputHandler.JumpPressed, _animationContainer.JumpConfigs[0].JumpHeight);
        Controller.Rotation(InputHandler.Rotation, rotationSpeed);
        
        PlayablesAnimatorController.UpdateLocomotion(Controller.HorizontalVelocity.normalized);

        if (Controller.IsJumping())
        {
            //
            PlayablesAnimatorController.SetTestGameObject(testObject);
            //
            PlayablesAnimatorController.PlayOneShotAnimationClip(_animationContainer.JumpConfigs[0].JumpStart0);
        }
    }

    private void OnDestroy()
    {
        PlayablesAnimatorController.Destroy();
    }
}