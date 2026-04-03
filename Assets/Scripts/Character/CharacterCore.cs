using UnityEngine;
using Zenject;

public class CharacterCore : CoreController
{
    [field:SerializeField] public bool HasPick {get; private set;} // для теста
    [field:SerializeField] public bool HasLumberAxe {get; private set;}// для теста
    [SerializeField] private InputSourceMode                  mode;
    [SerializeField] private AdvancedCharacterControllerData  controllerData;

    private CharacterAnimationContainer _animationContainer;  
    private CharacterSoundContainer    _soundContainer;
    private MoveSpeed                  _moveSpeed;
    private LocomotionSelector         _locomotionSelector;
    private CharacterAnimationEvents   _animationEvents;
    private LocomotionType             _currentLocomotionType;

    private MoveSpeedData _moveData;
    private Vector3 _clampedInput;
    private float _currentSpeed;
    
    public bool IsInteracting => PlayablesAnimatorController.OneShotIsActive();
    public LocomotionType CurrentLocomotionType => _locomotionSelector.GetLocomotionType();
    public CharacterSlots CharacterSlots {get; private set;}

    [Inject]
    private void Construct(
        PlayerInputSO                    playerInput,
        AIInput                          aiInput,
        CharacterController              controller,
        Animator                         animator,
        CharacterAnimationContainer      animationContainer,
        CharacterSoundContainer          soundContainer,
        AudioSource                      audioSource,
        CharacterAnimationEvents         animationEvents,
        CharacterSlots                   slots)
    {
        InputHandler = new InputHandler(playerInput, aiInput);
        InputHandler.SetupInput(mode);

        Controller          = new AdvancedCharacterController(controller, controllerData);
        _animationContainer = animationContainer;
        _soundContainer     = soundContainer;
        _animationEvents    = animationEvents;
        CharacterSlots      = slots;
        _locomotionSelector = new LocomotionSelector(Controller, InputHandler);
        _moveSpeed          = new MoveSpeed(InputHandler);

        PlayablesAnimatorController =
            new PlayablesAnimatorController(this, animator, audioSource, _animationContainer.LocomotionConfigs);
        
        PlayablesAnimatorController.SetEventTagResolver(_animationEvents.Resolve);

        SetLocomotion(true);
    }

    public void PlayInteractAnimation(AnimationClip animationClip, FrameEventConfig frameEventConfig)
    {
        if (IsInteracting) return;
        PlayablesAnimatorController.PlayOneShotAnimationClip(animationClip, frameEventConfig);
    }

    private void OnValidate()
    {
        InputHandler?.SetupInput(mode);
    }

    private void Update()
    {
        SetLocomotion();
        
        if (IsInteracting) return;
        
        _moveData = _animationContainer.GetMoveSpeedData(CurrentLocomotionType);
        _clampedInput = _moveSpeed.GetClampedInput(_moveData);
        _currentSpeed = _moveSpeed.GetSpeed(_moveData);
        
        Controller.JumpAndGravity(InputHandler.JumpPressed, _animationContainer.GetMoveSpeedData(LocomotionType.Jump0).YSpeed);
        Controller.Move(_clampedInput, _currentSpeed, controllerData.SpeedChangeRate);

        if (InputHandler.Rotation != Vector3.zero)
        {
            Controller.Rotation(InputHandler.Rotation, controllerData.RotationSpeed);
        }
        PlayablesAnimatorController.UpdateLocomotion(Controller.Velocity);
    }

    public override void SetLocomotion(bool isInitialization = false)
    {
        var locomotionType = _locomotionSelector.GetLocomotionType();
        if (locomotionType == _currentLocomotionType && !isInitialization)
        {
            return;
        }
        _currentLocomotionType = locomotionType;
        PlayablesAnimatorController.SetLocomotion(_currentLocomotionType);
        PlayablesAnimatorController.ConnectFootSteps(_soundContainer.GetAudioSet(_currentLocomotionType));
    }

    public void Interact(bool value, LocomotionType locomotionType)
    {
        Controller.Interact(value);
        _locomotionSelector.SetInteractLocomotion(locomotionType);
    }
    
    public void Interact(bool value, LocomotionConfigs locomotionConfigs)
    {
        Controller.Interact(value);
        _locomotionSelector.SetInteractLocomotion(locomotionConfigs);
    }

    private void OnDestroy()
    {
        PlayablesAnimatorController.Destroy();
    }
}