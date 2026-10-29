namespace Reptile
{
	public class GraffitiAchievementsTracker : AchievementsTracker
	{
		private GraffitiStats graffitiStats;

		public GraffitiAchievementsTracker(PlayerStats playerStats)
			: base(playerStats)
		{
			graffitiStats = playerStats.graffitiStats;
			graffitiStats.RegisterEvent(GraffitiEvent.GRAFFITISPOT_CLAIMED, UpdateGraffitiAchievements);
		}

		public void UpdateGraffitiAchievements(GraffitiEvent statEvent, int updatedValue, int deltaValue, GraffitiSpot graffitiSpot)
		{
		}
	}
}
