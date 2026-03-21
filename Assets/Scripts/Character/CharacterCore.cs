using UnityEngine;
using UnityEngine.Serialization;
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
    
    public bool IsInteracting { get; private set; }
    public bool CanMove { get; private set; } = true;

    [Inject]
    private void Construct(
        PlayerInputSO                    playerInput,
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

        SetLocomotion(true);
    }

    public void PlayInteractAnimation(AnimationClip animationClip, bool canMove)
    {
        if (IsInteracting) return;
        IsInteracting = true;
        CanMove = false;
        PlayablesAnimatorController.PlayOneShotAnimationClip(animationClip,
            new FrameEventConfig(
                fromFrame: 0,
                toFrame:  20,
                onExit: () => InteractEnd()
            ));
    }

    private void InteractEnd()
    {
        Debug.Log("AnimFinish");
        IsInteracting = false;
        CanMove = true;
    }

    private void OnValidate()
    {
        InputHandler?.SetupInput(mode);
    }

    private void Update()
    {
        SetLocomotion();
        if(!CanMove) return;
        
        var moveData = _animationContainer.GetMoveSpeedData(_locomotionSelector.GetLocomotionType());
        var clampedInput = _moveSpeed.GetClampedInput(moveData);
        var moveSpeed = _moveSpeed.GetSpeed(moveData);
        
        Controller.JumpAndGravity(InputHandler.JumpPressed, _animationContainer.GetMoveSpeedData(LocomotionType.Jump0).YSpeed);
        Controller.Move(clampedInput, moveSpeed, controllerData.SpeedChangeRate);

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

    private void OnDestroy()
    {
        PlayablesAnimatorController.Destroy();
    }
}