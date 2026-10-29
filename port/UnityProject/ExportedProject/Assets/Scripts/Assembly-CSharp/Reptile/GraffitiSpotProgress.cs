using System.IO;

namespace Reptile
{
	public class GraffitiSpotProgress : ProgressableData
	{
		private const int GRAFFITISPOT_PROGRESS_VERSION = 0;

		public int version;

		public bool isOpen = true;

		public Crew topCrew;

		public string topGraffitiArt = string.Empty;

		public Crew bottomCrew;

		public string bottomGraffitiArt = string.Empty;

		public bool firstTime = true;

		public GraffitiSpotProgress()
		{
			progressableType = ProgressableType.GRAFFITI_SPOT_PROGRESS;
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(0);
			writer.Write(uid);
			writer.Write(isOpen);
			writer.Write(topGraffitiArt);
			writer.Write(bottomGraffitiArt);
			writer.Write((int)topCrew);
			writer.Write((int)bottomCrew);
			writer.Write(firstTime);
		}

		public override void Read(BinaryReader reader)
		{
			version = reader.ReadInt32();
			if (version >= 0)
			{
				uid = reader.ReadString();
				isOpen = reader.ReadBoolean();
				topGraffitiArt = reader.ReadString();
				bottomGraffitiArt = reader.ReadString();
				topCrew = (Crew)reader.ReadInt32();
				bottomCrew = (Crew)reader.ReadInt32();
				firstTime = reader.ReadBoolean();
			}
		}
	}
}
