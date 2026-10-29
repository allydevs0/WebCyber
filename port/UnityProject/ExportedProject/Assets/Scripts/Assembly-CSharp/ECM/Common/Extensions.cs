using UnityEngine;

namespace ECM.Common
{
	public static class Extensions
	{
		public static bool isZero(this float value)
		{
			return Mathf.Abs(value) < 1E-10f;
		}

		public static Vector3 onlyX(this Vector3 vector3)
		{
			vector3.y = 0f;
			vector3.z = 0f;
			return vector3;
		}

		public static Vector3 onlyY(this Vector3 vector3)
		{
			vector3.x = 0f;
			vector3.z = 0f;
			return vector3;
		}

		public static Vector3 onlyZ(this Vector3 vector3)
		{
			vector3.x = 0f;
			vector3.y = 0f;
			return vector3;
		}

		public static Vector3 onlyXZ(this Vector3 vector3)
		{
			vector3.y = 0f;
			return vector3;
		}

		public static bool isZero(this Vector3 vector3)
		{
			return (double)vector3.sqrMagnitude < 9.99999943962493E-11;
		}

		public static bool isExceeding(this Vector3 vector3, float magnitude)
		{
			return vector3.sqrMagnitude > magnitude * magnitude * 1.01f;
		}

		public static Vector3 normalized(this Vector3 vector3, out float magnitude)
		{
			magnitude = vector3.magnitude;
			if ((double)magnitude > 9.99999974737875E-06)
			{
				return vector3 / magnitude;
			}
			magnitude = 0f;
			return Vector3.zero;
		}

		public static Vector3 clamped(this Vector3 vector3, out float magnitude)
		{
			magnitude = vector3.magnitude;
			if (!(magnitude > 1f))
			{
				return vector3;
			}
			return vector3 / magnitude;
		}

		public static Vector3 clampedTo(this Vector3 vector3, float maxLength)
		{
			if ((double)vector3.sqrMagnitude > (double)maxLength * (double)maxLength)
			{
				return vector3.normalized * maxLength;
			}
			return vector3;
		}

		public static Vector3 relativeTo(this Vector3 vector3, Transform target, bool onlyLateral = true)
		{
			return Quaternion.LookRotation(onlyLateral ? target.forward.onlyXZ() : target.forward) * vector3;
		}
	}
}
