namespace Reptile
{
	public class BoosterStats : GenericStats<BoosterEvent, Pickup>
	{
		public override int GetEventCount()
		{
			return 4;
		}

		public override void InitStatTrackers()
		{
			statTrackers = new StatTracker[4]
			{
				new StatTrackerSUM
				{
					variableToTrack = 0
				},
				new StatTrackerSUM
				{
					variableToTrack = 1
				},
				new StatTrackerSUM
				{
					variableToTrack = 2
				},
				new StatTrackerSUM
				{
					variableToTrack = 3
				}
			};
		}

		public override void UpdateAdditionalStats(BoosterEvent statEvent, Pickup extraInfo, int value = 1)
		{
			UpdateStatTracker((int)(1 + extraInfo.pickupType), value);
		}
	}
}
