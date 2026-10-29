namespace Reptile
{
	public class AchievementsLocal : Achievements
	{
		public AchievementsLocal()
		{
			isLocal = true;
		}

		public override void PlatformClearAchievements()
		{
		}

		public override void PlatformInit(AUser user)
		{
		}

		public override void PlatformRefreshAchievementsCacheThroughServer(RefreshAchievementsCacheThroughServer refreshCallback)
		{
		}

		public override void PlatformRetrieveAchievement(AchievementType type, RetrieveAchievementCallback retrieveCallback)
		{
		}

		public override void PlatformUnlockAchievement(AchievementType type)
		{
		}
	}
}
