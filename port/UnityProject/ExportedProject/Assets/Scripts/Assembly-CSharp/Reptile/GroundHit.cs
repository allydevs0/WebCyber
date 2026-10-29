using UnityEngine;

namespace Reptile
{
	public struct GroundHit
	{
		private float _stepHeight;

		public bool isOnGround { get; set; }

		public bool isValidGround { get; set; }

		public bool isOnStep { get; set; }

		public float stepHeight
		{
			get
			{
				return _stepHeight;
			}
			set
			{
				_stepHeight = Mathf.Max(0f, value);
			}
		}

		public Vector3 groundPoint { get; set; }

		public Vector3 groundNormal { get; set; }

		public float groundDistance { get; set; }

		public Collider groundCollider { get; set; }

		public Rigidbody groundRigidbody { get; set; }

		public Vector3 groundNormalVisual { get; set; }

		public void SetTrue()
		{
			isOnGround = true;
			isValidGround = true;
		}

		public GroundHit(GroundHit other)
		{
			this = default;
			isOnGround = other.isOnGround;
			isValidGround = other.isValidGround;
			isOnStep = other.isOnStep;
			stepHeight = other.stepHeight;
			groundPoint = other.groundPoint;
			groundNormal = other.groundNormal;
			groundDistance = Mathf.Max(0f, other.groundDistance);
			groundCollider = other.groundCollider;
			groundRigidbody = other.groundRigidbody;
			groundNormalVisual = other.groundNormalVisual;
		}

		public GroundHit(RaycastHit hitInfo)
		{
			this = default;
			SetFrom(hitInfo);
		}

		public void SetFrom(RaycastHit hitInfo)
		{
			groundPoint = hitInfo.point;
			groundNormal = hitInfo.normal;
			groundDistance = Mathf.Max(0f, hitInfo.distance);
			groundCollider = hitInfo.collider;
			groundRigidbody = hitInfo.rigidbody;
		}
	}
}
