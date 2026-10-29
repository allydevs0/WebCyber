using UnityEngine;

namespace Reptile
{
	public static class CameraExtensions
	{
		public static Plane[] CalculateCameraFrustumPlanes(this Camera camera)
		{
			return GeometryUtility.CalculateFrustumPlanes(camera);
		}
	}
}
