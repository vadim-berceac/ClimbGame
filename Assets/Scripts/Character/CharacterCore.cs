using Unity.Netcode;
using UnityEngine;
using Zenject;

public class CharacterCore : CoreController
{
    [field:Header("Test Inventory Temp")]
    [field:SerializeField] public bool HasPick {get; private set;}
    [field:SerializeField] public bool HasLumberAxe {get; private set;} 
    
    [field:Header("Controller Settings")]
    [SerializeField] private AdvancedCharacterControllerData  controllerData;

    private CharacterAnimationContainer _animationContainer; 
    private MoveSpeed                  _moveSpeed;
    private LocomotionSelector         _locomotionSelector;
    private CharacterAnimationEvents   _animationEvents;
    private LocomotionType             _currentLocomotionType;

    private MoveSpeedData _moveData;
    private Vector3 _clampedInput;
    private float _currentSpeed;
   
    private readonly NetworkVariable<InputSourceMode> _inputSourceMode = new(InputSourceMode.AI);
   
    private readonly NetworkVariable<Vector3> _networkVelocity = 
        new (Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
   
    private readonly NetworkVariable<LocomotionType> _networkLocomotionType = 
        new (LocomotionType.Walk0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
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

        Controller          = new AdvancedCharacterController(controller, controllerData);
        _animationContainer = animationContainer;
        _animationEvents    = animationEvents;
        CharacterSlots      = slots;
        _locomotionSelector = new LocomotionSelector(Controller, InputHandler);
        _moveSpeed          = new MoveSpeed(InputHandler);

        PlayablesAnimatorController =
            new PlayablesAnimatorController(this, animator, audioSource, _animationContainer.LocomotionConfigs, soundContainer);
        
        PlayablesAnimatorController.SetEventTagResolver(_animationEvents.Resolve);

        _currentLocomotionType = _animationContainer.GetDefaultLocomotionType();
        PlayablesAnimatorController.FinalizeLocomotionChange(_currentLocomotionType, UpdateLocomotion, UpdateNetworkLocomotion);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
       
        InputHandler.SetupInput(_inputSourceMode.Value);
        
        _inputSourceMode.OnValueChanged += OnInputSourceModeChanged;
        _networkLocomotionType.OnValueChanged += OnNetworkLocomotionTypeChanged;
    }

    private void OnInputSourceModeChanged(InputSourceMode previousValue, InputSourceMode newValue)
    {
        InputHandler.SetupInput(newValue);
    }

    private void OnNetworkLocomotionTypeChanged(LocomotionType previousValue, LocomotionType newValue)
    {
        if (!IsOwner)
        {
            _currentLocomotionType = newValue;
            PlayablesAnimatorController.SetLocomotion(newValue);
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            var locomotionType = _locomotionSelector.GetLocomotionType();
            
            if (locomotionType != _networkLocomotionType.Value && IsSpawned)
            {
                _networkLocomotionType.Value = locomotionType;
                SwitchLocomotion(_animationContainer.GetLocomotionConfigs(locomotionType));
            }
            
            if (IsInteracting)
            {
                return;
            }
            
            var time = PlayablesAnimatorController.NormalizedTime;
            _moveData = _animationContainer.GetMoveSpeedData(locomotionType);
            _clampedInput = _moveSpeed.GetCurrentInput();
            _currentSpeed = _moveSpeed.GetSpeed(_moveData, time);
                
            Controller.JumpAndGravity(InputHandler.JumpPressed, _moveData.GetY(time));
            Controller.Move(_clampedInput, _currentSpeed, controllerData.SpeedChangeRate);

            if (InputHandler.Rotation != Vector3.zero)
            {
                Controller.Rotation(InputHandler.Rotation, controllerData.RotationSpeed);
            }
               
            if (IsSpawned)
            {
                _networkVelocity.Value = Controller.Velocity;
            }
        }
        
        PlayablesAnimatorController.UpdateCurrentLocomotion(_networkVelocity.Value);
    }

    public override void SwitchLocomotion(LocomotionConfigs newLocomotionConfig, bool setBusy = false)
    {
        if (IsInteracting || !IsOwner) 
            return;

        Controller.SetBusy(setBusy);

        var currentConfig = _animationContainer.GetLocomotionConfigs(_currentLocomotionType);

        if (currentConfig.ExitEventField?.Clip != null)
        {
            if (!IsInteracting)
            {
                PlayablesAnimatorController.PlayOneAnimation(currentConfig.ExitEventField, () =>
                {
                    ApplyNewLocomotion(newLocomotionConfig);
                });
            }
        }
        else
        {
            ApplyNewLocomotion(newLocomotionConfig);
        }
    }

    private void ApplyNewLocomotion(LocomotionConfigs newConfig)
    {
        _locomotionSelector.SetInteractLocomotion(newConfig.Locomotion);

        if (newConfig.EnterEventField?.Clip != null)
        {
            if (!IsInteracting)
            {
                PlayablesAnimatorController.PlayOneAnimation(newConfig.EnterEventField, () =>
                {
                    PlayablesAnimatorController.FinalizeLocomotionChange(newConfig.Locomotion, UpdateLocomotion, UpdateNetworkLocomotion);
                });
            }
        }
        else
        {
            PlayablesAnimatorController.FinalizeLocomotionChange(newConfig.Locomotion, UpdateLocomotion, UpdateNetworkLocomotion);
        }
    }

    private void UpdateNetworkLocomotion(LocomotionType newType)
    {
        if (IsSpawned)
        {
            _networkLocomotionType.Value = newType;
        }
    }

    private void UpdateLocomotion(LocomotionType newType)
    {
        _currentLocomotionType = newType;
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public override void RequestOwnershipServerRpc(ulong requestingClientId, InputSourceMode mode)
    {
        var netObj = GetComponent<NetworkObject>();
        netObj.ChangeOwnership(requestingClientId);
        
        _inputSourceMode.Value = mode;
    }

    public override void OnNetworkDespawn()
    {
        _inputSourceMode.OnValueChanged -= OnInputSourceModeChanged;
        _networkLocomotionType.OnValueChanged -= OnNetworkLocomotionTypeChanged;
        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        PlayablesAnimatorController.Destroy();
        base.OnDestroy();
    }
}