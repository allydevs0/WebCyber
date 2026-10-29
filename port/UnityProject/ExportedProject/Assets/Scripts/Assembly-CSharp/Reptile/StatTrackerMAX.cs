namespace Reptile
{
	public class StatTrackerMAX : StatTracker
	{
		public override TrackerType GetTrackerType()
		{
			return TrackerType.MAX;
		}

		public override int UpdateValue(int deltaValue)
		{
			if (ivalue > deltaValue)
			{
				return ivalue;
			}
			return ivalue = deltaValue;
		}
	}
}
