using System.IO;

namespace Reptile
{
	public class PhotoObjectProgress : ProgressableData
	{
		private const int PHOTO_OBJECT_PROGRESS_VERSION = 0;

		public int version;

		public bool hasPhotographedObject;

		public PhotoObjectProgress()
		{
			progressableType = ProgressableType.PHOTO_PROGRESS;
		}

		public override void Read(BinaryReader reader)
		{
			version = reader.ReadInt32();
			if (version >= 0)
			{
				uid = reader.ReadString();
				hasPhotographedObject = reader.ReadBoolean();
			}
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(0);
			writer.Write(uid);
			writer.Write(hasPhotographedObject);
		}
	}
}
