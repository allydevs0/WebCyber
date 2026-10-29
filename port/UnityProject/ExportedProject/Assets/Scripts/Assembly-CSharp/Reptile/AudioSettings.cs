using System.IO;

namespace Reptile
{
	public class AudioSettings : ISaveable
	{
		private const int AUDIO_SETTINGS_VERSION = 0;

		public float masterVolume = 0.5f;

		public float musicVolume = 0.5f;

		public float sfxVolume = 0.5f;

		public void Write(BinaryWriter writer)
		{
			writer.Write(0);
			writer.Write(masterVolume);
			writer.Write(musicVolume);
			writer.Write(sfxVolume);
		}

		public void Read(BinaryReader reader)
		{
			if (reader.ReadInt32() >= 0)
			{
				masterVolume = reader.ReadSingle();
				musicVolume = reader.ReadSingle();
				sfxVolume = reader.ReadSingle();
			}
		}
	}
}
