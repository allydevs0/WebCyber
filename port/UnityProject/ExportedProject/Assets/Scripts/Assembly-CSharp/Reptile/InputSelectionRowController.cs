using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class InputSelectionRowController : MonoBehaviour
	{
		private const string UNKNOWN_BINDING = "???";

		private const string CONFLICTED_LOCALIZATION_KEY = "INPUT_CONFLICTED";

		private const string WAITING_LOCALIZATION_KEY = "INPUT_WAITING";

		[SerializeField]
		private GameObject gamepadBinding;

		[SerializeField]
		private GameObject keyboardMouseBinding;

		[SerializeField]
		private TextMeshProUGUI gamepadButtonGlyph;

		[SerializeField]
		private TextMeshProUGUI keyboardMouseBindingText;

		[SerializeField]
		private Image buttonImage;

		[SerializeField]
		[ActionIdProperty(typeof(TRInputId))]
		private int inputActionId;

		[SerializeField]
		private Pole axisContribution;

		[SerializeField]
		private bool isMovementAxis;

		private UIManager uIManager;

		private GameInput gameInput;

		private RewiredMappingHandler mappingHandler;

		private ActionElementMap gamepadActionElementMap;

		private ActionElementMap keyboardActionElementMap;

		private ActionElementMap mouseActionElementMap;

		private IGameTextLocalizer localizer;

		private bool isGamepadConflicted;

		private bool isKeyboardMouseConflicted;

		public int InputActionId => inputActionId;

		public bool IsMovementAxis => isMovementAxis;

		public Pole AxisContribution => axisContribution;

		public int GamepadControllerMapCategoryId
		{
			get
			{
				if (gamepadActionElementMap == null)
				{
					return -1;
				}
				return gamepadActionElementMap.controllerMap.categoryId;
			}
		}

		public int KeyboardControllerMapCategoryId
		{
			get
			{
				if (keyboardActionElementMap == null)
				{
					return -1;
				}
				return keyboardActionElementMap.controllerMap.categoryId;
			}
		}

		public int MouseControllerMapCategoryId
		{
			get
			{
				if (mouseActionElementMap == null)
				{
					return -1;
				}
				return mouseActionElementMap.controllerMap.categoryId;
			}
		}

		public bool HasGamepadBinding => gamepadActionElementMap != null;

		public bool HasKeyboardBinding => keyboardActionElementMap != null;

		public bool HasMouseBinding => mouseActionElementMap != null;

		public bool IsGamepadConflicted
		{
			get
			{
				if (!isGamepadConflicted)
				{
					return isKeyboardMouseConflicted;
				}
				return true;
			}
		}

		public bool IsKeyboardMouseConflicted => isKeyboardMouseConflicted;

		public void ShowGamepadBinding()
		{
			if (!isMovementAxis)
			{
				gamepadBinding.SetActive(value: true);
			}
		}

		public void HideGamepadBinding()
		{
			gamepadBinding.SetActive(value: false);
		}

		public void ShowKeyboardMouseBinding()
		{
			keyboardMouseBinding.SetActive(value: true);
		}

		public void HideKeyboardMouseBinding()
		{
			keyboardMouseBinding.SetActive(value: false);
		}

		public void RefreshGamepadButtonGlyph()
		{
			if (isGamepadConflicted)
			{
				ShowGamepadBindingConflicted();
			}
			else if (gamepadActionElementMap == null)
			{
				gamepadButtonGlyph.text = "???";
			}
			else
			{
				uIManager.ShowTextMeshProCurrentGamepadActionButtonGlyph(gamepadButtonGlyph, inputActionId);
			}
		}

		public void ShowAttemptMappingKeyboardMouseActionBinding()
		{
			keyboardMouseBindingText.text = localizer.GetUserInterfaceText("INPUT_WAITING");
		}

		public void StopShowingAttemptMappingKeyboardMouseActionBinding()
		{
			RefreshKeyboardMouseBinding();
		}

		public void RefreshKeyboardMouseBinding()
		{
			if (isKeyboardMouseConflicted)
			{
				ShowKeyboardMouseBindingConflicted();
				return;
			}
			ActionElementMap keyboardMouseActionElementMap = GetKeyboardMouseActionElementMap();
			if (keyboardMouseActionElementMap == null)
			{
				keyboardMouseBindingText.text = "???";
			}
			else
			{
				keyboardMouseBindingText.text = keyboardMouseActionElementMap.elementIdentifierName.ToString();
			}
		}

		public void ShowAttemptMappingGamepadActionBinding()
		{
			gamepadButtonGlyph.text = localizer.GetUserInterfaceText("INPUT_WAITING");
		}

		public void StopShowingAttemptMappingGamepadActionBinding()
		{
			RefreshGamepadButtonGlyph();
		}

		public ActionElementMap GetKeyboardActionElementMap()
		{
			return keyboardActionElementMap;
		}

		public ActionElementMap GetMouseActionElementMap()
		{
			return mouseActionElementMap;
		}

		public ActionElementMap GetGamepadActionElementMap()
		{
			return gamepadActionElementMap;
		}

		public void RefreshGamepadActionElementMap()
		{
			gamepadActionElementMap = mappingHandler.GetCurrentControllerActionElementMap(inputActionId, ControllerType.Joystick);
		}

		public void RefreshKeyboardMouseActionElementMap()
		{
			List<ActionElementMap> allControllersActionElementMaps = mappingHandler.GetAllControllersActionElementMaps(inputActionId);
			if (isMovementAxis)
			{
				keyboardActionElementMap = GetControllerTypeAxisActionElementMap(allControllersActionElementMaps, ControllerType.Keyboard, axisContribution);
			}
			else
			{
				keyboardActionElementMap = GetControllerTypeActionElementMap(allControllersActionElementMaps, ControllerType.Keyboard);
			}
			mouseActionElementMap = GetControllerTypeActionElementMap(allControllersActionElementMaps, ControllerType.Mouse);
		}

		public void UpdateGamepadBinding(ActionElementMap actionElementMap)
		{
			gamepadActionElementMap = actionElementMap;
		}

		public void UpdateKeyboardBinding(ActionElementMap actionElementMap)
		{
			keyboardActionElementMap = actionElementMap;
		}

		public void UpdateMouseBinding(ActionElementMap actionElementMap)
		{
			mouseActionElementMap = actionElementMap;
		}

		public void RemoveGamepadBinding()
		{
			gamepadActionElementMap = null;
		}

		public void RemoveKeyboardBinding()
		{
			keyboardActionElementMap = null;
		}

		public void RemoveMouseBinding()
		{
			mouseActionElementMap = null;
		}

		public void SetGamepadConflictedState(bool isConflicted)
		{
			isGamepadConflicted = isConflicted;
		}

		public void SetKeyboardMouseConflictedState(bool isConflicted)
		{
			isKeyboardMouseConflicted = isConflicted;
		}

		public void DisableRaycastTarget()
		{
			buttonImage.raycastTarget = false;
		}

		public void EnableRaycastTarget()
		{
			buttonImage.raycastTarget = true;
		}

		public int GetBindableControllerMapCategoryId()
		{
			if (gamepadActionElementMap == null)
			{
				return GetControllerMapCategoryId(ControllerType.Keyboard);
			}
			return GamepadControllerMapCategoryId;
		}

		public int GetControllerMapCategoryId(ControllerType controllerType)
		{
			if (controllerType == ControllerType.Joystick)
			{
				return GamepadControllerMapCategoryId;
			}
			if (keyboardActionElementMap == null)
			{
				return MouseControllerMapCategoryId;
			}
			return KeyboardControllerMapCategoryId;
		}

		protected void Awake()
		{
			uIManager = Core.Instance.UIManager;
			gameInput = Core.Instance.GameInput;
			mappingHandler = gameInput.RewiredMappingHandler;
			localizer = Core.Instance.Localizer;
		}

		protected void OnDestroy()
		{
		}

		private ActionElementMap GetControllerTypeActionElementMap(List<ActionElementMap> elementMapsForThisAction, ControllerType controllerType)
		{
			return elementMapsForThisAction.Find((ActionElementMap x) => x.controllerMap.controllerType == controllerType);
		}

		private ActionElementMap GetControllerTypeAxisActionElementMap(List<ActionElementMap> elementMapsForThisAction, ControllerType controllerType, Pole axisContribution)
		{
			return elementMapsForThisAction.Find((ActionElementMap x) => x.controllerMap.controllerType == controllerType && x.axisContribution == axisContribution);
		}

		private ActionElementMap GetKeyboardMouseActionElementMap()
		{
			if (keyboardActionElementMap == null)
			{
				return mouseActionElementMap;
			}
			return keyboardActionElementMap;
		}

		private void ShowGamepadBindingConflicted()
		{
			gamepadButtonGlyph.text = localizer.GetUserInterfaceText("INPUT_CONFLICTED");
		}

		private void ShowKeyboardMouseBindingConflicted()
		{
			keyboardMouseBindingText.text = localizer.GetUserInterfaceText("INPUT_CONFLICTED");
		}
	}
}
