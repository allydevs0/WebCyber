using UnityEngine;

namespace Reptile
{
	public interface IPoolable
	{
		PoolableState State { get; set; }

		string PoolableName { get; set; }

		void InitPoolable(Vector3 position, Quaternion rotation);
	}
}
