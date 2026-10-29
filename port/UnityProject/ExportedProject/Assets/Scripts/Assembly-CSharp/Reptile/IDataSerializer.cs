using System.IO;

namespace Reptile
{
	public interface IDataSerializer
	{
		void Read<T>(Stream stream, LoadTransaction<T> transaction) where T : class;

		void Write<T>(Stream stream, SaveTransaction<T> transaction) where T : class;
	}
}
