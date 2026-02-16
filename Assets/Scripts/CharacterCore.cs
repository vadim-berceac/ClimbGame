using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [SerializeField] private InputSourceMode mode;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private AdvancedCharacterControllerData controllerData;
    
    public AdvancedCharacterController Controller { get; set; }
    public InputHandler InputHandler { get; set; }
    
    //test
    public Animator Animator { get; set; }
    private int _climbingStateHash;
    private int _jumpStateHash;
    
    [Inject]
    private void Construct(PlayerInput playerInput, AIInput aiInput, CharacterController controller, Animator animator)
    {
        InputHandler = new InputHandler(playerInput, aiInput);
        InputHandler.SetupInput(mode);
        
        Controller = new AdvancedCharacterController(controller, controllerData);
        
        Animator = animator;
        _climbingStateHash = Animator.StringToHash("IsClimb");
        _jumpStateHash = Animator.StringToHash("IsJump");
    }

    private void OnValidate()
    {
        InputHandler?.SetupInput(mode);
    }

    private void Update()
    {
        Controller.Move(InputHandler.MoveInput, walkSpeed, 0.1f);
        Controller.JumpAndGravity(InputHandler.JumpPressed, jumpHeight);
        Controller.Rotation(InputHandler.Rotation, rotationSpeed);
        
        Animator.SetBool(_climbingStateHash, Controller.IsClimbing() || Controller.IsOnClimbableSurface());
        Animator.SetBool(_jumpStateHash, Controller.IsJumping());
    }
}