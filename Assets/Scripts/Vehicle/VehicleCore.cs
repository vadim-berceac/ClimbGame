using UnityEngine;
using Zenject;

public class VehicleCore : CoreController
{
    [SerializeField] private LocomotionConfigs locomotionConfig;
    [SerializeField] private AdvancedCharacterControllerData  controllerData;

    private AnimationContainer         _animationContainer;
    private SoundContainer             _soundContainer;
    private MoveSpeed                  _moveSpeed;
    private CharacterAnimationEvents   _animationEvents;
    
    private bool _isInteracting;
    private InputHandler _driverInput;

    [Inject]
    private void Construct(
        PlayerInput              playerInput,
        AIInput                  aiInput,
        CharacterController      controller,
        Animator                 animator,
        AnimationContainer       animationContainer,
        SoundContainer           soundContainer,
        AudioSource              audioSource,
        CharacterAnimationEvents animationEvents)
    {
        InputHandler = new InputHandler(playerInput, aiInput);
        InputHandler.SetupInput(InputSourceMode.Vehicle);

        Controller          = new AdvancedCharacterController(controller, controllerData);
        _animationContainer = animationContainer;
        _soundContainer     = soundContainer;
        _animationEvents    = animationEvents;
        _moveSpeed          = new MoveSpeed(InputHandler);

        PlayablesAnimatorController =
            new PlayablesAnimatorController(this, animator, audioSource, _animationContainer.LocomotionConfigs);
        
        PlayablesAnimatorController.SetEventTagResolver(_animationEvents.Resolve);

        _driverInput = InputHandler;

        UpdateLocomotion(true);
    }

    private void Update()
    {
        UpdateLocomotion();

        var clampedInput = _moveSpeed.GetClampedInput(locomotionConfig.MoveSpeedData);
        var moveSpeed = _moveSpeed.GetSpeed(locomotionConfig.MoveSpeedData);

        Controller.Move(clampedInput, moveSpeed, 50f);
        Controller.JumpAndGravity(_driverInput.JumpPressed, locomotionConfig.MoveSpeedData.YSpeed);
        Controller.Rotation(_driverInput.Rotation, controllerData.RotationSpeed);

        PlayablesAnimatorController.UpdateLocomotion(Controller.HorizontalVelocity.normalized);
    }

    public void SetDriverInput(InputHandler inputHandler)
    {
        _driverInput = inputHandler;
        _moveSpeed = new MoveSpeed(inputHandler);
    }

    public void ResetDriver()
    {
        _driverInput = InputHandler;
        _moveSpeed = new MoveSpeed(InputHandler);
    }

    public override void UpdateLocomotion(bool isInitialization = false)
    {
        PlayablesAnimatorController.SetLocomotion(locomotionConfig.Locomotion);
        PlayablesAnimatorController.ConnectFootSteps(_soundContainer.GetAudioSet(locomotionConfig.Locomotion));
    }

    private void OnDestroy()
    {
        PlayablesAnimatorController.Destroy();
    }
}
