using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class MainMenuManager : AMenuController
	{
		[SerializeField]
		private Button quitBtn;

		public void Init(BaseModule baseModule, UIManager uIManager, SaveManager saveManager)
		{
			base.uIManager = uIManager;
		}

		public void ShowSaveSlotMenu()
		{
			uIManager.HideMainMenu();
			uIManager.ShowSlotsMenu();
		}

		public void ShowOptionsMenu()
		{
			uIManager.HideMainMenu();
			uIManager.ShowOptionsMenu();
		}

		public void QuitGame()
		{
			ApplicationUtility.QuitApplication();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		private void HideQuitButton()
		{
			quitBtn.gameObject.SetActive(value: false);
		}
	}
}
