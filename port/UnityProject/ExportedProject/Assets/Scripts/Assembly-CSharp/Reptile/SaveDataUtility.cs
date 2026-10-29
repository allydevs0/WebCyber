using System.IO;

namespace Reptile
{
	public static class SaveDataUtility
	{
		public static void ConsumeStageProgressVersionThree(BinaryReader reader)
		{
			ConsumeObjectProgressVersionOne(reader);
		}

		public static void ConsumeObjectProgressVersionOne(BinaryReader reader)
		{
			reader.ReadBoolean();
		}
	}
}
