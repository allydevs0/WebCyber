namespace Reptile
{
	public class GraffitiStats : GenericStats<GraffitiEvent, GraffitiSpot>
	{
		public override int GetEventCount()
		{
			return 13;
		}

		public override void InitStatTrackers()
		{
			statTrackers = new StatTracker[13]
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
				},
				new StatTrackerSUM
				{
					variableToTrack = 4
				},
				new StatTrackerSUM
				{
					variableToTrack = 5
				},
				new StatTrackerSUM
				{
					variableToTrack = 6
				},
				new StatTrackerSUM
				{
					variableToTrack = 7
				},
				new StatTrackerSUM
				{
					variableToTrack = 8
				},
				new StatTrackerSUM
				{
					variableToTrack = 9
				},
				new StatTrackerSUM
				{
					variableToTrack = 10
				},
				new StatTrackerSUM
				{
					variableToTrack = 11
				},
				new StatTrackerSUM
				{
					variableToTrack = 12
				}
			};
		}

		public override void UpdateAdditionalStats(GraffitiEvent statEvent, GraffitiSpot extraInfo, int value = 1)
		{
			if (statEvent == GraffitiEvent.GRAFFITISPOT_CLAIMED)
			{
				UpdateStatTracker((int)(1 + extraInfo.size), value);
				UpdateStatTracker((int)(5 + extraInfo.bottomCrew), value);
			}
		}
	}
}
