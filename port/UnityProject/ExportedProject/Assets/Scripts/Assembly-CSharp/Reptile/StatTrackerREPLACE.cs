namespace Reptile
{
	public class StatTrackerREPLACE : StatTracker
	{
		public override TrackerType GetTrackerType()
		{
			return TrackerType.REPLACE;
		}

		public override int UpdateValue(int deltaValue)
		{
			return ivalue = deltaValue;
		}
	}
}
