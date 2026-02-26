using UnityEngine;
using Zenject;

public class CharacterCore : CoreController
{
    [SerializeField] private InputSourceMode                  mode;
    [SerializeField] private AdvancedCharacterControllerData  controllerData;

    private CharacterAnimationContainer _animationContainer;
    private CharacterSoundContainer    _soundContainer;
    private MoveSpeed                  _moveSpeed;
    private LocomotionSelector         _locomotionSelector;
    private CharacterAnimationEvents   _animationEvents;
    private LocomotionType             _currentLocomotionType;
    
    private bool _isInteracting;

    [Inject]
    private void Construct(
        PlayerInput                      playerInput,
        AIInput                          aiInput,
        CharacterController              controller,
        Animator                         animator,
        CharacterAnimationContainer      animationContainer,
        CharacterSoundContainer          soundContainer,
        AudioSource                      audioSource,
        CharacterAnimationEvents         animationEvents)
    {
        InputHandler = new InputHandler(playerInput, aiInput);
        InputHandler.SetupInput(mode);

        Controller          = new AdvancedCharacterController(controller, controllerData);
        _animationContainer = animationContainer;
        _soundContainer     = soundContainer;
        _animationEvents    = animationEvents;
        _locomotionSelector = new LocomotionSelector(Controller, InputHandler);
        _moveSpeed          = new MoveSpeed(InputHandler);

        PlayablesAnimatorController =
            new PlayablesAnimatorController(this, animator, audioSource, _animationContainer.LocomotionConfigs);
        
        PlayablesAnimatorController.SetEventTagResolver(_animationEvents.Resolve);

        UpdateLocomotion(true);
    }

    // public void PlayInteractAnimation(AnimationClip animationClip)
    // {
    //     if (_isInteracting) return;
    //     _isInteracting = true;
    //
    //     PlayablesAnimatorController.PlayOneShotAnimationClip(animationClip,
    //         new FrameEventConfig(
    //             fromFrame: 2,
    //             toFrame:   30,
    //             onEnter:   () => testObject.SetActive(true),
    //             onExit:    () => testObject.SetActive(false)));
    // }

    private void OnValidate()
    {
        InputHandler?.SetupInput(mode);
    }

    private void Update()
    {
        UpdateLocomotion();

        var moveData = _animationContainer.GetMoveSpeedData(_locomotionSelector.GetLocomotionType());
        var clampedInput = _moveSpeed.GetClampedInput(moveData);
        var moveSpeed = _moveSpeed.GetSpeed(moveData);

        Controller.Move(clampedInput, moveSpeed, 1f);
        Controller.JumpAndGravity(InputHandler.JumpPressed, _animationContainer.GetMoveSpeedData(LocomotionType.Jump0).YSpeed);
        Controller.Rotation(InputHandler.Rotation, controllerData.RotationSpeed);

        PlayablesAnimatorController.UpdateLocomotion(Controller.HorizontalVelocity.normalized);
    }

    public override void UpdateLocomotion(bool isInitialization = false)
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

    private void OnDestroy()
    {
        PlayablesAnimatorController.Destroy();
    }
}