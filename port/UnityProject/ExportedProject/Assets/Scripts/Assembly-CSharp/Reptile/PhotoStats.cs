namespace Reptile
{
	public class PhotoStats : GenericStats<PhotoEvent, AchievementPhotoObjective>
	{
		public override int GetEventCount()
		{
			return 1;
		}

		public override void InitStatTrackers()
		{
			statTrackers = new StatTracker[1]
			{
				new StatTrackerSUM
				{
					variableToTrack = 0
				}
			};
		}

		public override void UpdateAdditionalStats(PhotoEvent statEvent, AchievementPhotoObjective extraInfo, int value = 1)
		{
		}
	}
}
