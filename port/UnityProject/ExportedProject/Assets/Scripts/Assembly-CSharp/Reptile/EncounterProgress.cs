using System.IO;

namespace Reptile
{
	public class EncounterProgress : ProgressableData
	{
		private const int ENCOUNTER_PROGRESS_VERSION = 0;

		public int version;

		public bool canOpen;

		public bool currentlyActive;

		public bool completed;

		public int currentCheckpoint = -1;

		public EncounterProgress()
		{
			progressableType = ProgressableType.ENCOUNTER_PROGRESS;
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(0);
			writer.Write(uid);
			writer.Write(canOpen);
			writer.Write(currentlyActive);
			writer.Write(completed);
			writer.Write(currentCheckpoint);
		}

		public override void Read(BinaryReader reader)
		{
			version = reader.ReadInt32();
			if (version >= 0)
			{
				uid = reader.ReadString();
				canOpen = reader.ReadBoolean();
				currentlyActive = reader.ReadBoolean();
				completed = reader.ReadBoolean();
				currentCheckpoint = reader.ReadInt32();
			}
		}
	}
}
