using System.IO;

namespace Reptile
{
	public interface ISaveable
	{
		void Write(BinaryWriter writer);

		void Read(BinaryReader reader);
	}
}
