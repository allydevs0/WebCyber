using System;
using UnityEngine;

namespace Reptile
{
	public abstract class CameraMode
	{
		protected struct OrbitVector
		{
			public float angleHor;

			public float angleVert;

			public float dist;

			public static OrbitVector Lerp(OrbitVector look1, OrbitVector look2, float f)
			{
				return new OrbitVector
				{
					angleHor = Mathf.LerpAngle(look1.angleHor, look2.angleHor, f),
					angleVert = Mathf.LerpAngle(look1.angleVert, look2.angleVert, f),
					dist = Mathf.Lerp(look1.dist, look2.dist, f)
				};
			}

			public static OrbitVector FromTo(Vector3 pos, Vector3 lookAt)
			{
				OrbitVector result = default;
				Vector3 vector = pos - lookAt;
				result.angleHor = Mathf.Atan2(vector.z, vector.x) * 57.29578f;
				Vector3 vector2 = Quaternion.AngleAxis(result.angleHor, Vector3.up) * vector;
				result.dist = vector2.magnitude;
				result.angleVert = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
				return result;
			}

			public Vector3 GetPos(Vector3 lookAt)
			{
				Vector3 vector = new Vector3(Mathf.Cos(angleVert * (MathF.PI / 180f)), Mathf.Sin(angleVert * (MathF.PI / 180f)), 0f) * dist;
				vector = Quaternion.Euler(0f, 0f - angleHor, 0f) * vector;
				return lookAt + vector;
			}
		}

		protected Player player;

		protected GameplayCamera cam;

		public bool inputEnabled;

		protected Vector3 position;

		public Vector3 positionFinal;

		public Vector3 lookAtPos;

		public float tilt;

		protected float targetLookAtY;

		internal float lookAtYOffset;

		protected bool wasObstructed;

		protected RaycastHit[] obstructions = new RaycastHit[32];

		protected const int maxObstructions = 32;

		protected bool sortaGrounded = true;

		protected bool lerpDefaultDistance;

		protected float previousObstructedDistance;

		public bool prevSortaGrounded = true;

		protected Vector3 localFlatPos;

		protected float lastOrbitTimer;

		protected float manualOrbitFactor;

		protected bool straightToNormalHeight;

		protected float postureOffset;

		private Vector3 prevDragOffset;

		protected const float backToDefaultDistDamping = 1f;

		protected const float velocityZeroValueSq = 1E-05f;

		protected float dt => Time.deltaTime;

		public CameraMode()
		{
		}

		public virtual void Activate(Player player, CameraMode modePrev)
		{
			this.player = player;
			cam = player.cam;
			if (modePrev == null)
			{
				Reset();
			}
			else
			{
				position = modePrev.position;
				lookAtPos = modePrev.lookAtPos;
				targetLookAtY = lookAtPos.y;
			}
			prevDragOffset = Vector3.zero;
		}

		protected abstract void UpdatePos(int fixedFrameCounter);

		public void Update(int fixedFrameCounter)
		{
			sortaGrounded = player.TreatPlayerAsSortaGrounded();
			UpdatePos(fixedFrameCounter);
			prevSortaGrounded = sortaGrounded;
		}

		public void Reset()
		{
			positionFinal = (position = player.tf.position + cam.defaultCamHeight * Vector3.up + player.motor.dir * (0f - cam.dragDistanceMax));
			Vector3 vector = (cam.topLocalLookAt + cam.postureOffsetTarget) * Vector3.up;
			lookAtYOffset = vector.y;
			lookAtPos = player.tf.position + vector;
			targetLookAtY = lookAtPos.y;
			tilt = 0f;
		}

		protected void HandleLookAt(int fixedFrameCounter, Vector3? offsetOverride = null)
		{
			Vector3 vector = player.tf.position;
			Vector3 vector2 = vector;
			if (offsetOverride.HasValue)
			{
				vector2 += offsetOverride.Value;
				targetLookAtY = vector2.y;
			}
			else
			{
				postureOffset = LerpTo(postureOffset, cam.postureOffsetTarget, cam.postureOffsetSharpness);
				if (sortaGrounded)
				{
					lookAtYOffset = LerpTo(lookAtYOffset, cam.topLocalLookAt * CamHeightFactor(), cam.lookAtVertSharpness);
				}
				else
				{
					float value = targetLookAtY - vector.y - postureOffset;
					lookAtYOffset = Mathf.Clamp(value, cam.bottomLocalLookAt, cam.topLocalLookAt);
				}
				targetLookAtY = vector.y + lookAtYOffset + postureOffset;
			}
			Vector3 velocity = player.GetVelocity();
			Vector3 vector3 = vector2 - lookAtPos;
			vector3.y = targetLookAtY - lookAtPos.y;
			if (Mathf.Abs(vector3.x) > Mathf.Abs(velocity.x * Time.fixedDeltaTime * (float)fixedFrameCounter))
			{
				vector2.x = LerpTo(lookAtPos.x, vector2.x, cam.lookAtSharpness);
			}
			if (Mathf.Abs(vector3.y) > Mathf.Abs(velocity.y * Time.fixedDeltaTime * (float)fixedFrameCounter))
			{
				vector2.y = LerpTo(lookAtPos.y, targetLookAtY, cam.lookAtVertSharpness);
			}
			if (Mathf.Abs(vector3.z) > Mathf.Abs(velocity.z * Time.fixedDeltaTime * (float)fixedFrameCounter))
			{
				vector2.z = LerpTo(lookAtPos.z, vector2.z, cam.lookAtSharpness);
			}
			lookAtPos.x = vector2.x;
			lookAtPos.y = targetLookAtY;
			lookAtPos.z = vector2.z;
			if (lookAtPos.y > targetLookAtY)
			{
				lookAtPos.y = targetLookAtY;
			}
		}

		protected Vector3 HandleCamInput(Vector3 pos, Vector2? extraInput = null)
		{
			Vector2 vector = Vector2.zero;
			if (inputEnabled)
			{
				vector = cam.orbitInput * (cam.mouseForOrbit ? cam.mouseFactor : cam.joystickFactor);
				if (extraInput.HasValue)
				{
					vector += extraInput.Value * cam.joystickFactor;
				}
				vector *= 60f * dt;
			}
			if (vector != Vector2.zero)
			{
				OrbitVector orbitVector = OrbitVector.FromTo(pos, lookAtPos);
				orbitVector.angleVert += vector.y;
				orbitVector.angleHor -= vector.x;
				orbitVector.angleVert = Mathf.Clamp(orbitVector.angleVert, cam.lookAngleLowest, cam.lookAngleHighest);
				pos = orbitVector.GetPos(lookAtPos);
			}
			if (vector != Vector2.zero || cam.mouseForOrbit)
			{
				lastOrbitTimer = 0f;
			}
			else
			{
				lastOrbitTimer += dt / cam.orbitFadeDuration;
			}
			manualOrbitFactor = Mathf.Clamp01(1f - lastOrbitTimer);
			return pos;
		}

		protected Vector3 HandleDragging(Vector3 pos, float? heightOffset = null, float? pushBehind = null, float? horDistMaxOverride = null, float? horDistDefaultOverride = null)
		{
			Vector3 vector = player.tf.position;
			Vector3 practicalWorldVelocity = player.GetPracticalWorldVelocity();
			float num = player.minMoveSpeed + 0.1f;
			bool flag = practicalWorldVelocity.x * practicalWorldVelocity.x + practicalWorldVelocity.z * practicalWorldVelocity.z > num * num;
			float num2 = vector.y + cam.defaultCamHeight * CamHeightFactor();
			bool flag2 = cam.orbitInput != Vector2.zero;
			if (!flag2 && manualOrbitFactor == 1f && prevDragOffset != Vector3.zero)
			{
				return lookAtPos + prevDragOffset;
			}
			Vector3 vector2 = ((!flag) ? Vector3.zero : (practicalWorldVelocity * manualOrbitFactor));
			if (sortaGrounded)
			{
				vector2.y = practicalWorldVelocity.y;
			}
			else
			{
				float num3 = Mathf.Clamp01((Mathf.Abs(practicalWorldVelocity.y) - 10f) / 4f);
				vector2.y = num3 * practicalWorldVelocity.y * manualOrbitFactor;
			}
			pos += vector2 * dt;
			localFlatPos = pos - vector;
			localFlatPos.y = 0f;
			float num4 = (horDistDefaultOverride.HasValue ? horDistDefaultOverride.Value : cam.dragDistanceDefault);
			float num5 = (horDistMaxOverride.HasValue ? horDistMaxOverride.Value : cam.dragDistanceMax);
			float num6 = localFlatPos.magnitude;
			if (num6 > num5)
			{
				num6 = num5;
			}
			else if (num6 < num4 && practicalWorldVelocity.sqrMagnitude > 1E-05f)
			{
				if (lerpDefaultDistance)
				{
					num6 = LerpTo(num6, num4, cam.orbitDistanceSharpness);
					if (num6 == num4)
					{
						lerpDefaultDistance = false;
					}
				}
				else
				{
					num6 = num4;
				}
			}
			else if (flag2 && practicalWorldVelocity.sqrMagnitude <= 1E-05f)
			{
				num6 = LerpTo(num6, num4, 1f);
				lerpDefaultDistance = true;
			}
			if (pushBehind.HasValue)
			{
				localFlatPos = num6 * LerpTo(localFlatPos.normalized, -player.dir, pushBehind.Value * (1f - manualOrbitFactor));
			}
			else
			{
				localFlatPos = num6 * localFlatPos.normalized;
			}
			previousObstructedDistance = LerpTo(previousObstructedDistance, num4, 1f);
			if (!cam.mouseForOrbit && !prevSortaGrounded && sortaGrounded)
			{
				straightToNormalHeight = true;
			}
			if (flag2 || !sortaGrounded || cam.mouseForOrbit)
			{
				straightToNormalHeight = false;
			}
			float num7 = pos.y;
			if (heightOffset.HasValue)
			{
				num7 = Mathf.Lerp(lookAtPos.y + heightOffset.Value, num7, manualOrbitFactor);
			}
			else
			{
				float y = vector.y;
				if (pos.y < y - cam.maxBelowPlayer)
				{
					num7 = (pos.y = y - cam.maxBelowPlayer);
				}
				if (pos.y > y + cam.maxAbovePlayer)
				{
					num7 = (pos.y = y + cam.maxAbovePlayer);
				}
				if (straightToNormalHeight)
				{
					num7 = num2;
				}
				else if ((sortaGrounded && !flag2) & flag)
				{
					num7 = Mathf.Lerp(num2, num7, manualOrbitFactor);
				}
			}
			Vector3 vector3 = vector + localFlatPos;
			vector3.y = LerpTo(pos.y, num7, cam.camHeightSharpness);
			prevDragOffset = vector3 - lookAtPos;
			return vector3;
		}

		protected Vector3 HandleObstructions(Vector3 pos)
		{
			return HandleObstructions(pos, lookAtPos);
		}

		protected Vector3 HandleObstructions(Vector3 pos, Vector3 target)
		{
			Vector3 vector = target - pos;
			float num = vector.magnitude;
			Vector3 vector2 = -vector / num;
			int layerMask = 8388625;
			int num2 = Physics.SphereCastNonAlloc(target, cam.obstructionCheckRadius, vector2, obstructions, num, layerMask, QueryTriggerInteraction.Ignore);
			float num3 = float.MaxValue;
			for (int i = 0; i < num2; i++)
			{
				if (obstructions[i].distance > 0f && obstructions[i].distance < num3 && !cam.IgnoredColliders.Contains(obstructions[i].collider))
				{
					num3 = obstructions[i].distance;
				}
			}
			if (num3 < float.MaxValue)
			{
				num = (previousObstructedDistance = num3);
				wasObstructed = true;
			}
			else if (wasObstructed)
			{
				num = previousObstructedDistance;
				lerpDefaultDistance = true;
				wasObstructed = false;
			}
			return target + num * vector2;
		}

		protected void HandleTilt(float angle)
		{
			tilt = LerpTo(tilt, angle, 6f);
		}

		protected float CamHeightFactor()
		{
			float num = player.tf.up.y;
			if (num < 0f)
			{
				num *= 0.4f;
			}
			return num;
		}

		protected float LerpTo(float val, float target, float rate)
		{
			return Mathf.Lerp(target, val, Mathf.Exp((0f - rate) * dt));
		}

		protected Vector3 LerpTo(Vector3 val, Vector3 target, float rate)
		{
			return Vector3.Lerp(target, val, Mathf.Exp((0f - rate) * dt));
		}
	}
}
