using System;
using Rewired.Integration.UnityUI;
using UnityEngine;

namespace Reptile
{
	public class PlatformWeb : APlatform
	{
		private RewiredStandaloneInputModule rewiredStandaloneInputModule;
		private GameInput gameInput;

		public PlatformWeb()
		{
			platformName = "Web";
			storage = new StorageWeb(Application.persistentDataPath);
			user = new UserWeb();
			achievements = new AchievementsLocal();
			downloadableContent = new DownloadableContentWindows();
			errorHandler = new RegularErrorHandler(Core.Instance.errorQueuePrefab, storage);
		}

		public override void Init(SaveManager saveManager, RewiredStandaloneInputModule rewiredStandAloneInputModule, PlatformData platformData, GameInput gameInput, UIManager uiManager, AudioManager audioManager, IGameTextLocalizer localizer, AchievementLibrary achievementLibrary, DownloadableContentLibrary downloadableContentLibrary)
		{
			base.Init(saveManager, rewiredStandAloneInputModule, platformData, gameInput, uiManager, audioManager, localizer, achievementLibrary, downloadableContentLibrary);
			this.gameInput = gameInput;
		}

		public override void HandleExceptionInGame(Exception exception)
		{
			Debug.LogException(exception);
		}

		protected override void SetRewiredEventSystemParameters(RewiredStandaloneInputModule rewiredStandaloneInputModule)
		{
			this.rewiredStandaloneInputModule = rewiredStandaloneInputModule;
			if (this.rewiredStandaloneInputModule != null)
			{
				this.rewiredStandaloneInputModule.allowMouseInput = true;
				this.rewiredStandaloneInputModule.allowMouseInputIfTouchSupported = true;
				this.rewiredStandaloneInputModule.allowTouchInput = false;
			}
		}
	}
}
