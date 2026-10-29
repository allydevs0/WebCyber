using System.IO;

namespace Reptile
{
	public class PhonePictureData : ISaveable
	{
		private byte[] pictureData;

		internal string pictureFilename = string.Empty;

		public PhonePictureData(byte[] pictureData)
		{
			this.pictureData = pictureData;
		}

		public byte[] GetPictureData()
		{
			return pictureData;
		}

		public void Read(BinaryReader reader)
		{
			int num = 0;
			int num2 = 4;
			byte[] array = new byte[num2];
			do
			{
				array = reader.ReadBytes(num2);
				int num3 = array.Length;
				for (int i = 0; i < num3; i++)
				{
					pictureData[num + i] = array[i];
				}
				num += num2;
			}
			while (array.Length != 0);
			byte[] array2 = new byte[num];
			for (int j = 0; j < num; j++)
			{
				array2[j] = pictureData[j];
			}
			pictureData = array2;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(pictureData);
		}
	}
}
