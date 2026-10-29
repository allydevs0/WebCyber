using System;
using Reptile;
using UnityEngine;

namespace ECM.Helpers
{
	[RequireComponent(typeof(MovementMotor))]
	public sealed class OrientModelToGround : MonoBehaviour
	{
		[Tooltip("Should the model follow the ground slope?")]
		[SerializeField]
		private bool _orientToGround = true;

		[Tooltip("The model's transform to orient.")]
		public Transform modelTransform;

		[Tooltip("Minimum slope angle (in degrees) to cause an orientation change.")]
		[SerializeField]
		private float _minAngle = 5f;

		[Tooltip(" Maximum turning speed (in deg/s).")]
		[SerializeField]
		private float _rotationSpeed = 240f;

		private MovementMotor _movement;

		private Quaternion _groundRotation = Quaternion.identity;

		public bool orientToGround
		{
			get
			{
				return _orientToGround;
			}
			set
			{
				_orientToGround = value;
				if ((bool)modelTransform)
				{
					modelTransform.rotation = base.transform.rotation;
				}
			}
		}

		public float minAngle
		{
			get
			{
				return _minAngle;
			}
			set
			{
				_minAngle = Mathf.Clamp(value, 0f, 360f);
			}
		}

		public float rotationSpeed
		{
			get
			{
				return _rotationSpeed;
			}
			set
			{
				_rotationSpeed = Mathf.Max(0f, value);
			}
		}

		public void OnValidate()
		{
			orientToGround = _orientToGround;
			minAngle = _minAngle;
			rotationSpeed = _rotationSpeed;
		}

		public void Awake()
		{
			_movement = GetComponent<MovementMotor>();
			_groundRotation = modelTransform.rotation;
		}

		public void Update()
		{
			if (orientToGround)
			{
				Quaternion b = ((_movement.groundAngle < minAngle) ? Quaternion.identity : Quaternion.FromToRotation(Vector3.up, _movement.groundNormalVisual));
				float t = rotationSpeed * (MathF.PI / 180f) * Core.dt;
				_groundRotation = Quaternion.Slerp(_groundRotation, b, t);
				modelTransform.rotation = _groundRotation * base.transform.rotation;
			}
		}
	}
}
