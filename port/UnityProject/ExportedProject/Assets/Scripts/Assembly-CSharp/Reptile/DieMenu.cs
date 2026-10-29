using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class DieMenu : AMenuController
	{
		public Button resumeBtn;

		public Button quitBtn;

		private BaseModule baseModule;

		public void Init(UIManager uIManager, BaseModule baseModule)
		{
			base.uIManager = uIManager;
			this.baseModule = baseModule;
			InitButtons();
		}

		public void InitButtons()
		{
			resumeBtn.onClick.AddListener(() =>
			{
				uIManager.HideDieMenu();
				Core.Instance.BaseModule.StageManager.RestartStage();
				baseModule.UnPauseGame(PauseType.GameOver);
			});
			quitBtn.onClick.AddListener(() =>
			{
				Core.Instance.BaseModule.QuitCurrentGameSession();
			});
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (uIManager.IsInitialized && base.IsEnabled)
			{
				uIManager.PopCurrentMenuInstant();
			}
			uIManager = null;
			baseModule = null;
		}

		public bool Show()
		{
			if (base.gameObject.activeInHierarchy)
			{
				return true;
			}
			resumeBtn.gameObject.SetActive(value: false);
			quitBtn.gameObject.SetActive(value: false);
			return false;
		}

		public override void Activate()
		{
			base.Activate();
			resumeBtn.gameObject.SetActive(value: true);
			quitBtn.gameObject.SetActive(value: true);
			GameObject selectedGameObject = resumeBtn.gameObject;
			uIManager.SetSelectedGameObject(selectedGameObject);
		}
	}
}
