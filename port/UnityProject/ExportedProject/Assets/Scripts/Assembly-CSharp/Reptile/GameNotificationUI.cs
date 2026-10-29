using Rewired;
using TMPro;

namespace Reptile
{
	public class GameNotificationUI : AMenuController
	{
		public TextMeshProUGUI messageText;

		private GameInput gameInput;

		private BaseModule baseModule;

		private IGameTextLocalizer localizer;

		private string currentLocalizationKey = string.Empty;

		private string[] currentTextsToInsert;

		public void Init(GameInput gameInput, BaseModule baseModule, IGameTextLocalizer localizer, UIManager uIManager)
		{
			this.gameInput = gameInput;
			this.baseModule = baseModule;
			this.localizer = localizer;
			base.uIManager = uIManager;
		}

		public void SetNotificationText(string localizationKey, params string[] textsToInsert)
		{
			currentLocalizationKey = localizationKey;
			currentTextsToInsert = textsToInsert;
			string notificationText = localizer.GetNotificationText(currentLocalizationKey, currentTextsToInsert);
			messageText.text = notificationText;
			uIManager.UpdateTextMeshProCurrentControllerButtonGlyph(messageText);
		}

		public void SetNotificationText(NotificationTextAsset textAsset, params string[] textsToInsert)
		{
			SetNotificationText(textAsset.localizationKey, textsToInsert);
		}

		public override void Activate()
		{
			base.Activate();
			EnableMenuInput();
			gameInput.ListenToLastActiveControllerChanged(OnActiveControllerChanged);
		}

		public override void Deactivate()
		{
			base.Deactivate();
			if (!uIManager.IsShowingAnyMenu && baseModule.IsPlayingInStage)
			{
				baseModule.StageManager.RestoreCurrentPlayerInput();
			}
			gameInput.StopListenToLastActiveControllerChanged(OnActiveControllerChanged);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (base.IsEnabled && uIManager != null && uIManager.IsShowingAnyMenu)
			{
				uIManager.PopCurrentMenuInstant();
			}
			if (gameInput != null)
			{
				gameInput.StopListenToLastActiveControllerChanged(OnActiveControllerChanged);
			}
		}

		private void EnableMenuInput()
		{
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMap(1);
		}

		private void Update()
		{
			if (gameInput.GetButtonNew(2) && base.IsEnabled)
			{
				DismissNotification();
			}
		}

		private void DismissNotification()
		{
			uIManager.PopCurrentMenuInstant();
		}

		private void OnActiveControllerChanged(int playerid, ControllerType controllerType)
		{
			SetNotificationText(currentLocalizationKey, currentTextsToInsert);
		}
	}
}
