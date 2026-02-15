using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : InputSource
{
   [SerializeField] private InputActionAsset inputAsset;

   private InputAction _move;
   private InputAction _jump;
   
   private void OnEnable()
   {
      FindActions();
      Subscribe();
   }

   private void FindActions()
   {
      _move = inputAsset.FindAction("Move");
      _jump = inputAsset.FindAction("Jump");
   }

   private void Subscribe()
   {
      _move.performed += OnMoveCTX;
      _move.canceled += OnMoveCTXCancel;
      
      _jump.performed += OnJumpCTX;
      _jump.canceled += OnJumpCTXCancel;
   }

   private void Unsubscribe()
   {
      _move.performed -= OnMoveCTX;
      _move.canceled -= OnMoveCTXCancel;
      
      _jump.performed -= OnJumpCTX;
      _jump.canceled -= OnJumpCTXCancel;
   }
   
   private void OnMoveCTX(InputAction.CallbackContext ctx)
   {
      OnMove = ctx.ReadValue<Vector2>();
   }
   
   private void OnMoveCTXCancel(InputAction.CallbackContext ctx)
   {
      OnMove = Vector2.zero;
   }
   
   private void OnJumpCTX(InputAction.CallbackContext ctx)
   {
      OnJump = true;
   }
   
   private void OnJumpCTXCancel(InputAction.CallbackContext ctx)
   {
      OnJump = false;
   }

   private void OnDisable()
   {
      Unsubscribe();
   }
}
