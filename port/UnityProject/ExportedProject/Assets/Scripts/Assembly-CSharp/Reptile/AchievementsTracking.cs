namespace Reptile
{
	public class AchievementsTracking
	{
		public AchievementsTracker[] achievementsTrackers;

		public AchievementsTracking(PlayerStats playerStats)
		{
			achievementsTrackers = new AchievementsTracker[3]
			{
				new GraffitiAchievementsTracker(playerStats),
				new BoosterAchievementsTracker(playerStats),
				new PhotoAchievementTracker(playerStats)
			};
		}
	}
}
