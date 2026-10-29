using UnityEngine;

namespace ECM.Common
{
	public static class MathLibrary
	{
		public static Vector3 GetTangent(Vector3 direction, Vector3 normal, Vector3 up)
		{
			Vector3 rhs = Vector3.Cross(direction, up);
			return Vector3.Cross(normal, rhs).normalized;
		}

		public static Vector3 GetTangent(Vector3 direction, Vector3 normal)
		{
			return GetTangent(direction, normal, Vector3.up);
		}

		public static Vector3 ProjectPointOnPlane(Vector3 point, Vector3 planeOrigin, Vector3 planeNormal)
		{
			Vector3 vector = Vector3.Project(point - planeOrigin, planeNormal.normalized);
			return point - vector;
		}
	}
}
