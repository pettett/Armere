using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

[CreateAssetMenu(menuName = "Channels/Input Reader")]
public class InputReader : ScriptableObject, PlayerControls.IGroundActionMapActions, PlayerControls.IDebugActions, PlayerControls.IUIActions, PlayerControls.IAlwaysActiveActions
{
	public const string groundActionMap = "Ground Action Map";
	public const string attackAction = "Attack";
	public const string altAttackAction = "AltAttack";
	public string GetBindingDisplayString(string map, string action)
	{
		return asset.FindActionMap(map).FindAction(action).GetBindingDisplayString();
	}
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
	public UnityAction<InputActionPhase> openInventoryEvent;
	public UnityAction<InputActionPhase> openMapEvent;
	public UnityAction<InputActionPhase> openQuestsEvent;
	public UnityAction<InputActionPhase> startChangeSelection;
	public UnityAction<InputActionPhase> changeSelection;
	public UnityAction<InputActionPhase> sprintEvent;
	public UnityAction<InputActionPhase> shieldEvent;
	public UnityAction<InputActionPhase> showReadoutScreenEvent;
	public UnityAction<InputActionPhase, int> selectSpellEvent;
	public UnityAction<InputActionPhase> quicksaveEvent;
	public UnityAction<InputActionPhase> quickloadEvent;
	public UnityAction<InputActionPhase> equipBowEvent;

	public UnityAction<InputActionPhase> uiSubmitEvent;
	public UnityAction<InputActionPhase> uiCancelEvent;
	public UnityAction<Vector2> uiNavigateEvent;
	public UnityAction<InputActionPhase, float> uiNavigateHorizontalEvent;
	public UnityAction<InputAction.CallbackContext> uiScrollEvent;

	private PlayerControls gameInput;

	public InputActionAsset asset => gameInput.asset;

	public Vector2 horizontalMovement { get; private set; }
	public float verticalMovement { get; private set; }



	public void VirtualCameraMove(Vector2 movement, bool mouse) => cameraMoveEvent?.Invoke(movement, mouse);
	public void VirtualMovement(Vector2 movement)
	{
		movementEvent?.Invoke(movement);
		horizontalMovement = movement;
	}

	private void OnEnable()
	{
		if (gameInput == null)
		{
			gameInput = new PlayerControls();
			gameInput.GroundActionMap.SetCallbacks(this);
			gameInput.Debug.SetCallbacks(this);
			gameInput.UI.SetCallbacks(this);
			gameInput.AlwaysActive.SetCallbacks(this);
		}

		SwitchToGameplayInput();
		gameInput.Debug.Enable();
		gameInput.AlwaysActive.Enable();
	}

	private void OnDisable()
	{
		DisableAllInput();
		gameInput.Debug.Disable();
		gameInput.AlwaysActive.Disable();
	}

	public void SwitchToGameplayInput()
	{
		gameInput.GroundActionMap.Enable();
		gameInput.UI.Disable();
	}

	public void DisableAllInput()
	{
		gameInput.GroundActionMap.Disable();

		gameInput.UI.Disable();
	}

	public void SwitchToUIInput()
	{
		gameInput.UI.Enable();
		gameInput.GroundActionMap.Disable();
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
	public void OnSelectSpell(CallbackContext context) => selectSpellEvent?.Invoke(context.phase, (int)context.ReadValue<float>());
	public void OnEquipBow(CallbackContext context) => equipBowEvent?.Invoke(context.phase);
	public void OnShield(CallbackContext context) => shieldEvent?.Invoke(context.phase);
	public void OnSprint(CallbackContext context) => sprintEvent?.Invoke(context.phase);

	public bool IsSprintPressed() => gameInput.GroundActionMap.Sprint.ReadValue<float>() == 1;
	public bool IsCrouchPressed() => gameInput.GroundActionMap.Crouch.ReadValue<float>() == 1;


	public void OnTabMenu(CallbackContext context) => tabMenuEvent?.Invoke(context.phase);
	public void OnOpenInventory(CallbackContext context) => openInventoryEvent?.Invoke(context.phase);
	public void OnOpenQuests(CallbackContext context) => openQuestsEvent?.Invoke(context.phase);
	public void OnOpenMap(CallbackContext context) => openMapEvent?.Invoke(context.phase);
	public void OnVerticalMovement(CallbackContext context)
	{
		verticalMovement = context.ReadValue<float>();
		verticalMovementEvent?.Invoke(verticalMovement);
	}
	public void OnWalk(CallbackContext context)
	{
		horizontalMovement = context.ReadValue<Vector2>();
		movementEvent?.Invoke(horizontalMovement);
	}

	public void OnShowReadoutScreen(CallbackContext context) => showReadoutScreenEvent?.Invoke(context.phase);


	public void OnQuickSave(InputAction.CallbackContext context) => quicksaveEvent?.Invoke(context.phase);
	public void OnQuickLoad(InputAction.CallbackContext context) => quickloadEvent?.Invoke(context.phase);

	public void OnSumbit(CallbackContext context) => uiSubmitEvent?.Invoke(context.phase);

	public void OnCancel(CallbackContext context) => uiCancelEvent?.Invoke(context.phase);

	public void OnNavigate(CallbackContext context) => uiNavigateEvent?.Invoke(context.ReadValue<Vector2>());

	public void OnScroll(CallbackContext context)
	{
		uiScrollEvent?.Invoke(context);
	}


	public void OnChangeSelection(CallbackContext context)
	{
		changeSelection?.Invoke(context.phase);
	}

	public void OnNavigateHorizontal(CallbackContext context)
	{
		uiNavigateHorizontalEvent?.Invoke(context.phase, context.ReadValue<float>());
	}

	public string GetActionDisplayName(string action)
	{
		return asset.FindAction(action).controls[0].displayName;
	}

	public void OnTrackedDeviceOrientation(CallbackContext context)
	{
	}

	public void OnTrackedDevicePosition(CallbackContext context)
	{
	}

	public void OnRightClick(CallbackContext context)
	{
	}

	public void OnMiddleClick(CallbackContext context)
	{
	}

	public void OnScrollWheel(CallbackContext context)
	{
	}

	public void OnClick(CallbackContext context)
	{
	}

	public void OnPoint(CallbackContext context)
	{
	}

	public void OnSubmit(CallbackContext context)
	{
	}
}
