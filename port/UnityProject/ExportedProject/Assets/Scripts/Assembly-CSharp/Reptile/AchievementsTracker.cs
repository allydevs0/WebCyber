namespace Reptile
{
	public abstract class AchievementsTracker
	{
		protected static Achievements achievements;

		protected PlayerStats playerStats;

		public AchievementsTracker(PlayerStats playerStats)
		{
			this.playerStats = playerStats;
			achievements = Core.Instance.Platform.Achievements;
		}
	}
}
