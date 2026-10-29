using System.IO;

namespace Reptile
{
	public class ObjectProgress : ProgressableData
	{
		private const int OBJECT_PROGRESS_VERSION = 1;

		public int version;

		public bool isActive;

		public bool isTriggered;

		public bool isDiscovered;

		public ObjectProgress()
		{
			progressableType = ProgressableType.OBJECT_PROGRESS;
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(1);
			writer.Write(uid);
			writer.Write(isActive);
			writer.Write(isTriggered);
		}

		public override void Read(BinaryReader reader)
		{
			version = reader.ReadInt32();
			if (version >= 0)
			{
				uid = reader.ReadString();
				isActive = reader.ReadBoolean();
				isTriggered = reader.ReadBoolean();
			}
		}

		public void WriteVersionOne(BinaryWriter writer)
		{
			writer.Write(isDiscovered);
		}

		public void ReadVersionOne(BinaryReader reader)
		{
			if (version >= 1)
			{
				isDiscovered = reader.ReadBoolean();
			}
		}
	}
}
