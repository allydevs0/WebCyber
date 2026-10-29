using System.IO;

namespace Reptile
{
	public abstract class StatTracker : ISaveable
	{
		public int variableToTrack;

		public int ivalue;

		public abstract TrackerType GetTrackerType();

		public abstract int UpdateValue(int deltaValue);

		public StatTrackerInfo GetTrackerInfo()
		{
			return new StatTrackerInfo
			{
				variableToTrack = variableToTrack,
				ivalue = ivalue,
				trackerType = GetTrackerType()
			};
		}

		public virtual void Write(BinaryWriter writer)
		{
			writer.Write((int)GetTrackerType());
			writer.Write(variableToTrack);
			writer.Write(ivalue);
		}

		public virtual void Read(BinaryReader reader)
		{
			variableToTrack = reader.ReadInt32();
			ivalue = reader.ReadInt32();
		}
	}
}
