using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class UIButtonGlyphComponent : MonoBehaviour
	{
		[SerializeField]
		private Button buttonComponent;

		[SerializeField]
		private TextMeshProUGUI localizedGlyphTextComponent;

		[SerializeField]
		private TextMeshProUGUI localizedTextComponent;

		[SerializeField]
		[ActionIdProperty(typeof(TRInputId))]
		private int inputActionID = -1;

		[SerializeField]
		private bool showOnlyGamepadGlyph;

		private GameInput gameInput;

		private UIManager uIManager;

		public void SetButtonTextLabel(string localizationKey)
		{
			uIManager.SetUILocalization(localizationKey, localizedTextComponent);
		}

		public void SetButtonTextGlyph(int inputActionID)
		{
			this.inputActionID = inputActionID;
			UpdateCurrentButtonGlyph(inputActionID);
		}

		public void ShowButton()
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
		}

		public void HideButton()
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}

		private void Awake()
		{
			gameInput = Core.Instance.GameInput;
			uIManager = Core.Instance.UIManager;
			if (buttonComponent != null)
			{
				buttonComponent.interactable = true;
			}
			UpdateCurrentButtonGlyph(inputActionID);
		}

		private void OnEnable()
		{
			gameInput.ListenToLastActiveControllerChanged(OnLastActiveControllerChanged);
			UpdateCurrentButtonGlyph(inputActionID);
		}

		private void OnDisable()
		{
			gameInput.StopListenToLastActiveControllerChanged(OnLastActiveControllerChanged);
		}

		private void OnLastActiveControllerChanged(int playerid, ControllerType controllerType)
		{
			UpdateCurrentButtonGlyph(inputActionID);
		}

		private void UpdateCurrentButtonGlyph(int inputActionID)
		{
			if (showOnlyGamepadGlyph)
			{
				UpdateCurrentGamepadButtonGlyph(inputActionID);
			}
			else
			{
				UpdateCurrentInputDeviceButtonGlyph(inputActionID);
			}
		}

		private void UpdateCurrentGamepadButtonGlyph(int inputActionID)
		{
			if (gameInput.IsPlayerControllerJoystick())
			{
				uIManager.ShowTextMeshProCurrentGamepadActionButtonGlyph(localizedGlyphTextComponent, inputActionID);
			}
		}

		private void UpdateCurrentInputDeviceButtonGlyph(int inputActionID)
		{
			uIManager.ShowTextMeshProCurrentControllerActionButtonGlyph(localizedGlyphTextComponent, inputActionID);
		}
	}
}
