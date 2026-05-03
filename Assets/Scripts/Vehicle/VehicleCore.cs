using UnityEngine;
using Zenject;

public class VehicleCore : CoreController
{
    [SerializeField] private LocomotionConfigs locomotionConfig;
    [SerializeField] private AdvancedCharacterControllerData  controllerData;

    private CharacterAnimationContainer _animationContainer;
    private CharacterSoundContainer    _soundContainer;
    private MoveSpeed                  _moveSpeed;
    private CharacterAnimationEvents   _animationEvents;
    
    private bool _isInteracting;
    private InputHandler _driverInput;

    [Inject]
    private void Construct(
        PlayerInputSO                       playerInput,
        AIInput                           aiInput,
        CharacterController               controller,
        Animator                          animator,
        CharacterAnimationContainer       animationContainer,
        CharacterSoundContainer           soundContainer,
        AudioSource                       audioSource,
        CharacterAnimationEvents          animationEvents)
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

        SetLocomotion(true);
    }

    private void Update()
    {
        SetLocomotion();

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

    public override void SetLocomotion(bool isInitialization = false)
    {
        PlayablesAnimatorController.SetLocomotion(locomotionConfig.Locomotion);
        PlayablesAnimatorController.ConnectFootSteps(_soundContainer.GetAudioSet(locomotionConfig.Locomotion));
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayablesAnimatorController.Destroy();
    }
}
