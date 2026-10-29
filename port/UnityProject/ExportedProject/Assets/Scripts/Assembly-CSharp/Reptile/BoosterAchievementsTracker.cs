namespace Reptile
{
	public class BoosterAchievementsTracker : AchievementsTracker
	{
		private BoosterStats boosterStats;

		public BoosterAchievementsTracker(PlayerStats playerStats)
			: base(playerStats)
		{
			boosterStats = playerStats.boosterStats;
			boosterStats.RegisterEvent(BoosterEvent.BOOSTERS_PICKEDUP, UpdateBoosterAchievements);
		}

		public void UpdateBoosterAchievements(BoosterEvent statEvent, int updatedValue, int deltaValue, Pickup pickup)
		{
		}
	}
}
