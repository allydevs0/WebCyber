using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class PoolableInstantiator : MonoBehaviour
	{
		private List<IPoolable> poolables = new List<IPoolable>();

		public IPoolable GetPoolableFromPoolOrInstantiate(PoolableObjectToName poolableObject)
		{
			IPoolable poolable = null;
			for (int i = 0; i < poolables.Count; i++)
			{
				if (poolables[i].PoolableName == poolableObject.name && poolables[i].State == PoolableState.Pooled)
				{
					return poolables[i];
				}
			}
			if (poolable == null)
			{
				poolable = Object.Instantiate(poolableObject.obj).GetComponent<IPoolable>();
				poolable.PoolableName = poolableObject.name;
				poolables.Add(poolable);
			}
			return poolable;
		}
	}
}
