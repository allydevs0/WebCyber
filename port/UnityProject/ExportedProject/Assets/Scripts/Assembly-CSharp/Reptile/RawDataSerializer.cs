using System.IO;
using System.Text;

namespace Reptile
{
	public class RawDataSerializer : ADataSerializer
	{
		public override void Read<T>(Stream stream, LoadTransaction<T> loadTransaction)
		{
			BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
			((ISaveable)loadTransaction.Data).Read(binaryReader);
			binaryReader.Close();
		}

		public override void Write<T>(Stream stream, SaveTransaction<T> saveTransaction)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8);
			((ISaveable)saveTransaction.Data).Write(binaryWriter);
			binaryWriter.Close();
		}
	}
}
