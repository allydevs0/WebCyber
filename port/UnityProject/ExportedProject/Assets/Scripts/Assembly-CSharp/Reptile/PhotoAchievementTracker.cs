namespace Reptile
{
	public class PhotoAchievementTracker : AchievementsTracker
	{
		private const int PHOTOS_NEEDED = 17;

		private PhotoStats photoStats;

		public PhotoAchievementTracker(PlayerStats playerStats)
			: base(playerStats)
		{
			photoStats = playerStats.photoStats;
			photoStats.RegisterEvent(PhotoEvent.PHOTO_TAKEN, UpdatePhotosAchievement);
		}

		public void UpdatePhotosAchievement(PhotoEvent statEvent, int updatedValue, int deltaValue, AchievementPhotoObjective photoAchievement)
		{
			bool flag = AchievementsTracker.achievements.HasUnlockedAchievement(AchievementType.PhotoGeneric);
			if (updatedValue >= 17 && !flag)
			{
				AchievementsTracker.achievements.UnlockAchievement(AchievementType.PhotoGeneric);
			}
		}
	}
}
