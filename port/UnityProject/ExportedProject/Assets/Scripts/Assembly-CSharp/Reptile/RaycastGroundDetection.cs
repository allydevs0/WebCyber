using System;
using UnityEngine;

namespace Reptile
{
	[Obsolete("This has been replaced for the more feature-rich GroundDetection component. Please feel free to remove this class as this will be removed in next update.")]
	public sealed class RaycastGroundDetection : BaseGroundDetection
	{
		public override bool ComputeGroundHit(Vector3 position, Quaternion rotation, ref GroundHit groundHitInfo, float distance = float.PositiveInfinity)
		{
			throw new NotImplementedException();
		}

		public override bool FindGround(Vector3 direction, out RaycastHit hitInfo, float distance = float.PositiveInfinity, float backstepDistance = 0.05f)
		{
			throw new NotImplementedException();
		}
	}
}
