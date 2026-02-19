using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [SerializeField] private InputSourceMode                  mode;
    [SerializeField] private float                            rotationSpeed = 50f;
    [SerializeField] private AdvancedCharacterControllerData  controllerData;
    [SerializeField] private AudioSet                         footSteps;
    [SerializeField] private CharacterAnimationEvents         _animationEvents;
    [SerializeField] private GameObject                       testObject;

    public AdvancedCharacterController  Controller                  { get; private set; }
    public PlayablesAnimatorController  PlayablesAnimatorController { get; private set; }

    private AnimationContainer  _animationContainer;
    private MoveSpeed           _moveSpeed;
    private LocomotionSelector  _locomotionSelector;
    private LocomotionType      _currentLocomotionType;

    public InputHandler InputHandler { get; private set; }
    private bool _isInteracting;

    [Inject]
    private void Construct(
        PlayerInput        playerInput,
        AIInput            aiInput,
        CharacterController controller,
        Animator           animator,
        AnimationContainer animationContainer,
        AudioSource        audioSource)
    {
        InputHandler = new InputHandler(playerInput, aiInput);
        InputHandler.SetupInput(mode);

        Controller          = new AdvancedCharacterController(controller, controllerData);
        _animationContainer = animationContainer;
        _locomotionSelector = new LocomotionSelector(Controller, InputHandler);
        _moveSpeed          = new MoveSpeed(InputHandler);

        PlayablesAnimatorController =
            new PlayablesAnimatorController(this, animator, audioSource, _animationContainer.LocomotionConfigs);

        // Что делать при каждом теге — задано на этом персонаже в инспекторе
        PlayablesAnimatorController.SetEventTagResolver(_animationEvents.Resolve);

        _currentLocomotionType = _animationContainer.DefaultLocomotion;
        PlayablesAnimatorController.SetLocomotion(_currentLocomotionType);
        PlayablesAnimatorController.ConnectFootSteps(footSteps);
    }

    public void PlayInteractAnimation(AnimationClip animationClip)
    {
        if (_isInteracting) return;
        _isInteracting = true;

        PlayablesAnimatorController.PlayOneShotAnimationClip(animationClip,
            new FrameEventConfig(
                fromFrame: 2,
                toFrame:   30,
                onEnter:   () => testObject.SetActive(true),
                onExit:    () => testObject.SetActive(false)));
    }

    // Вызывается из CharacterAnimationEvents через UnityEvent
    public void EnableTest()  => testObject.SetActive(true);
    public void DisableTest() => testObject.SetActive(false);

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

        var moveSpeed = _moveSpeed.GetSpeed(
            _animationContainer.GetMoveSpeedData(_locomotionSelector.GetLocomotionType()));

        Controller.Move(InputHandler.MoveInput, moveSpeed, 1f);
        Controller.JumpAndGravity(InputHandler.JumpPressed, _animationContainer.JumpConfigs[0].JumpHeight);
        Controller.Rotation(InputHandler.Rotation, rotationSpeed);

        PlayablesAnimatorController.UpdateLocomotion(Controller.HorizontalVelocity.normalized);

        if (Controller.IsJumping())
            PlayablesAnimatorController.PlayOneShotAnimationClip(_animationContainer.JumpConfigs[0].JumpStart0);
    }

    private void OnDestroy()
    {
        PlayablesAnimatorController.Destroy();
    }
}