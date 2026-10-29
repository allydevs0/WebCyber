namespace Reptile
{
	public class StatTrackerMIN : StatTracker
	{
		public override TrackerType GetTrackerType()
		{
			return TrackerType.MIN;
		}

		public override int UpdateValue(int deltaValue)
		{
			if (ivalue < deltaValue)
			{
				return ivalue;
			}
			return ivalue = deltaValue;
		}
	}
}
