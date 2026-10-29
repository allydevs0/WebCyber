using System.IO;

namespace Reptile
{
	public class UnlockableSaveData : ISaveable
	{
		private string uid = string.Empty;

		private bool hasSeen;

		private bool isUnlocked;

		public string Uid => uid;

		public bool HasSeen
		{
			get
			{
				return hasSeen;
			}
			set
			{
				hasSeen = value;
			}
		}

		public bool IsUnlocked
		{
			get
			{
				return isUnlocked;
			}
			set
			{
				isUnlocked = value;
			}
		}

		public UnlockableSaveData(string uID)
		{
			uid = uID;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(uid);
			writer.Write(hasSeen);
			writer.Write(isUnlocked);
		}

		public void Read(BinaryReader reader)
		{
			uid = reader.ReadString();
			hasSeen = reader.ReadBoolean();
			isUnlocked = reader.ReadBoolean();
		}
	}
}
