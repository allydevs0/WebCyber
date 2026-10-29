using System.Collections;

namespace Reptile
{
	public abstract class Achievements
	{
		public delegate void RetrieveAchievementCallback(AchievementType type, bool achievementUnlocked);

		public delegate void RefreshAchievementsCacheThroughServer(bool isUptodate);

		private AchievementsTracking tracking;

		protected AchievementLibrary achievementLibrary;

		public const int Version = 0;

		protected AsyncState asyncState;

		protected bool isLocal;

		protected SaveManager saveManager;

		public bool IsLocal => isLocal;

		public abstract void PlatformInit(AUser user);

		public abstract void PlatformUnlockAchievement(AchievementType type);

		public abstract void PlatformRetrieveAchievement(AchievementType type, RetrieveAchievementCallback retrieveCallback);

		public abstract void PlatformRefreshAchievementsCacheThroughServer(RefreshAchievementsCacheThroughServer refreshCallback);

		public abstract void PlatformClearAchievements();

		public void InitAchievements(SaveManager saveManager, AchievementLibrary achievementLibrary)
		{
			this.saveManager = saveManager;
			this.achievementLibrary = achievementLibrary;
			ResetAchievementLibrary();
			Core.Instance.StartCoroutine(WaitForUserLogin());
		}

		public void ResetAchievementLibrary()
		{
			achievementLibrary.ResetAchievements();
		}

		public bool HasUnlockedAchievement(AchievementType type)
		{
			if (type == AchievementType.NONE || type == AchievementType.NULL || type == AchievementType.MAX)
			{
				return false;
			}
			return achievementLibrary.HasUnlockedAchievement((int)type);
		}

		public bool HasUnlockedAchievement(Achievement achievement)
		{
			AchievementType achievementType = achievementLibrary.FindAchievementType(achievement);
			if (achievementType == AchievementType.NULL)
			{
				return false;
			}
			return HasUnlockedAchievement(achievementType);
		}

		private IEnumerator WaitForUserLogin()
		{
			while (Core.Instance.BaseModule == null)
			{
				yield return null;
			}
			Core.Instance.BaseModule.OnUserLoginFinished += BaseModule_OnUserLoginFinished;
		}

		public virtual void Dispose()
		{
			Core.Instance.BaseModule.OnUserLoginFinished -= BaseModule_OnUserLoginFinished;
		}

		private void BaseModule_OnUserLoginFinished(bool success, AUser user)
		{
			if (success)
			{
				AchievementsInit(user);
			}
		}

		public void AchievementsInit(AUser user)
		{
			tracking = new AchievementsTracking(saveManager.PlayerStats);
			asyncState = AsyncState.None;
			PlatformInit(user);
		}

		public void UnlockAchievement(AchievementType achievementType)
		{
			if (achievementType != AchievementType.NULL && achievementType != AchievementType.NONE && achievementType != AchievementType.MAX)
			{
				achievementLibrary.GetAchievement((int)achievementType).UnlockAchievement();
				PlatformUnlockAchievement(achievementType);
			}
		}

		public void UnlockAchievement(Achievement achievement)
		{
			AchievementType achievementType = achievementLibrary.FindAchievementType(achievement);
			if (achievementType != AchievementType.NULL)
			{
				achievement.UnlockAchievement();
				PlatformUnlockAchievement(achievementType);
			}
		}

		public IEnumerator RetrieveAchievement(AchievementType type, RetrieveAchievementCallback retrieveCallback)
		{
			if (type != AchievementType.NULL && type != AchievementType.NONE && type != AchievementType.MAX)
			{
				while (asyncState == AsyncState.RunningTask)
				{
					yield return null;
				}
				asyncState = AsyncState.RunningTask;
				PlatformRetrieveAchievement(type, retrieveCallback);
				while (asyncState == AsyncState.RunningTask)
				{
					yield return null;
				}
			}
		}

		public AchievementType GetAchievementType(Achievement achievement)
		{
			return achievementLibrary.FindAchievementType(achievement);
		}

		public void ClearAchievements()
		{
			achievementLibrary.ResetAchievements();
			PlatformClearAchievements();
		}

		public StageAchievements GetStageAchievements(Stage stage)
		{
			return achievementLibrary.GetStageAchievements(stage);
		}

		public Stage[] GetStagesWithGraffitiAchievement()
		{
			return achievementLibrary.GetStagesContainingGraffitiAchievements();
		}
	}
}
