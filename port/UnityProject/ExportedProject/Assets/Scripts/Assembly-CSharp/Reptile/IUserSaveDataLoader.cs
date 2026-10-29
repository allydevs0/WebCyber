using System.Collections;

namespace Reptile
{
	public interface IUserSaveDataLoader
	{
		bool IsSuccess { get; }

		IEnumerator LoadSaveDataASync();

		void ResetLoader();
	}
}
