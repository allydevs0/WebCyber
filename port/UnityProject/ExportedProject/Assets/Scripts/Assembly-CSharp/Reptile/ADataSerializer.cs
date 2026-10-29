using System.IO;

namespace Reptile
{
	public abstract class ADataSerializer : IDataSerializer
	{
		public abstract void Read<T>(Stream stream, LoadTransaction<T> transaction) where T : class;

		public abstract void Write<T>(Stream stream, SaveTransaction<T> transaction) where T : class;
	}
}
