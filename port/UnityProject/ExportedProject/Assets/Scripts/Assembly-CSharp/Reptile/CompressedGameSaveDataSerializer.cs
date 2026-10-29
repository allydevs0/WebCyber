using System.IO;
using System.IO.Compression;

namespace Reptile
{
	public class CompressedGameSaveDataSerializer : ADataSerializer
	{
		private byte[] oneByteArray = new byte[1];

		public override void Read<T>(Stream stream, LoadTransaction<T> loadTransaction)
		{
			EnsureFormat(loadTransaction.FileName, stream, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
			BinaryReader reader = new BinaryReader(gZipStream);
			((ISaveable)loadTransaction.Data).Read(reader);
			gZipStream.Close();
		}

		private void EnsureFormat(string fileName, Stream stream, DataSerializerFactory.FormatType expectedFormat)
		{
			DataSerializerFactory.FormatType formatType = (DataSerializerFactory.FormatType)stream.ReadByte();
			if (formatType != expectedFormat)
			{
				throw new IncorrectFormatException(fileName, formatType);
			}
		}

		public override void Write<T>(Stream stream, SaveTransaction<T> saveTransaction)
		{
			oneByteArray[0] = 2;
			stream.Write(oneByteArray, 0, 1);
			GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress);
			BinaryWriter writer = new BinaryWriter(gZipStream);
			((ISaveable)saveTransaction.Data).Write(writer);
			gZipStream.Close();
		}
	}
}
