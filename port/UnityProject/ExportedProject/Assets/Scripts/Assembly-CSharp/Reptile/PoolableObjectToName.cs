using System;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	public class PoolableObjectToName
	{
		public GameObject obj;

		public string name;

		public PoolableObjectToName(GameObject poolableObject, string objectName)
		{
			obj = poolableObject;
			name = objectName;
		}
	}
}
