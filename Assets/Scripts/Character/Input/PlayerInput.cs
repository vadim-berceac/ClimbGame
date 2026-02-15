using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : InputSource
{
   [SerializeField] private InputActionAsset inputAsset;

   private InputAction _move;
   private InputAction _look;
   private InputAction _jump;

   private InputAction _nextCharacter;

   public Action OnCharacterSwitch;
   
   private void OnEnable()
   {
      FindActions();
      Subscribe();
   }

   private void FindActions()
   {
      _move = inputAsset.FindAction("Move");
      _look = inputAsset.FindAction("Look");
      _jump = inputAsset.FindAction("Jump");
      
      _nextCharacter = inputAsset.FindAction("Next");
   }

   private void Subscribe()
   {
      _move.performed += OnMoveCTX;
      _move.canceled += OnMoveCTXCancel;
      
      _look.performed += OnLookCTX;
      _look.canceled += OnLookCTXCancel;
      
      _jump.performed += OnJumpCTX;
      _jump.canceled += OnJumpCTXCancel;
      
      _nextCharacter.performed += OnNextCTX;
   }

   private void Unsubscribe()
   {
      _move.performed -= OnMoveCTX;
      _move.canceled -= OnMoveCTXCancel;
      
      _look.performed -= OnLookCTX;
      _look.canceled -= OnLookCTXCancel;
      
      _jump.performed -= OnJumpCTX;
      _jump.canceled -= OnJumpCTXCancel;
      
      _nextCharacter.performed -= OnNextCTX;
   }

   private void OnNextCTX(InputAction.CallbackContext context)
   {
      OnCharacterSwitch?.Invoke();
   }
   
   private void OnMoveCTX(InputAction.CallbackContext ctx)
   {
      OnMove = ctx.ReadValue<Vector2>();
   }
   
   private void OnMoveCTXCancel(InputAction.CallbackContext ctx)
   {
      OnMove = Vector2.zero;
   }

   private void OnLookCTX(InputAction.CallbackContext ctx)
   {
      OnLook = ctx.ReadValue<Vector2>();
   }

   private void OnLookCTXCancel(InputAction.CallbackContext ctx)
   {
      OnLook = Vector2.zero;
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
