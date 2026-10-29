using System;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	public struct SafeLocation
	{
		public Vector3 position;

		public Quaternion rotation;

		public float timeStamp;

		public bool set;

		public SafeLocation(Vector3 pos, Quaternion rot, float time)
		{
			position = pos;
			rotation = rot;
			timeStamp = time;
			set = true;
		}
	}
}
