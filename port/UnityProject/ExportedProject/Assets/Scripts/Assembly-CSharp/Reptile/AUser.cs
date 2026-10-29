using System.Collections;
using UnityEngine;

namespace Reptile
{
	public abstract class AUser
	{
		private const int USER_SAVE_DATA_LOADER_COUNT = 5;

		protected SaveManager saveManager;

		protected IUserId userId;

		internal bool isLoggedIn;

		protected VideoSettingsManager videoSettingsManager;

		protected AudioManager audioManager;

		protected AStorage storage;

		protected IGameTextLocalizer localizer;

		protected ADownloadableContent downloadableContent;

		private GameInput gameInput;

		private AErrorHandler errorHandler;

		private UIManager uIManager;

		private IUserSaveDataLoader[] userSaveDataLoaders;

		private Achievements achievements;

		public bool IsLoggedIn => isLoggedIn;

		public string UserName => userId.UserName;

		public string UserIdentifier => userId.Identifier.ToString();

		public IUserId UserId => userId;

		public VideoSettingsManager VideoSettingsManager => videoSettingsManager;

		public void InitUser(SaveManager saveManager, AStorage storage, GameInput gameInput, AErrorHandler errorHandler, UIManager uIManager, AudioManager audioManager, IGameTextLocalizer localizer, Achievements achievements, ADownloadableContent downloadableContent)
		{
			this.saveManager = saveManager;
			this.storage = storage;
			this.gameInput = gameInput;
			this.errorHandler = errorHandler;
			this.uIManager = uIManager;
			this.audioManager = audioManager;
			this.localizer = localizer;
			this.achievements = achievements;
			this.downloadableContent = downloadableContent;
			userId = GetDefaultUserID();
			CreateUserSaveDataLoaders();
		}

		public abstract SystemLanguage GetUserLanguage();

		public IEnumerator LoginASync()
		{
			yield return LoginUserASync();
			if (isLoggedIn)
			{
				HandleLoginSucces();
				yield return downloadableContent.CheckDownloadableContentLicenses();
			}
		}

		public void LogOut()
		{
			storage.PlatformClearUserSaveData(userId);
			downloadableContent.ClearDownloadableContent();
			LogoutUser();
			ResetUser();
			isLoggedIn = false;
		}

		public virtual void SetUserSettings()
		{
			SetUserAudioSettings();
			SetUserInputSettings();
			SetUserGameSettings();
		}

		public UnlockableSaveData GetUnlockableSaveDataFor(AUnlockable unlockable)
		{
			UnlockableSaveData unlockableDataByUid = saveManager.CurrentSaveSlot.GetUnlockableDataByUid(unlockable.Uid);
			if (unlockable.IsDefault)
			{
				unlockableDataByUid.IsUnlocked = true;
			}
			return unlockableDataByUid;
		}

		public IEnumerator LoadUserSaveData()
		{
			for (int i = 0; i < userSaveDataLoaders.Length; i++)
			{
				userSaveDataLoaders[i].ResetLoader();
				yield return userSaveDataLoaders[i].LoadSaveDataASync();
				if (!userSaveDataLoaders[i].IsSuccess)
				{
					LogOut();
					break;
				}
			}
		}

		protected abstract IEnumerator LoginUserASync();

		protected abstract void LogoutUser();

		protected abstract IUserId GetDefaultUserID();

		protected virtual void ResetUser()
		{
			if (achievements != null)
			{
				achievements.ResetAchievementLibrary();
			}
			ResetUserSaveData();
		}

		protected void SetUserVideoSettings()
		{
			if (videoSettingsManager != null)
			{
				videoSettingsManager.InitializeVideoSettings(saveManager.Settings.videoSettings);
			}
		}

		protected virtual void SetUserGameSettings()
		{
			localizer.UpdateLocalization(GetUserLanguage());
		}

		private void CreateUserSaveDataLoaders()
		{
			int num = DetermineLoaderCount();
			userSaveDataLoaders = new IUserSaveDataLoader[num];
			userSaveDataLoaders[0] = new UserSettingsSaveDataLoader(saveManager, storage, errorHandler, uIManager, this, localizer);
			userSaveDataLoaders[1] = new UserSaveSlotSaveDataLoader(saveManager, storage, errorHandler, uIManager, localizer);
			userSaveDataLoaders[2] = new UserSaveSlotSettingsDataLoader(saveManager, storage, errorHandler, uIManager, localizer);
			userSaveDataLoaders[3] = new UserPlayerStatsLoader(saveManager, storage, errorHandler, uIManager, localizer);
			if (Core.Instance.Platform.HasAchievementsSupport)
			{
				userSaveDataLoaders[4] = new UserAchievementsCacheLoader(saveManager, storage, errorHandler, uIManager, localizer);
			}
		}

		private int DetermineLoaderCount()
		{
			int num = 5;
			if (!Core.Instance.Platform.HasAchievementsSupport)
			{
				num--;
			}
			return num;
		}

		private void HandleLoginSucces()
		{
			userId = GetDefaultUserID();
			InitializeUserData();
		}

		private void InitializeUserData()
		{
			if (!storage.PlatformInitUserSaveData(userId))
			{
				LogOut();
			}
		}

		private void ResetUserSaveData()
		{
			saveManager.ResetSettings();
			saveManager.ResetSaveSlotSettings();
			saveManager.ResetSaveSlots();
			saveManager.ResetPlayerStats();
			saveManager.ResetAchievementsCache();
			SetUserVideoSettings();
		}

		private void SetUserAudioSettings()
		{
			audioManager.SetAudioSettings(saveManager.Settings.audioSettings);
		}

		private void SetUserInputSettings()
		{
			gameInput.ApplyInputSettings(saveManager.Settings.inputSettings);
		}
	}
}
