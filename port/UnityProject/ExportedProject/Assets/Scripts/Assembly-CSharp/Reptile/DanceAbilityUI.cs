using TMPro;
using UnityEngine;

namespace Reptile
{
	public class DanceAbilityUI : AMenuController
	{
		public const int DANCE_OPTIONS_AMOUNT = 6;

		private const int NO_DANCE_SELECTED = -1;

		private const float HALF_CIRCLE_DEGREES = 180f;

		private const float FULL_CIRCLE_DEGREES = 360f;

		[SerializeField]
		private RectTransform pointer;

		[SerializeField]
		private TextMeshProUGUI optionText;

		[SerializeField]
		private float pointerMovementRadius = 220f;

		[SerializeField]
		private float pointerMouseDeadzone = 200f;

		[SerializeField]
		private DanceAbilityUIButton[] optionIcons = new DanceAbilityUIButton[6];

		private Vector3 pointerOriginalPosition;

		private Vector2 pointerDirection;

		private Vector2 inputVector;

		private Vector2 middleOfScreen;

		private int currentIndex = -1;

		private float pointerMouseDeadzoneSqr;

		private GameInput gameInput;

		private SaveSlotData currentSaveSlot;

		public override void Activate()
		{
			base.Activate();
			pointer.localPosition = pointerOriginalPosition;
			inputVector = Vector2.zero;
			optionText.text = string.Empty;
			currentIndex = -1;
			InitializeDanceButtons();
			DetermineInputDirection();
			if (inputVector.sqrMagnitude == 0f)
			{
				SetPointerSelection(Vector2.up);
			}
			WorldHandler.instance.GetCurrentPlayer().cam.ignoreInput = true;
			Core.OnUpdate += UpdateUI;
		}

		public override void Deactivate()
		{
			base.Deactivate();
			WorldHandler.instance.GetCurrentPlayer().cam.ignoreInput = false;
			Core.OnUpdate -= UpdateUI;
		}

		public override GameObject GetFirstFocusGameObject()
		{
			return null;
		}

		public void Init(GameInput gameInput, SaveSlotData saveSlot)
		{
			this.gameInput = gameInput;
			currentSaveSlot = saveSlot;
			middleOfScreen = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			pointerOriginalPosition = pointer.localPosition;
			pointerMouseDeadzoneSqr = Mathf.Pow(pointerMouseDeadzone, 2f);
			base.gameObject.SetActive(value: false);
			Core.OnScreenSizeChanged += OnScreenSizeChanged;
		}

		public int GetCurrentSelectedOption()
		{
			float circleArcPercentage = 1f - (180f + Vector2.SignedAngle(Vector2.down, pointerDirection)) / 360f;
			for (int i = 0; i < optionIcons.Length; i++)
			{
				if (optionIcons[i].IsActive && optionIcons[i].IsInCircleArc(circleArcPercentage))
				{
					return i;
				}
			}
			return -1;
		}

		public bool IsOptionSelected()
		{
			if (currentIndex == -1 || base.IsTransitioning || !base.IsEnabled)
			{
				return false;
			}
			if (!gameInput.GetButtonNew(2))
			{
				return gameInput.GetButtonHeld(66);
			}
			return true;
		}

		public bool HasCancelled()
		{
			if (base.IsTransitioning || !base.IsEnabled)
			{
				return false;
			}
			if (!gameInput.GetButtonNew(3))
			{
				return gameInput.GetButtonNew(17);
			}
			return true;
		}

		public void UpdateUI()
		{
			DetermineDanceSelection();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Core.OnScreenSizeChanged -= OnScreenSizeChanged;
			if (Core.Instance != null)
			{
				UIManager uIManager = Core.Instance.UIManager;
				if (uIManager != null)
				{
					uIManager.AbsolutePopIfPresent(this);
				}
			}
		}

		private void InitializeDanceButtons()
		{
			for (int i = 0; i < optionIcons.Length; i++)
			{
				optionIcons[i].Dehighlight();
				int danceSaveId = optionIcons[i].DanceSaveId;
				if (currentSaveSlot.dancesUnlocked[danceSaveId])
				{
					optionIcons[i].ShowButton();
				}
				else
				{
					optionIcons[i].HideButton();
				}
			}
		}

		private void OnScreenSizeChanged(int width, int height)
		{
			middleOfScreen = new Vector2((float)width * 0.5f, (float)height * 0.5f);
		}

		private void DetermineDanceSelection()
		{
			DetermineInputDirection();
			if (inputVector.sqrMagnitude > 0f)
			{
				SetPointerSelection(inputVector);
			}
		}

		private void SetPointerSelection(Vector2 inputDirection)
		{
			pointerDirection = inputDirection;
			pointer.localPosition = Vector2.Lerp(pointer.localPosition, inputDirection * pointerMovementRadius, 0.9f);
			RotatePointer();
			HighlightSelection();
		}

		private void DetermineInputDirection()
		{
			if (gameInput.IsPlayerControllerJoystick())
			{
				HandleControllerInput();
			}
			else
			{
				HandleMouseInput();
			}
		}

		private void HandleControllerInput()
		{
			float axis = gameInput.GetAxis(5);
			float axis2 = gameInput.GetAxis(6);
			float axis3 = gameInput.GetAxis(13);
			float axis4 = gameInput.GetAxis(14);
			Vector2 vector = new Vector2(axis, axis2);
			Vector2 vector2 = new Vector2(axis3, axis4);
			if (vector.sqrMagnitude > 0f)
			{
				inputVector = vector;
			}
			else if (vector2.sqrMagnitude > 0f)
			{
				inputVector = vector2;
			}
			inputVector.Normalize();
		}

		private void HandleMouseInput()
		{
			Vector2 vector = gameInput.GetMousePosition() - middleOfScreen;
			if (vector.sqrMagnitude <= pointerMouseDeadzoneSqr)
			{
				vector = Vector2.zero;
			}
			if (!gameInput.IsPlayerControllerJoystick())
			{
				inputVector = vector;
				inputVector.Normalize();
			}
		}

		private void RotatePointer()
		{
			float z = Vector2.SignedAngle(Vector2.down, middleOfScreen - new Vector2(pointer.position.x, pointer.position.y));
			pointer.localEulerAngles = new Vector3(0f, 0f, z);
		}

		private void HighlightSelection()
		{
			int currentSelectedOption = GetCurrentSelectedOption();
			if (currentIndex == currentSelectedOption)
			{
				return;
			}
			if (currentSelectedOption >= 0 && optionIcons[currentSelectedOption].IsActive)
			{
				optionText.text = optionIcons[currentSelectedOption].DanceName;
				optionIcons[currentSelectedOption].Highlight();
				if (currentIndex != -1)
				{
					optionIcons[currentIndex].Dehighlight();
				}
			}
			else
			{
				optionText.text = string.Empty;
				if (currentIndex != -1)
				{
					optionIcons[currentIndex].Dehighlight();
				}
			}
			currentIndex = currentSelectedOption;
		}
	}
}
