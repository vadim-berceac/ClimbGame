using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInput : InputSource
{
   [SerializeField] private InputActionAsset inputAsset;

   private InputAction _move;
   private InputAction _look;
   private InputAction _jump;
   private InputAction _run;
   private InputAction _crouch;
   private InputAction _interact;

   private InputAction _nextCharacter;
   
   private Camera _mainCamera;

   public Action OnCharacterSwitch;

   [Inject]
   private void Construct(Camera mainCamera)
   {
      _mainCamera = mainCamera;
   }
   
   private void OnEnable()
   {
      FindActions();
      Subscribe();
   }

   private void Update()
   {
      Rotation = _mainCamera.transform.rotation.eulerAngles;
   }

   private void FindActions()
   {
      _move = inputAsset.FindAction("Move");
      _look = inputAsset.FindAction("Look");
      _jump = inputAsset.FindAction("Jump");
      _run = inputAsset.FindAction("Run");
      _crouch = inputAsset.FindAction("Crouch");
      _interact = inputAsset.FindAction("Interact");
      
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
      
      _run.performed += OnRunCTX;
      _run.canceled += OnRunCTXCancel;
      
      _crouch.performed += OnCrouchCTX;
      _crouch.canceled += OnCrouchCTXCancel;
      
      _interact.performed += OnInteractCTX;
      _interact.canceled += OnInteractCTXCancel;
      
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
      
      _run.performed -= OnRunCTX;
      _run.canceled -= OnRunCTXCancel;
      
      _crouch.performed -= OnCrouchCTX;
      _crouch.canceled -= OnCrouchCTXCancel;
      
      _interact.performed -= OnInteractCTX;
      _interact.canceled -= OnInteractCTXCancel;
      
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
   
   private void OnRunCTX(InputAction.CallbackContext ctx)
   {
      OnRun = true;
   }
   
   private void OnRunCTXCancel(InputAction.CallbackContext ctx)
   {
      OnRun = false;
   }
   
   private void OnCrouchCTX(InputAction.CallbackContext ctx)
   {
      OnCrouch = true;
   }
   
   private void OnCrouchCTXCancel(InputAction.CallbackContext ctx)
   {
      OnCrouch = false;
   }
   
   private void OnInteractCTX(InputAction.CallbackContext ctx)
   {
      OnInteract = true;
   }
   
   private void OnInteractCTXCancel(InputAction.CallbackContext ctx)
   {
      OnInteract = false;
   }

   private void OnDisable()
   {
      Unsubscribe();
   }
}
