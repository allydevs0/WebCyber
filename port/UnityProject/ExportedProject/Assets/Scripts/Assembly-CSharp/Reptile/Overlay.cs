using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class Overlay : MonoBehaviour
	{
		public GameObject loadingScreen;

		public UIPopup popup;

		public bool IsShowingLoadingScreen => loadingScreen.activeSelf;

		public void Init()
		{
			base.gameObject.SetActive(value: true);
			loadingScreen.SetActive(value: false);
			popup.Init();
		}

		public void ShowLoadingScreen()
		{
			loadingScreen.SetActive(value: true);
		}

		public void HideLoadingScreen()
		{
			loadingScreen.SetActive(value: false);
		}

		public void ShowPopup(string messageKey)
		{
			popup.Show(messageKey);
		}

		public void ShowPopup(string messageKey, string option1TextKey, UIPopup.OnPopupButtonCallback option1Callback, params string[] textsToInsert)
		{
			popup.Show(messageKey, option1TextKey, option1Callback, textsToInsert);
		}

		public void ShowPopup(string messageKey, string option1TextKey, string option2TextKey, UIPopup.OnPopupButtonCallback option1Callback, UIPopup.OnPopupButtonCallback option2Callback, params string[] textsToInsert)
		{
			popup.Show(messageKey, option1TextKey, option2TextKey, option1Callback, option2Callback, textsToInsert);
		}

		public void ShowPopupAsync(string messageKey, string option1TextKey)
		{
			popup.ShowAsync(messageKey, option1TextKey);
		}

		public void ShowPopupAsync(string messageKey, string option1TextKey, string option2TextKey, params string[] textToInsert)
		{
			popup.ShowAsync(messageKey, option1TextKey, option2TextKey, textToInsert);
		}

		public IEnumerator WaitForPopupAction()
		{
			yield return popup.WaitForPopupAction();
		}

		public int GetLastPopupResult()
		{
			return popup.GetLastPopupResult();
		}
	}
}
