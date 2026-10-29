using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reptile
{
	public class UIPopup : AMenuController
	{
		public delegate void OnPopupButtonCallback();

		public const string UI_POPUP_BUTTON_OK_LOCALIZATION_KEY = "BT_OK";

		public const string UI_POPUP_BUTTON_CANCEL_LOCALIZATION_KEY = "BT_CANCEL";

		[SerializeField]
		private Button option1Button;

		[SerializeField]
		private Button option2Button;

		[SerializeField]
		private TMProLocalizationAddOn option1TextAddOn;

		[SerializeField]
		private TMProLocalizationAddOn option2TextAddOn;

		[SerializeField]
		private TMProLocalizationAddOn messageTextAddOn;

		[SerializeField]
		private RectTransform option1RectTransform;

		[SerializeField]
		private RectTransform loadingIconRectTransform;

		[SerializeField]
		private RectTransform soloButtonTransform;

		[SerializeField]
		private RectTransform dualButtonTransform;

		private OnPopupButtonCallback onOption1Callback;

		private OnPopupButtonCallback onOption2Callback;

		private char promptResult;

		private bool waitForSceneLoadingDone;

		private int coreBuildIndex;

		private GameInput gameInput;

		internal static bool waitForPopupToFinish;

		public void Init()
		{
			gameInput = Core.Instance.GameInput;
			Deactivate();
			option1Button.onClick.AddListener(Option1Callback);
			option2Button.onClick.AddListener(Option2Callback);
			SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
			coreBuildIndex = SceneManager.GetSceneByPath(Core.Instance.sceneLibrary.coreScenePath).buildIndex;
		}

		private void SceneManager_activeSceneChanged(Scene current, Scene next)
		{
			if (waitForSceneLoadingDone)
			{
				waitForSceneLoadingDone = false;
			}
			else if (next.buildIndex == coreBuildIndex)
			{
				waitForSceneLoadingDone = true;
				if (base.gameObject.activeInHierarchy)
				{
					waitForPopupToFinish = true;
				}
			}
		}

		public void ShowLoading()
		{
			base.gameObject.SetActive(value: true);
			option1Button.gameObject.SetActive(value: false);
			option2Button.gameObject.SetActive(value: false);
			messageTextAddOn.gameObject.SetActive(value: false);
			loadingIconRectTransform.gameObject.SetActive(value: true);
			if (!base.IsEnabled)
			{
				UIManager uIManager = Core.Instance.UIManager;
				if (uIManager != null)
				{
					uIManager.PushNewMenu(this);
				}
			}
		}

		public void Show(string messageKey)
		{
			promptResult = '\0';
			base.gameObject.SetActive(value: true);
			option1Button.gameObject.SetActive(value: false);
			option2Button.gameObject.SetActive(value: false);
			messageTextAddOn.gameObject.SetActive(value: true);
			loadingIconRectTransform.gameObject.SetActive(value: false);
			option1RectTransform.localPosition = soloButtonTransform.position;
			messageTextAddOn.AssignAndUpdateText(messageKey, GroupOptions.Text);
			if (!base.IsEnabled)
			{
				UIManager uIManager = Core.Instance.UIManager;
				if (uIManager != null)
				{
					uIManager.PushNewMenu(this);
				}
			}
		}

		public void ShowAsync(string messageKey, string option1TextKey, params string[] textsToInsert)
		{
			if (waitForSceneLoadingDone)
			{
				waitForPopupToFinish = true;
			}
			promptResult = '\u0080';
			base.gameObject.SetActive(value: true);
			option2Button.gameObject.SetActive(value: false);
			option1Button.gameObject.SetActive(value: true);
			messageTextAddOn.gameObject.SetActive(value: true);
			loadingIconRectTransform.gameObject.SetActive(value: false);
			option1RectTransform.localPosition = soloButtonTransform.localPosition;
			option1Button.Select();
			option1TextAddOn.AssignAndUpdateText(option1TextKey, GroupOptions.Text);
			messageTextAddOn.AssignAndUpdateText(messageKey, GroupOptions.Text, textsToInsert);
			SetupPauseCore();
		}

		public void Show(string messageKey, string option1TextKey, OnPopupButtonCallback option1Callback, params string[] textsToInsert)
		{
			ShowAsync(messageKey, option1TextKey, textsToInsert);
			promptResult = '\0';
			onOption1Callback = option1Callback;
			if (!base.IsEnabled)
			{
				UIManager uIManager = Core.Instance.UIManager;
				if (uIManager != null)
				{
					uIManager.PushNewMenu(this);
				}
			}
		}

		public void ShowAsync(string messageKey, string option1TextKey, string option2TextKey, params string[] textsToInsert)
		{
			if (waitForSceneLoadingDone)
			{
				waitForPopupToFinish = true;
			}
			promptResult = '\u0080';
			base.gameObject.SetActive(value: true);
			option2Button.gameObject.SetActive(value: true);
			option1Button.gameObject.SetActive(value: true);
			messageTextAddOn.gameObject.SetActive(value: true);
			loadingIconRectTransform.gameObject.SetActive(value: false);
			option1RectTransform.localPosition = dualButtonTransform.localPosition;
			option1Button.Select();
			option1TextAddOn.AssignAndUpdateText(option1TextKey, GroupOptions.Text);
			option2TextAddOn.AssignAndUpdateText(option2TextKey, GroupOptions.Text);
			messageTextAddOn.AssignAndUpdateText(messageKey, GroupOptions.Text, textsToInsert);
			SetupPauseCore();
		}

		public void Show(string messageKey, string option1TextKey, string option2TextKey, OnPopupButtonCallback option1Callback, OnPopupButtonCallback option2Callback, params string[] textsToInsert)
		{
			ShowAsync(messageKey, option1TextKey, option2TextKey, textsToInsert);
			promptResult = '\0';
			onOption1Callback = option1Callback;
			onOption2Callback = option2Callback;
			if (!base.IsEnabled)
			{
				UIManager uIManager = Core.Instance.UIManager;
				if (uIManager != null)
				{
					uIManager.PushNewMenu(this);
				}
			}
		}

		public void Hide()
		{
			if (base.IsEnabled)
			{
				UIManager uIManager = Core.Instance.UIManager;
				if (uIManager != null)
				{
					uIManager.PopCurrentMenu();
				}
				else
				{
					Deactivate();
				}
				waitForPopupToFinish = false;
			}
		}

		public IEnumerator WaitForPopupAction()
		{
			while ((promptResult & 0x7FFF) == 0)
			{
				yield return null;
			}
		}

		public int GetLastPopupResult()
		{
			return promptResult & 0x7FFF;
		}

		protected void Option1Callback()
		{
			if ((promptResult & 0x80) == 0)
			{
				if (!loadingIconRectTransform.gameObject.activeSelf)
				{
					Hide();
				}
				onOption1Callback();
				onOption1Callback = null;
				onOption2Callback = null;
			}
			promptResult = (char)((promptResult & 0x7F) | 1);
			waitForPopupToFinish = false;
		}

		protected void Option2Callback()
		{
			if ((promptResult & 0x80) == 0)
			{
				if (!loadingIconRectTransform.gameObject.activeSelf)
				{
					Hide();
				}
				onOption2Callback();
				onOption1Callback = null;
				onOption2Callback = null;
			}
			promptResult = (char)((promptResult & 0x7F) | 2);
			waitForPopupToFinish = false;
		}

		private void SetupPauseCore()
		{
			Core instance = Core.Instance;
			if (instance.BaseModule.IsPlayingInStage)
			{
				instance.BaseModule.PauseGame(PauseType.Prompt);
			}
			else
			{
				instance.PauseCore(PauseType.Prompt);
			}
		}

		private void SetupUnpauseCore()
		{
			Core instance = Core.Instance;
			if (instance.BaseModule.IsPlayingInStage)
			{
				instance.BaseModule.UnPauseGame(PauseType.Prompt);
				return;
			}
			instance.UnPauseCore(PauseType.Prompt);
			instance.BaseModule.RestoreMouseInputState();
		}

		public override GameObject GetFirstFocusGameObject()
		{
			if (option1Button.gameObject.activeInHierarchy)
			{
				return option1Button.gameObject;
			}
			return null;
		}

		public override void Activate()
		{
			base.Activate();
			SetupPauseCore();
		}

		public override void Deactivate()
		{
			base.Deactivate();
			SetupUnpauseCore();
			UIManager uIManager = Core.Instance.UIManager;
			if (uIManager != null)
			{
				uIManager.RemoveMenuIfJustPopped(this);
			}
		}
	}
}
