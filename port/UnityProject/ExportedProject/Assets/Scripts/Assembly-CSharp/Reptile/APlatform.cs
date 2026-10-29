using System;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine.Diagnostics;

namespace Reptile
{
	public abstract class APlatform
	{
		protected Achievements achievements;

		protected AStorage storage;

		protected AUser user;

		protected AErrorHandler errorHandler;

		protected PlatformData platformData;

		protected ADownloadableContent downloadableContent;

		protected string platformName = string.Empty;

		protected Guid platformDefaultControllerGuid = Guid.Empty;

		public AStorage Storage => storage;

		public AUser User => user;

		public Achievements Achievements => achievements;

		public AErrorHandler ErrorHandler
		{
			get
			{
				if (errorHandler == null)
				{
					errorHandler = new RegularErrorHandler(Core.Instance.errorQueuePrefab, storage);
				}
				return errorHandler;
			}
		}

		public ADownloadableContent DownloadableContent => downloadableContent;

		public string PlatformName => platformName;

		public Guid PlatformDefaultControllerGuid => platformDefaultControllerGuid;

		public ControllerType PlatformDefaultControllerType => platformData.defaultControllerType;

		public bool HasAchievementsSupport => achievements != null;

		public virtual void Init(SaveManager saveManager, RewiredStandaloneInputModule rewiredStandAloneInputModule, PlatformData platformData, GameInput gameInput, UIManager uiManager, AudioManager audioManager, IGameTextLocalizer localizer, AchievementLibrary achievementLibrary, DownloadableContentLibrary downloadableContentLibrary)
		{
			this.platformData = platformData;
			platformDefaultControllerGuid = new Guid(platformData.defaultControllerGuid);
			if (HasAchievementsSupport)
			{
				achievements.InitAchievements(saveManager, achievementLibrary);
			}
			downloadableContent.Init(downloadableContentLibrary);
			user.InitUser(saveManager, storage, gameInput, errorHandler, uiManager, audioManager, localizer, achievements, downloadableContent);
			SetRewiredEventSystemParameters(rewiredStandAloneInputModule);
		}

		public virtual void Update()
		{
			if (!storage.IsAllowedToDoFileOperations && !storage.IsDisposed)
			{
				Utils.ForceCrash(ForcedCrashCategory.FatalError);
			}
		}

		public static APlatform CreatePlatformInstance()
		{
			return new PlatformWeb();
		}

		public AGameDebug CreateDebugInstanceForPlatform(BaseModule baseModule, SaveManager saveManager, GameInput gameInput, UIManager uIManager)
		{
			return new GameDebugDummy(baseModule, saveManager, gameInput, uIManager);
		}

		public virtual void Dispose()
		{
			storage.Dispose();
			achievements?.Dispose();
			errorHandler.Dispose();
			errorHandler = null;
			storage = null;
			user = null;
			achievements = null;
		}

		public abstract void HandleExceptionInGame(Exception exception);

		protected abstract void SetRewiredEventSystemParameters(RewiredStandaloneInputModule rewiredStandaloneInputModule);
	}
}
