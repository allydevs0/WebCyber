using System;
using System.Collections;
using ECM.Common;
using UnityEngine;

namespace Reptile
{
	public sealed class MovementMotor : MonoBehaviour
	{
		[Tooltip("The maximum falling speed, including movement from external forces like sliding, collisions, etc.")]
		[SerializeField]
		private float _maxFallSpeed = 20f;

		[Tooltip("The gravity applied to this character.")]
		[SerializeField]
		private float _gravity = 25f;

		[SerializeField]
		private float _gravityUp = 10f;

		[Tooltip("The amount of gravity to be applied when sliding off a steep slope.")]
		[SerializeField]
		private float _slideGravityMultiplier = 2f;

		[Tooltip("When enabled, will force the character to safely follow the walkable 'ground' geometry.")]
		[SerializeField]
		private bool _snapToGround = true;

		[Tooltip("A tolerance of how close to the 'ground' maintain the character.\n0 == no snap at all, 1 == 100% stick to ground.")]
		[Range(0f, 1f)]
		[SerializeField]
		private float _snapStrength = 0.5f;

		private Coroutine _lateFixedUpdateCoroutine;

		private Rigidbody _rigidbody;

		private CapsuleCollider capsuleCollider;

		private Vector3 _normal;

		private float _referenceCastDistance;

		private bool _forceUnground;

		private Quaternion nextRigidbodyRotation = Quaternion.identity;

		public bool rotateWithPlatform;

		[NonSerialized]
		public Vector3 dir;

		[NonSerialized]
		public bool decelerating;

		[NonSerialized]
		public bool smoothRotation;

		public float maxFallSpeed
		{
			get
			{
				return _maxFallSpeed;
			}
			set
			{
				_maxFallSpeed = Mathf.Max(0f, value);
			}
		}

		public float gravity
		{
			get
			{
				if (!(velocity.y > 0f))
				{
					return _gravity;
				}
				return _gravityUp;
			}
			set
			{
				_gravity = Mathf.Max(0f, value);
			}
		}

		public float gravityUp
		{
			get
			{
				return _gravityUp;
			}
			set
			{
				_gravityUp = Mathf.Max(0f, value);
			}
		}

		public float slideGravityMultiplier
		{
			get
			{
				return _slideGravityMultiplier;
			}
			set
			{
				_slideGravityMultiplier = Mathf.Max(1f, value);
			}
		}

		public float slideGravity => gravity * slideGravityMultiplier;

		public bool snapToGround
		{
			get
			{
				return _snapToGround;
			}
			set
			{
				_snapToGround = value;
			}
		}

		public float snapStrength
		{
			get
			{
				return _snapStrength;
			}
			set
			{
				_snapStrength = Mathf.Clamp01(value);
			}
		}

		public BaseGroundDetection groundDetection { get; set; }

		public Vector3 groundPoint => groundDetection.groundPoint;

		public Vector3 groundNormal => groundDetection.groundNormal;

		public Vector3 groundNormalVisual => groundDetection.groundNormalVisual;

		public float groundDistance => groundDetection.groundDistance;

		public Collider groundCollider => groundDetection.groundCollider;

		public Rigidbody groundRigidbody => groundDetection.groundRigidbody;

		public bool isGrounded
		{
			get
			{
				if (groundDetection.isOnGround)
				{
					return groundDetection.isValidGround;
				}
				return false;
			}
		}

		public bool isOnNonStableGround
		{
			get
			{
				if (groundDetection.isOnGround)
				{
					return !groundDetection.isValidGround;
				}
				return false;
			}
		}

		public bool wasGrounded
		{
			get
			{
				if (groundDetection.prevGroundHit.isOnGround)
				{
					return groundDetection.prevGroundHit.isValidGround;
				}
				return false;
			}
		}

		public bool isOnGround => groundDetection.isOnGround;

		public bool wasOnGround => groundDetection.prevGroundHit.isOnGround;

		public bool isValidGround => groundDetection.isValidGround;

		public bool isOnPlatform { get; private set; }

		public bool isOnStep => groundDetection.isOnStep;

		public float stepHeight => groundDetection.stepHeight;

		public bool isOnSlope => groundDetection.isOnSlope;

		public float groundAngle => groundDetection.groundAngle;

		public Vector3 platformVelocity { get; private set; }

		public Vector3 platformAngularVelocity { get; private set; }

		public Vector3 velocity
		{
			get
			{
				return _rigidbody.velocity - platformVelocity;
			}
			set
			{
				_rigidbody.velocity = value + platformVelocity;
			}
		}

		public float forwardSpeed => Vector3.Dot(velocity, base.transform.forward);

		public Quaternion rotation
		{
			get
			{
				return _rigidbody.rotation;
			}
			set
			{
				if (smoothRotation)
				{
					nextRigidbodyRotation = value;
				}
				else
				{
					_rigidbody.rotation = value;
				}
			}
		}

		public GroundHit groundHit => groundDetection.groundHit;

		public GroundHit prevGroundHit => groundDetection.prevGroundHit;

		public void HaveCollision(bool have)
		{
			capsuleCollider.enabled = have;
		}

		public bool CollisionIsOn()
		{
			return capsuleCollider.enabled;
		}

		public void SetKinematic(bool set)
		{
			_rigidbody.isKinematic = set;
		}

		public bool IsKinematic()
		{
			return _rigidbody.isKinematic;
		}

		public CapsuleCollider GetCapsule()
		{
			return groundDetection.capsuleCollider;
		}

		public void SetVelocityTotal(Vector3 vel, Vector3 platformVel, Vector3 platformAngularVel)
		{
			_rigidbody.velocity = vel;
			platformVelocity = platformVel;
			platformAngularVelocity = platformAngularVel;
		}

		public void SetGroundedSpawn()
		{
			groundDetection.groundHit.SetTrue();
			groundDetection.prevGroundHit.SetTrue();
		}

		public void SetPositionTeleport(Vector3 pos)
		{
			_rigidbody.position = pos;
		}

		public Vector3 BodyPosition()
		{
			return _rigidbody.position;
		}

		public void RigidbodyMove(Vector3 newPos)
		{
			_rigidbody.MovePosition(newPos);
		}

		public void RigidbodyMoveRotation(Quaternion rot)
		{
			_rigidbody.MoveRotation(rot);
		}

		private void OverlapRecovery(ref Vector3 probingPosition, Quaternion probingRotation)
		{
			Collider[] array = groundDetection.OverlapCapsule(probingPosition, probingRotation, out var overlapCount);
			CapsuleCollider colliderA = groundDetection.capsuleCollider;
			for (int i = 0; i < overlapCount; i++)
			{
				Collider collider = array[i];
				if (!(collider.attachedRigidbody != null))
				{
					Transform transform = collider.transform;
					if (Physics.ComputePenetration(colliderA, probingPosition, probingRotation, collider, transform.position, transform.rotation, out var direction, out var distance))
					{
						probingPosition += direction * distance;
					}
				}
			}
		}

		public bool ComputeGroundHit(Vector3 probingPosition, Quaternion probingRotation, out GroundHit groundHitInfo, float scanDistance = float.PositiveInfinity)
		{
			groundHitInfo = default;
			return groundDetection.ComputeGroundHit(probingPosition, probingRotation, ref groundHitInfo, scanDistance);
		}

		public bool ComputeGroundHit(out GroundHit hitInfo, float scanDistance = float.PositiveInfinity)
		{
			Vector3 position = base.transform.position;
			Quaternion probingRotation = base.transform.rotation;
			return ComputeGroundHit(position, probingRotation, out hitInfo, scanDistance);
		}

		public void Rotate(Vector3 direction, float rotSpeed, bool onlyLateral = true)
		{
			if (onlyLateral)
			{
				direction.y = 0f;
			}
			if (!(direction.sqrMagnitude < 0.0001f))
			{
				Quaternion b = Quaternion.LookRotation(direction);
				Quaternion quaternion = Quaternion.Slerp(_rigidbody.rotation, b, rotSpeed * Core.dt);
				dir = quaternion * Vector3.forward;
				if (smoothRotation)
				{
					nextRigidbodyRotation = quaternion;
				}
				else
				{
					_rigidbody.MoveRotation(quaternion);
				}
			}
		}

		public void ApplyDrag(float drag, bool onlyLateral = true)
		{
			Vector3 vector = (onlyLateral ? velocity.onlyXZ() : velocity);
			Vector3 vector2 = (0f - drag) * vector.magnitude * vector;
			if (onlyLateral)
			{
				vector2 = Vector3.ProjectOnPlane(vector2, Vector3.up);
			}
			_rigidbody.AddForce(vector2, ForceMode.Acceleration);
		}

		public void ApplyForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
		{
			_rigidbody.AddForce(force, forceMode);
		}

		public void SetVelocityYOneTime(float y)
		{
			Vector3 vector = Vector3.up * y;
			_rigidbody.velocity = _rigidbody.velocity.onlyXZ() + vector;
		}

		public void ForceUnground()
		{
			_forceUnground = true;
			groundDetection.castDistance = 0f;
		}

		public bool ForcedUnground()
		{
			return _forceUnground;
		}

		private void ResetGroundInfo()
		{
			groundDetection.ResetGroundInfo();
			isOnPlatform = false;
			platformVelocity = Vector3.zero;
			platformAngularVelocity = Vector3.zero;
			_normal = Vector3.up;
		}

		public void DetectGround()
		{
			ResetGroundInfo();
			if (_forceUnground)
			{
				_forceUnground = false;
			}
			else
			{
				groundDetection.DetectGround();
				groundDetection.castDistance = (isGrounded ? _referenceCastDistance : 0f);
			}
			if (!isOnGround)
			{
				return;
			}
			Vector3 up = base.transform.up;
			if (isValidGround)
			{
				_normal = groundDetection.groundNormal;
			}
			else
			{
				_normal = Vector3.Cross(Vector3.Cross(up, groundDetection.groundNormal), up).normalized;
			}
			Rigidbody rigidbody = groundRigidbody;
			if (!(rigidbody == null))
			{
				if (rigidbody.isKinematic)
				{
					isOnPlatform = true;
					platformVelocity = rigidbody.GetPointVelocity(groundPoint);
					platformAngularVelocity = Vector3.Project(rigidbody.angularVelocity, up);
				}
				else
				{
					_normal = Vector3.up;
				}
			}
		}

		private void PreventGroundPenetration()
		{
			if (!isOnGround)
			{
				Vector3 vector = velocity;
				float magnitude = vector.magnitude;
				Vector3 direction = ((magnitude > 0f) ? (vector / magnitude) : Vector3.zero);
				float distance = magnitude * Core.dt;
				if (groundDetection.FindGround(direction, out var _, distance))
				{
					groundDetection.castDistance = _referenceCastDistance;
				}
			}
		}

		private void ApplyGroundMovement(Vector3 desiredVelocity, float acceleration, float deceleration)
		{
			float dt = Core.dt;
			Vector3 vector = (wasGrounded ? velocity : Vector3.ProjectOnPlane(velocity, Vector3.up));
			float magnitude = desiredVelocity.magnitude;
			Vector3 vector2 = MathLibrary.GetTangent(desiredVelocity, _normal);
			Vector3 vector3 = vector2 * acceleration * dt;
			if (vector2.isZero())
			{
				vector2 = base.transform.forward;
			}
			vector = vector2 * vector.magnitude;
			vector = MathLibrary.GetTangent(vector, _normal) * vector.magnitude;
			if (desiredVelocity.isZero() || vector.isExceeding(magnitude))
			{
				float magnitude2 = vector.magnitude;
				if (magnitude2 > magnitude)
				{
					magnitude2 -= deceleration * dt;
					if (magnitude2 < magnitude)
					{
						magnitude2 = magnitude;
					}
					vector = vector.normalized * magnitude2;
				}
				else
				{
					vector = vector.normalized * magnitude;
				}
				decelerating = true;
			}
			else
			{
				vector = Vector3.ClampMagnitude(vector + vector3, magnitude);
				decelerating = false;
			}
			velocity = vector;
		}

		private void ApplyAirMovement(Vector3 desiredVelocity, float acceleration, float deceleration, bool onlyLateral = true)
		{
			Vector3 up = Vector3.up;
			Vector3 vector = (onlyLateral ? Vector3.ProjectOnPlane(velocity, up) : velocity);
			if (onlyLateral)
			{
				desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, up);
			}
			if (isOnGround && Vector3.Dot(desiredVelocity, _normal) <= 0f)
			{
				float magnitude = desiredVelocity.magnitude;
				Vector3 vector2 = Vector3.ProjectOnPlane(velocity, up);
				desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, _normal) + Vector3.Project(vector2, _normal);
				desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, magnitude);
			}
			float dt = Core.dt;
			float magnitude2 = desiredVelocity.magnitude;
			Vector3 vector3 = ((magnitude2 > 0f) ? (desiredVelocity / magnitude2) : base.transform.forward);
			Vector3 vector4 = vector3 * acceleration * dt;
			if (magnitude2 > 0f)
			{
				vector = vector3 * vector.magnitude;
			}
			if (vector4.isZero() || vector.isExceeding(magnitude2))
			{
				float magnitude3 = vector.magnitude;
				if (magnitude3 > magnitude2)
				{
					magnitude3 -= deceleration * 2f * dt;
					if (magnitude3 < magnitude2)
					{
						magnitude3 = magnitude2;
					}
					vector = vector.normalized * magnitude3;
				}
				else
				{
					vector = vector.normalized * magnitude2;
				}
			}
			else
			{
				vector = Vector3.ClampMagnitude(vector + vector4, magnitude2);
			}
			Vector3 vector5 = up * ((isOnGround ? (gravity * slideGravityMultiplier) : gravity) * dt);
			if (onlyLateral)
			{
				velocity += Vector3.ProjectOnPlane(vector - velocity, up) - vector5;
			}
			else
			{
				velocity += vector - velocity - vector5;
			}
		}

		public void ApplyCustomMovement(Vector3 desiredCustomVelocity, float customGravity)
		{
			Vector3 vector = velocity;
			Vector3 up = Vector3.up;
			if (isGrounded)
			{
				vector = desiredCustomVelocity;
				vector = MathLibrary.GetTangent(vector, _normal) * vector.magnitude;
			}
			else if (isOnGround && !isGrounded)
			{
				if (Vector3.Dot(desiredCustomVelocity, _normal) <= 0f)
				{
					float magnitude = desiredCustomVelocity.magnitude;
					Vector3 vector2 = Vector3.ProjectOnPlane(velocity, up);
					desiredCustomVelocity = Vector3.ProjectOnPlane(desiredCustomVelocity, _normal) + Vector3.Project(vector2, _normal);
					desiredCustomVelocity = Vector3.ClampMagnitude(desiredCustomVelocity, magnitude);
				}
				vector = desiredCustomVelocity;
			}
			velocity += vector - velocity;
			velocity -= up * (customGravity * Core.dt);
		}

		private void LimitFallSpeed()
		{
			float num = Vector3.Dot(velocity, Vector3.up);
			if (num < 0f - maxFallSpeed)
			{
				_rigidbody.velocity += new Vector3(0f, 0f - maxFallSpeed - num, 0f);
			}
		}

		public void MoveDrive(Vector3 desiredVelocity, float acceleration, float deceleration)
		{
			if (isGrounded)
			{
				ApplyGroundMovement(desiredVelocity, acceleration, deceleration);
			}
			else
			{
				ApplyAirMovement(desiredVelocity, acceleration, deceleration);
			}
			if (snapToGround && isGrounded)
			{
				SnapToGround();
			}
			LimitFallSpeed();
			PreventGroundPenetration();
		}

		public void CustomMove(Vector3 desiredCustomVelocity, float customGravity)
		{
			ApplyCustomMovement(desiredCustomVelocity, customGravity);
			if (snapToGround && isGrounded)
			{
				SnapToGround();
			}
			LimitFallSpeed();
			PreventGroundPenetration();
		}

		private void SnapToGround()
		{
			if (!(groundDistance < 0.001f))
			{
				Rigidbody rigidbody = groundRigidbody;
				if (!(rigidbody != null) || !rigidbody.isKinematic)
				{
					float num = Mathf.Max(0f, groundDistance - 0.01f);
					Vector3 vector = ((!isOnStep) ? (base.transform.up * (0f - num)) : (base.transform.up * stepHeight + base.transform.forward * 0.1f));
					vector *= snapStrength / Core.dt;
					velocity = (velocity + vector).normalized * velocity.magnitude;
				}
			}
		}

		private bool SnapToPlatform(ref Vector3 probingPosition, ref Quaternion probingRotation)
		{
			if (!ComputeGroundHit(probingPosition, probingRotation, out var groundHitInfo, groundDetection.castDistance))
			{
				return false;
			}
			Rigidbody rigidbody = groundHitInfo.groundRigidbody;
			if (rigidbody == null || !rigidbody.isKinematic)
			{
				return false;
			}
			Vector3 vector = probingRotation * Vector3.up;
			Vector3 vector2 = probingPosition - vector * groundHitInfo.groundDistance;
			Vector3 pointVelocity = rigidbody.GetPointVelocity(vector2);
			_rigidbody.velocity = velocity + pointVelocity;
			Vector3 vector3 = pointVelocity - platformVelocity;
			vector2 += vector3.onlyXZ() * Time.fixedDeltaTime;
			probingPosition = vector2;
			if (rigidbody.angularVelocity == Vector3.zero)
			{
				return false;
			}
			Quaternion quaternion = Quaternion.Euler(Vector3.Project(rigidbody.angularVelocity, vector) * 57.29578f * Core.dt);
			probingRotation *= quaternion;
			return true;
		}

		private IEnumerator LateFixedUpdate()
		{
			WaitForFixedUpdate waitTime = new WaitForFixedUpdate();
			while (true)
			{
				yield return waitTime;
				if (smoothRotation)
				{
					_rigidbody.MoveRotation(nextRigidbodyRotation);
				}
				Vector3 probingPosition = base.transform.position;
				Quaternion probingRotation = (smoothRotation ? nextRigidbodyRotation : base.transform.rotation);
				bool flag = false;
				OverlapRecovery(ref probingPosition, probingRotation);
				if (isOnGround && isOnPlatform)
				{
					flag = SnapToPlatform(ref probingPosition, ref probingRotation) && rotateWithPlatform;
					if (flag && smoothRotation)
					{
						nextRigidbodyRotation = probingRotation;
					}
				}
				_rigidbody.MovePosition(probingPosition);
				if (flag)
				{
					_rigidbody.MoveRotation(probingRotation);
				}
			}
		}

		private void OnValidate()
		{
			maxFallSpeed = _maxFallSpeed;
			gravity = _gravity;
			gravityUp = _gravityUp;
			slideGravityMultiplier = _slideGravityMultiplier;
			snapToGround = _snapToGround;
			snapStrength = _snapStrength;
		}

		private void Awake()
		{
			groundDetection = GetComponent<BaseGroundDetection>();
			if (groundDetection == null)
			{
				return;
			}
			_referenceCastDistance = groundDetection.castDistance;
			capsuleCollider = GetComponent<CapsuleCollider>();
			_rigidbody = GetComponent<Rigidbody>();
			if (_rigidbody == null)
			{
				return;
			}
			_rigidbody.useGravity = false;
			_rigidbody.isKinematic = false;
			_rigidbody.freezeRotation = true;
			dir = base.transform.forward;
			Collider component = GetComponent<Collider>();
			if (!(component == null))
			{
				PhysicMaterial sharedMaterial = component.sharedMaterial;
				if (!(sharedMaterial != null))
				{
					sharedMaterial = new PhysicMaterial("Frictionless")
					{
						dynamicFriction = 0f,
						staticFriction = 0f,
						bounciness = 0f,
						frictionCombine = PhysicMaterialCombine.Multiply,
						bounceCombine = PhysicMaterialCombine.Average
					};
					component.material = sharedMaterial;
				}
			}
		}

		private void OnEnable()
		{
			if (_lateFixedUpdateCoroutine != null)
			{
				StopCoroutine(_lateFixedUpdateCoroutine);
			}
			_lateFixedUpdateCoroutine = StartCoroutine(LateFixedUpdate());
		}

		private void OnDisable()
		{
			if (_lateFixedUpdateCoroutine != null)
			{
				StopCoroutine(_lateFixedUpdateCoroutine);
			}
		}
	}
}
