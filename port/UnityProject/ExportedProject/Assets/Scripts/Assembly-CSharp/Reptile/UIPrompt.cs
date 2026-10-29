using Rewired;
using UnityEngine;
using UnityEngine.Events;

namespace Reptile
{
	public class UIPrompt : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProMenuButton option1Button;

		[SerializeField]
		private TextMeshProMenuButton option2Button;

		[SerializeField]
		private TMProLocalizationAddOn option1TextAddOn;

		[SerializeField]
		private TMProLocalizationAddOn option2TextAddOn;

		[SerializeField]
		private TMProLocalizationAddOn messageTextAddOn;

		[SerializeField]
		private RectTransform loadingIcon;

		[SerializeField]
		private Vector3 option1SoloPosition;

		[SerializeField]
		private Vector3 option1DualPosition;

		public void ShowLoading()
		{
			base.gameObject.SetActive(value: true);
			option2Button.gameObject.SetActive(value: false);
			option1Button.gameObject.SetActive(value: false);
			messageTextAddOn.gameObject.SetActive(value: false);
			loadingIcon.gameObject.SetActive(value: true);
		}

		internal void LastActiveControllerChanged(int playerid, ControllerType controllerType)
		{
			if (controllerType == ControllerType.Joystick)
			{
				option1Button.Select();
			}
		}

		public void Show(string messageKey)
		{
			base.gameObject.SetActive(value: true);
			option1Button.gameObject.SetActive(value: false);
			option2Button.gameObject.SetActive(value: false);
			loadingIcon.gameObject.SetActive(value: false);
			messageTextAddOn.gameObject.SetActive(value: true);
			option1Button.RectTransform().localPosition = option1SoloPosition;
			messageTextAddOn.AssignAndUpdateText(messageKey, GroupOptions.Text);
			Core.Instance.GameInput.ListenToLastActiveControllerChanged(LastActiveControllerChanged);
		}

		public void Show(string messageKey, string option1TextKey, UnityAction option1Callback)
		{
			base.gameObject.SetActive(value: true);
			option2Button.gameObject.SetActive(value: false);
			loadingIcon.gameObject.SetActive(value: false);
			option1Button.gameObject.SetActive(value: true);
			messageTextAddOn.gameObject.SetActive(value: true);
			option1Button.RectTransform().localPosition = option1SoloPosition;
			option1Button.Select();
			option1Button.OnSelect(null);
			option2Button.DeHighlightText();
			option1TextAddOn.AssignAndUpdateText(option1TextKey, GroupOptions.Text);
			messageTextAddOn.AssignAndUpdateText(messageKey, GroupOptions.Text);
			option1Button.onClick.RemoveAllListeners();
			option1Button.onClick.AddListener(option1Callback);
			Core.Instance.GameInput.ListenToLastActiveControllerChanged(LastActiveControllerChanged);
		}

		public void Show(string messageKey, string option1TextKey, string option2TextKey, UnityAction option1Callback, UnityAction option2Callback, params string[] textToInsert)
		{
			base.gameObject.SetActive(value: true);
			loadingIcon.gameObject.SetActive(value: false);
			option2Button.gameObject.SetActive(value: true);
			option1Button.gameObject.SetActive(value: true);
			messageTextAddOn.gameObject.SetActive(value: true);
			option1Button.RectTransform().localPosition = option1DualPosition;
			option1Button.Select();
			option2Button.DeHighlightText();
			option1TextAddOn.AssignAndUpdateText(option1TextKey, GroupOptions.Text, textToInsert);
			option2TextAddOn.AssignAndUpdateText(option2TextKey, GroupOptions.Text, textToInsert);
			messageTextAddOn.AssignAndUpdateText(messageKey, GroupOptions.Text, textToInsert);
			option1Button.onDelayedButtonClick.RemoveAllListeners();
			option2Button.onDelayedButtonClick.RemoveAllListeners();
			option1Button.onDelayedButtonClick.AddListener(option1Callback);
			option2Button.onDelayedButtonClick.AddListener(option2Callback);
			Core.Instance.GameInput.ListenToLastActiveControllerChanged(LastActiveControllerChanged);
		}

		public void Hide()
		{
			base.gameObject.SetActive(value: false);
			option1Button.onDelayedButtonClick.RemoveAllListeners();
			option2Button.onDelayedButtonClick.RemoveAllListeners();
			Core.Instance.GameInput.StopListenToLastActiveControllerChanged(LastActiveControllerChanged);
		}

		public void OnDestroy()
		{
			Core instance = Core.Instance;
			if ((bool)instance && instance.GameInput != null && instance.IsCoreInitialized)
			{
				instance.GameInput.StopListenToLastActiveControllerChanged(LastActiveControllerChanged);
			}
		}
	}
}
