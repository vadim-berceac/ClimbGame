using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [SerializeField] private InputSourceMode                  mode;
    [SerializeField] private AdvancedCharacterControllerData  controllerData;

    public AdvancedCharacterController  Controller                  { get; private set; }
    public PlayablesAnimatorController  PlayablesAnimatorController { get; private set; }

    private AnimationContainer         _animationContainer;
    private SoundContainer             _soundContainer;
    private MoveSpeed                  _moveSpeed;
    private LocomotionSelector         _locomotionSelector;
    private CharacterAnimationEvents   _animationEvents;
    private LocomotionType             _currentLocomotionType;

    public InputHandler InputHandler { get; private set; }
    private bool _isInteracting;

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

        var moveSpeed = _moveSpeed.GetSpeed(
            _animationContainer.GetMoveSpeedData(_locomotionSelector.GetLocomotionType()));

        Controller.Move(InputHandler.MoveInput, moveSpeed, 1f);
        Controller.JumpAndGravity(InputHandler.JumpPressed, _animationContainer.JumpConfigs[0].JumpHeight);
        Controller.Rotation(InputHandler.Rotation, controllerData.RotationSpeed);

        PlayablesAnimatorController.UpdateLocomotion(Controller.HorizontalVelocity.normalized);

        if (Controller.IsJumping())
            PlayablesAnimatorController.PlayOneShotAnimationClip(_animationContainer.JumpConfigs[0].JumpStart0);
    }

    private void UpdateLocomotion(bool isInitialization = false)
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