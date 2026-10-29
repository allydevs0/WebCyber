using UnityEngine;

namespace Reptile
{
	public abstract class BaseGroundDetection : MonoBehaviour
	{
		[Tooltip("Layers to be considered as 'ground' (walkables).")]
		[SerializeField]
		private LayerMask _groundMask = 1;

		[Tooltip("The maximum angle (in degrees) that will be accounted as 'ground'. Anything above is treated as a 'wall'.")]
		[SerializeField]
		private float _groundLimit = 60f;

		[Tooltip("The maximum angle (in degrees) that will be accounted as 'ground' for half pipes Anything above is treated as a 'wall'.")]
		[SerializeField]
		private float _vertGroundLimit = 85f;

		[Tooltip("The maximum height (in meters) for a valid step.\nThe character will step up a stair only if it is closer to the ground than the indicated value.\nAs rule of thumb, configure it to your character's collider radius.")]
		[SerializeField]
		private float _stepOffset = 0.5f;

		[Tooltip("Determines the maximum length of the cast.\nAs rule of thumb, configure it to your character's collider radius.")]
		[SerializeField]
		private float _castDistance = 0.5f;

		[Tooltip("Should Triggers be considered as 'ground'?")]
		[SerializeField]
		private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

		private static readonly Collider[] OverlappedColliders = new Collider[16];

		protected const float kBackstepDistance = 0.05f;

		protected const float kMinCastDistance = 0.01f;

		protected const float kMinStepOffset = 0.1f;

		protected const float kHorizontalOffset = 0.001f;

		private CapsuleCollider _capsuleCollider;

		private Player _player;

		protected GroundHit _groundHitInfo;

		private LayerMask _overlapMask = -1;

		private int _ignoreRaycastLayer = 2;

		private int _cachedLayer;

		public LayerMask groundMask
		{
			get
			{
				return _groundMask;
			}
			set
			{
				_groundMask = value;
			}
		}

		public float groundLimit
		{
			get
			{
				return _groundLimit;
			}
			set
			{
				_groundLimit = Mathf.Clamp(value, 0f, 89f);
			}
		}

		public float vertGroundLimit
		{
			get
			{
				return _vertGroundLimit;
			}
			set
			{
				_vertGroundLimit = Mathf.Clamp(value, 0f, 89f);
			}
		}

		public float stepOffset
		{
			get
			{
				return _stepOffset;
			}
			set
			{
				_stepOffset = Mathf.Clamp(value, 0.1f, capsuleCollider.radius);
			}
		}

		public float castDistance
		{
			get
			{
				return _castDistance;
			}
			set
			{
				_castDistance = Mathf.Max(0.01f, value);
			}
		}

		public QueryTriggerInteraction triggerInteraction
		{
			get
			{
				return _triggerInteraction;
			}
			set
			{
				_triggerInteraction = value;
			}
		}

		public CapsuleCollider capsuleCollider
		{
			get
			{
				if (_capsuleCollider == null)
				{
					_capsuleCollider = GetComponent<CapsuleCollider>();
				}
				return _capsuleCollider;
			}
		}

		protected Player player
		{
			get
			{
				if (_player == null)
				{
					_player = GetComponent<Player>();
				}
				return _player;
			}
		}

		public bool isOnGround => _groundHitInfo.isOnGround;

		public bool isValidGround => _groundHitInfo.isValidGround;

		public bool isOnStep => _groundHitInfo.isOnStep;

		public float stepHeight => _groundHitInfo.stepHeight;

		public bool isOnSlope => !Mathf.Approximately(groundAngle, 0f);

		public Vector3 groundPoint => _groundHitInfo.groundPoint;

		public Vector3 groundNormal => _groundHitInfo.groundNormal;

		public float groundDistance => _groundHitInfo.groundDistance;

		public Collider groundCollider => _groundHitInfo.groundCollider;

		public Rigidbody groundRigidbody => _groundHitInfo.groundRigidbody;

		public float groundAngle
		{
			get
			{
				if (isOnGround)
				{
					return Vector3.Angle(groundNormal, base.transform.up);
				}
				return 0f;
			}
		}

		public Vector3 groundNormalVisual => _groundHitInfo.groundNormalVisual;

		public GroundHit groundHit => _groundHitInfo;

		public GroundHit prevGroundHit { get; set; }

		protected bool CanMoveOnVertSurface()
		{
			if (player.moveStyle != MoveStyle.ON_FOOT)
			{
				return player.targetMovement == Player.MovementType.RUNNING;
			}
			return false;
		}

		protected virtual void InitializeOverlapMask()
		{
			int layer = base.gameObject.layer;
			_overlapMask = 0;
			for (int i = 0; i < 32; i++)
			{
				if (!Physics.GetIgnoreLayerCollision(layer, i))
				{
					_overlapMask = (int)_overlapMask | (1 << i);
				}
			}
		}

		public Collider[] OverlapCapsule(Vector3 position, Quaternion rotation, out int overlapCount)
		{
			Vector3 center = capsuleCollider.center;
			float radius = capsuleCollider.radius;
			float num = capsuleCollider.height * 0.5f - radius;
			Vector3 vector = center + Vector3.up * num;
			Vector3 vector2 = center - Vector3.up * num;
			Vector3 point = position + rotation * vector;
			int num2 = (overlapCount = Physics.OverlapCapsuleNonAlloc(position + rotation * vector2, point, radius, OverlappedColliders, _overlapMask, triggerInteraction));
			for (int i = 0; i < num2; i++)
			{
				Collider collider = OverlappedColliders[i];
				if ((!(collider != null) || !(collider != capsuleCollider)) && i < --overlapCount)
				{
					OverlappedColliders[i] = OverlappedColliders[overlapCount];
				}
			}
			return OverlappedColliders;
		}

		protected bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float distance, float backstepDistance = 0.05f)
		{
			origin -= direction * backstepDistance;
			bool num = Physics.Raycast(origin, direction, out hitInfo, distance + backstepDistance, groundMask, triggerInteraction);
			if (num)
			{
				hitInfo.distance -= backstepDistance;
			}
			return num;
		}

		protected bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float distance, float backstepDistance = 0.05f)
		{
			origin -= direction * backstepDistance;
			bool num = Physics.SphereCast(origin, radius, direction, out hitInfo, distance + backstepDistance, groundMask, triggerInteraction);
			if (num)
			{
				hitInfo.distance -= backstepDistance;
			}
			return num;
		}

		protected bool BottomSphereCast(Vector3 position, Quaternion rotation, out RaycastHit hitInfo, float distance, float backstepDistance = 0.05f)
		{
			float radius = capsuleCollider.radius;
			float num = Mathf.Max(0f, capsuleCollider.height * 0.5f - radius);
			Vector3 vector = capsuleCollider.center - Vector3.up * num;
			Vector3 origin = position + rotation * vector;
			Vector3 direction = rotation * Vector3.down;
			return SphereCast(origin, radius, direction, out hitInfo, distance, backstepDistance);
		}

		protected bool CapsuleCast(Vector3 bottom, Vector3 top, float radius, Vector3 direction, out RaycastHit hitInfo, float distance, float backstepDistance = 0.05f)
		{
			top -= direction * backstepDistance;
			bottom -= direction * backstepDistance;
			bool num = Physics.CapsuleCast(bottom, top, radius, direction, out hitInfo, distance + backstepDistance, groundMask, triggerInteraction);
			if (num)
			{
				hitInfo.distance -= backstepDistance;
			}
			return num;
		}

		public virtual bool SweepTest(Vector3 position, Quaternion rotation, Vector3 direction, out RaycastHit hitInfo, float distance = float.PositiveInfinity, float backstepDistance = 0.05f)
		{
			float radius = capsuleCollider.radius;
			float num = Mathf.Max(0f, capsuleCollider.height * 0.5f - radius);
			Vector3 vector = capsuleCollider.center - Vector3.up * num;
			Vector3 vector2 = capsuleCollider.center + Vector3.up * num;
			Vector3 bottom = position + rotation * vector;
			Vector3 top = position + rotation * vector2;
			return CapsuleCast(bottom, top, radius, direction, out hitInfo, distance, backstepDistance);
		}

		protected virtual void DisableRaycastCollisions()
		{
			_cachedLayer = base.gameObject.layer;
			base.gameObject.layer = _ignoreRaycastLayer;
		}

		protected virtual void EnableRaycastCollisions()
		{
			base.gameObject.layer = _cachedLayer;
		}

		public virtual void ResetGroundInfo()
		{
			Vector3 up = base.transform.up;
			prevGroundHit = new GroundHit(_groundHitInfo);
			_groundHitInfo = new GroundHit
			{
				groundPoint = base.transform.position,
				groundNormal = up,
				groundNormalVisual = up
			};
		}

		public abstract bool ComputeGroundHit(Vector3 position, Quaternion rotation, ref GroundHit groundHitInfo, float distance = float.PositiveInfinity);

		public void DetectGround()
		{
			DisableRaycastCollisions();
			ComputeGroundHit(base.transform.position, base.transform.rotation, ref _groundHitInfo, castDistance);
			EnableRaycastCollisions();
		}

		public abstract bool FindGround(Vector3 direction, out RaycastHit hitInfo, float distance = float.PositiveInfinity, float backstepDistance = 0.05f);

		protected virtual void DrawGizmos()
		{
		}

		protected virtual void OnValidate()
		{
			groundLimit = _groundLimit;
			stepOffset = _stepOffset;
			castDistance = _castDistance;
		}

		protected virtual void Awake()
		{
			InitializeOverlapMask();
			_ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
		}

		private void OnDrawGizmosSelected()
		{
			DrawGizmos();
		}

		public override string ToString()
		{
			return _groundHitInfo.ToString();
		}
	}
}
