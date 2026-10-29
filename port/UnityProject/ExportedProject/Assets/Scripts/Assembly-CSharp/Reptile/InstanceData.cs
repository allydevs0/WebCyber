using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class InstanceData
	{
		public List<Matrix4x4[]> worldMatrix;

		public List<float[]> frameIndex;

		public List<float[]> preFrameIndex;

		public List<float[]> transitionProgress;
	}
}
