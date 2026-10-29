namespace Reptile
{
	public class StatTrackerSUM : StatTracker
	{
		public override TrackerType GetTrackerType()
		{
			return TrackerType.SUM;
		}

		public override int UpdateValue(int deltaValue)
		{
			return ivalue += deltaValue;
		}
	}
}
