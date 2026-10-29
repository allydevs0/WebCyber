using System.IO;

namespace Reptile
{
	public class SaveSettingsData : ISaveable
	{
		private const int SAVE_SETTINGS_VERSION = 0;

		public const int NO_SAVE_SLOT = -1;

		public int currentSaveSlot = -1;

		public void Write(BinaryWriter writer)
		{
			writer.Write(0);
			writer.Write(currentSaveSlot);
		}

		public void Read(BinaryReader reader)
		{
			if (reader.ReadInt32() >= 0)
			{
				currentSaveSlot = reader.ReadInt32();
			}
		}
	}
}
