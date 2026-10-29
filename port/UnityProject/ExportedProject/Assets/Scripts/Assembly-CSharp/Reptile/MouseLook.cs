using System;
using UnityEngine;

namespace Reptile
{
	public class MouseLook : MonoBehaviour
	{
		[Tooltip("Should the mouse cursor be locked (eg: hidden)?")]
		[SerializeField]
		private bool _lockCursor = true;

		[Tooltip("The keyboard key to unlock the mouse cursor.")]
		[SerializeField]
		private KeyCode _unlockCursorKey = KeyCode.Escape;

		[Tooltip("How fast the cursor moves in response to mouse lateral (x-axis) movement.")]
		[SerializeField]
		private float _lateralSensitivity = 2f;

		[Tooltip("How fast the cursor moves in response to mouse vertical (y-axis) movement.")]
		[SerializeField]
		private float _verticalSensitivity = 2f;

		[Tooltip("Should the rotation be smoothed (eg: interpolated)?")]
		[SerializeField]
		private bool _smooth;

		[Tooltip("Approximately the time (in seconds) it will take to reach the target.\nA smaller value will reach the target faster.")]
		[SerializeField]
		public float _smoothTime = 5f;

		[Tooltip("Should the rotation around x-axis be clamped?")]
		[SerializeField]
		private bool _clampPitch = true;

		[Tooltip("The minimum pitch angle (in degrees).")]
		[SerializeField]
		private float _minPitchAngle = -90f;

		[Tooltip("The maximum pitch angle (in degrees).")]
		[SerializeField]
		private float _maxPitchAngle = 90f;

		protected bool _isCursorLocked = true;

		protected Quaternion characterTargetRotation;

		protected Quaternion cameraTargetRotation;

		public bool lockCursor
		{
			get
			{
				return _lockCursor;
			}
			set
			{
				_lockCursor = value;
			}
		}

		public KeyCode unlockCursorKey
		{
			get
			{
				return _unlockCursorKey;
			}
			set
			{
				_unlockCursorKey = value;
			}
		}

		public float lateralSensitivity
		{
			get
			{
				return _lateralSensitivity;
			}
			set
			{
				_lateralSensitivity = Mathf.Max(0f, value);
			}
		}

		public float verticalSensitivity
		{
			get
			{
				return _verticalSensitivity;
			}
			set
			{
				_verticalSensitivity = Mathf.Max(0f, value);
			}
		}

		public bool smooth
		{
			get
			{
				return _smooth;
			}
			set
			{
				_smooth = value;
			}
		}

		public float smoothTime
		{
			get
			{
				return _smoothTime;
			}
			set
			{
				_smoothTime = Mathf.Max(0f, value);
			}
		}

		public bool clampPitch
		{
			get
			{
				return _clampPitch;
			}
			set
			{
				_clampPitch = value;
			}
		}

		public float minPitchAngle
		{
			get
			{
				return _minPitchAngle;
			}
			set
			{
				_minPitchAngle = Mathf.Clamp(value, -180f, 180f);
			}
		}

		public float maxPitchAngle
		{
			get
			{
				return _maxPitchAngle;
			}
			set
			{
				_maxPitchAngle = Mathf.Clamp(value, -180f, 180f);
			}
		}

		public virtual void Init(Transform characterTransform, Transform cameraTransform)
		{
			characterTargetRotation = characterTransform.localRotation;
			cameraTargetRotation = cameraTransform.localRotation;
		}

		public virtual void LookRotation(MovementMotor movement, Transform cameraTransform)
		{
			float y = Input.GetAxis("Mouse X") * lateralSensitivity;
			float num = Input.GetAxis("Mouse Y") * verticalSensitivity;
			Quaternion quaternion = Quaternion.Euler(0f, y, 0f);
			Quaternion quaternion2 = Quaternion.Euler(0f - num, 0f, 0f);
			characterTargetRotation *= quaternion;
			cameraTargetRotation *= quaternion2;
			if (smooth)
			{
				float dt = Core.dt;
				if (movement.isOnPlatform && movement.platformAngularVelocity != Vector3.zero)
				{
					characterTargetRotation *= Quaternion.Euler(movement.platformAngularVelocity * 57.29578f * dt);
				}
				movement.rotation = Quaternion.Slerp(movement.rotation, characterTargetRotation, smoothTime * dt);
				cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, cameraTargetRotation, smoothTime * dt);
			}
			else
			{
				movement.rotation *= quaternion;
				cameraTransform.localRotation *= quaternion2;
			}
			if (clampPitch)
			{
				cameraTransform.localRotation = ClampPitch(cameraTransform.localRotation);
			}
			UpdateCursorLock();
		}

		public virtual void SetCursorLock(bool value)
		{
			lockCursor = value;
			if (!lockCursor)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		public virtual void UpdateCursorLock()
		{
			if (lockCursor)
			{
				InternalLockUpdate();
			}
		}

		protected virtual void InternalLockUpdate()
		{
			if (Input.GetKeyUp(unlockCursorKey))
			{
				_isCursorLocked = false;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				_isCursorLocked = true;
			}
			if (_isCursorLocked)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else if (!_isCursorLocked)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		protected Quaternion ClampPitch(Quaternion q)
		{
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1f;
			float value = 114.59156f * Mathf.Atan(q.x);
			value = Mathf.Clamp(value, minPitchAngle, maxPitchAngle);
			q.x = Mathf.Tan(MathF.PI / 360f * value);
			return q;
		}

		public virtual void OnValidate()
		{
			lateralSensitivity = _lateralSensitivity;
			verticalSensitivity = _verticalSensitivity;
			smoothTime = _smoothTime;
			minPitchAngle = _minPitchAngle;
			maxPitchAngle = _maxPitchAngle;
		}
	}
}
