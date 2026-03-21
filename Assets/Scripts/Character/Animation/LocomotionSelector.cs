
public class LocomotionSelector
{
    private readonly AdvancedCharacterController _characterController;
    private readonly InputHandler _inputHandler;
    
    public LocomotionSelector(AdvancedCharacterController character, InputHandler inputHandler)
    {
        _characterController = character;
        _inputHandler = inputHandler;
    }

    public LocomotionType GetLocomotionType()
    {
        if (_characterController.IsFalling())
        {
            return LocomotionType.Fall0;
        }
        
        if (_characterController.IsSiting())
        {
            return LocomotionType.Sit0;
        }
        
        if (_characterController.IsJumping())
        {
            return LocomotionType.Jump0;
        }
        
        if (_characterController.IsClimbing() || _characterController.IsOnClimbableSurface())
        {
            return LocomotionType.Climb0;
        }
        
        if (_inputHandler.RunPressed)
        {
            return LocomotionType.Run0;
        }

        if (_inputHandler.CrouchPressed)
        {
            return LocomotionType.Crouch0;
        }

        return LocomotionType.Walk0;
    }
}
