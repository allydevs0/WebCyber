using System.IO;

namespace Reptile
{
	public class StageTransitionProgress : ProgressableData
	{
		private const int STAGE_TRANSITION_PROGRESS_VERSION = 0;

		public int version;

		public bool isActive;

		public bool allowEntry;

		public StageTransitionProgress()
		{
			progressableType = ProgressableType.STAGE_TRANSITION_PROGRESS;
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(0);
			writer.Write(uid);
			writer.Write(isActive);
			writer.Write(allowEntry);
		}

		public override void Read(BinaryReader reader)
		{
			version = reader.ReadInt32();
			if (version >= 0)
			{
				uid = reader.ReadString();
				isActive = reader.ReadBoolean();
				allowEntry = reader.ReadBoolean();
			}
		}
	}
}
