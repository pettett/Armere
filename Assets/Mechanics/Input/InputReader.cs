using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

[CreateAssetMenu(menuName = "Channels/Input Reader")]
public class InputReader : ScriptableObject, PlayerControls.IGroundActionMapActions, PlayerControls.IDebugActions
{
	public static bool PhaseMeansPressed(InputActionPhase phase) => phase switch
	{
		InputActionPhase.Performed => true,
		InputActionPhase.Started => true,
		InputActionPhase.Canceled => false,
		InputActionPhase.Disabled => false,
		InputActionPhase.Waiting => false,
		_ => false
	};
	public static bool DeviceIsMouse(CallbackContext context) => context.control.device.name == "Mouse";

	public UnityAction<InputActionPhase> actionEvent;
	public UnityAction<InputActionPhase> aimEvent;
	public UnityAction<InputActionPhase> attackEvent;
	public UnityAction<InputActionPhase> altAttackEvent;
	public UnityAction<Vector2, bool> cameraMoveEvent;
	public UnityAction<InputActionPhase> changeFocusEvent;
	public UnityAction<InputActionPhase> consoleEvent;
	public UnityAction<InputActionPhase> jumpEvent;
	public UnityAction<float> verticalMovementEvent;
	public UnityAction<Vector2> movementEvent;
	public UnityAction<InputActionPhase> crouchEvent;
	public UnityAction<InputActionPhase> koEvent;
	public UnityAction<InputActionPhase> tabMenuEvent;
	public UnityAction<InputActionPhase> switchWeaponSetEvent;
	public UnityAction<InputActionPhase> sprintEvent;
	public UnityAction<InputActionPhase> shieldEvent;
	public UnityAction<InputActionPhase> showReadoutScreenEvent;
	public UnityAction<InputActionPhase, int> selectWeaponEvent;
	public UnityAction<InputActionPhase> quicksaveEvent;
	public UnityAction<InputActionPhase> quickloadEvent;

	private PlayerControls gameInput;

	public InputActionAsset asset => gameInput.asset;

	private void OnEnable()
	{
		if (gameInput == null)
		{
			gameInput = new PlayerControls();
			gameInput.GroundActionMap.SetCallbacks(this);
			gameInput.Debug.SetCallbacks(this);
		}

		EnableGameplayInput();
	}

	private void OnDisable()
	{
		DisableAllInput();
	}

	public void EnableGameplayInput()
	{
		gameInput.GroundActionMap.Enable();
		gameInput.Debug.Enable();
	}

	public void DisableAllInput()
	{
		gameInput.GroundActionMap.Disable();
		gameInput.Debug.Disable();
	}


	public void OnAction(CallbackContext context) => actionEvent?.Invoke(context.phase);
	public void OnAim(CallbackContext context) => aimEvent?.Invoke(context.phase);
	public void OnAltAttack(CallbackContext context) => altAttackEvent?.Invoke(context.phase);
	public void OnAttack(CallbackContext context) => attackEvent?.Invoke(context.phase);
	public void OnCameraMove(CallbackContext context) => cameraMoveEvent?.Invoke(context.ReadValue<Vector2>(), DeviceIsMouse(context));
	public void OnChangeFocus(CallbackContext context) => changeFocusEvent?.Invoke(context.phase);
	public void OnConsole(CallbackContext context) => consoleEvent?.Invoke(context.phase);
	public void OnCrouch(CallbackContext context) => crouchEvent?.Invoke(context.phase);
	public void OnJump(CallbackContext context) => jumpEvent?.Invoke(context.phase);
	public void OnKO(CallbackContext context) => koEvent?.Invoke(context.phase);
	public void OnSelectWeapon(CallbackContext context) => selectWeaponEvent?.Invoke(context.phase, (int)context.ReadValue<float>());
	public void OnShield(CallbackContext context) => shieldEvent?.Invoke(context.phase);
	public void OnSprint(CallbackContext context) => sprintEvent?.Invoke(context.phase);
	public void OnSwitchWeaponSet(CallbackContext context) => switchWeaponSetEvent?.Invoke(context.phase);
	public void OnTabMenu(CallbackContext context) => tabMenuEvent?.Invoke(context.phase);
	public void OnVerticalMovement(CallbackContext context) => verticalMovementEvent?.Invoke(context.ReadValue<float>());
	public void OnWalk(CallbackContext context) => movementEvent?.Invoke(context.ReadValue<Vector2>());

	public void OnShowReadoutScreen(CallbackContext context) => showReadoutScreenEvent?.Invoke(context.phase);


	public void OnQuickSave(InputAction.CallbackContext context) => quicksaveEvent?.Invoke(context.phase);
	public void OnQuickLoad(InputAction.CallbackContext context) => quickloadEvent?.Invoke(context.phase);

}
