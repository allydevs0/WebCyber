using UnityEngine;

namespace Reptile
{
	public class StreetLifeBehaviour
	{
		internal const float minAudioInterval = 2f;

		internal const float minJostleInterval = 2f;

		public virtual void UpdateStreetLife(StreetLifeCluster cluster, StreetLifeHot[] streetLifeHotArray, int startIndex, int length)
		{
			float dt = Core.dt;
			for (int i = startIndex; i < startIndex + length; i++)
			{
				if (streetLifeHotArray[i].isShook)
				{
					VerticalSpeed(ref streetLifeHotArray[i], dt);
					if (streetLifeHotArray[i].moveTimer < streetLifeHotArray[i].moveAwayDuration)
					{
						FlatSpeed(ref streetLifeHotArray[i], dt);
						streetLifeHotArray[i].moveTimer += dt;
					}
					else if (streetLifeHotArray[i].isGrounded)
					{
						streetLifeHotArray[i].isShook = false;
						streetLifeHotArray[i].trigger.enabled = true;
					}
				}
				if (streetLifeHotArray[i].wasShook)
				{
					MoveAwayIdleAnim(ref streetLifeHotArray[i]);
					if (IsNotInView(cluster, ref streetLifeHotArray[i]))
					{
						ResetBack(cluster, ref streetLifeHotArray[i]);
					}
				}
			}
		}

		public virtual void Move(StreetLifeCluster cluster, StreetLife life, ref StreetLifeHot streetLifeHot, Transform otherTransform)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(streetLifeHot.transform.position - otherTransform.position, Vector3.up).normalized;
			Vector3 rhs = normalized * -1f;
			streetLifeHot.curSpeed = streetLifeHot.moveAwaySpeed;
			if (streetLifeHot.moveAlongPoints != null)
			{
				cluster.SetMoverOff(streetLifeHot.moveAlongPoints, setOff: true);
			}
			streetLifeHot.isShook = true;
			streetLifeHot.wasShook = true;
			streetLifeHot.moveTimer = 0f;
			streetLifeHot.trigger.enabled = false;
			MoveAwayAnim(ref streetLifeHot);
			if (streetLifeHot.collided)
			{
				streetLifeHot.moveDir = Vector3.ProjectOnPlane(Random.insideUnitSphere, Vector3.up).normalized;
			}
			else
			{
				bool flag = otherTransform.gameObject.layer == 18 || otherTransform.gameObject.layer == 9;
				if (flag && WorldHandler.instance.currentEncounter != null && life.GetType() == typeof(Pedestrian))
				{
					WorldHandler.instance.currentEncounter.JostleStreetlifeSignal();
				}
				if ((life.reportJostleAsCrime && life.lastJostleTime + 2f < Time.time) & flag)
				{
					WantedManager.instance.CrimePerpetrated(WantedManager.Crime.PEDESTRIAN_JOSTLE);
					life.lastJostleTime = Time.time;
				}
				if (life.lastAudioTime + 2f < Time.time)
				{
					life.PlayJostleSound();
				}
				if (otherTransform.gameObject.layer == 9)
				{
					if (Vector3.Dot(otherTransform.right, normalized) > 0f)
					{
						streetLifeHot.moveDir = Vector3.ProjectOnPlane(otherTransform.right, Vector3.up).normalized;
					}
					else
					{
						streetLifeHot.moveDir = Vector3.ProjectOnPlane(otherTransform.right * -1f, Vector3.up).normalized;
					}
				}
				else
				{
					streetLifeHot.moveDir = Vector3.ProjectOnPlane(Random.insideUnitSphere, Vector3.up).normalized;
					if (Vector3.Dot(streetLifeHot.moveDir, rhs) > 0f)
					{
						streetLifeHot.moveDir *= -1f;
					}
				}
			}
			streetLifeHot.transform.rotation = Quaternion.LookRotation(streetLifeHot.moveDir * -1f);
			streetLifeHot.collided = false;
		}

		public void VerticalSpeed(ref StreetLifeHot streetLifeHot, float coreDeltaTime)
		{
			if (streetLifeHot.verticalDir.y < 0f)
			{
				float num = 0.5f;
				Vector3 origin = streetLifeHot.transform.position + Vector3.up * num;
				num += 0.1f;
				int layerMask = 1;
				if (Physics.Raycast(origin, Vector3.down, out var hitInfo, num, layerMask))
				{
					streetLifeHot.verticalSpeed = 0f;
					streetLifeHot.isGrounded = true;
					streetLifeHot.transform.position = hitInfo.point;
				}
				else
				{
					ApplyVerticalSpeed(ref streetLifeHot, coreDeltaTime);
				}
			}
			else
			{
				ApplyVerticalSpeed(ref streetLifeHot, coreDeltaTime);
			}
		}

		public void FlatSpeed(ref StreetLifeHot streetLifeHot, float coreDeltaTime)
		{
			float num = 0.4f;
			float maxDistance = num + streetLifeHot.curSpeed * coreDeltaTime;
			float num2 = 0.4f;
			Vector3 origin = streetLifeHot.transform.position + Vector3.up * num2;
			int layerMask = 1041;
			if (Physics.Raycast(origin, streetLifeHot.moveDir, out var hitInfo, maxDistance, layerMask))
			{
				streetLifeHot.collided = true;
				streetLifeHot.curSpeed = 0f;
				streetLifeHot.transform.position = hitInfo.point - num * streetLifeHot.moveDir * 1.05f - Vector3.up * num2;
			}
			else
			{
				streetLifeHot.transform.position += streetLifeHot.moveDir * streetLifeHot.curSpeed * coreDeltaTime;
			}
		}

		public void ApplyVerticalSpeed(ref StreetLifeHot streetLifeHot, float coreDeltaTime)
		{
			streetLifeHot.verticalSpeed += streetLifeHot.verticalAcc * coreDeltaTime;
			if (streetLifeHot.verticalSpeed > streetLifeHot.maxVerticalSpeed)
			{
				streetLifeHot.verticalSpeed = streetLifeHot.maxVerticalSpeed;
			}
			streetLifeHot.transform.position += streetLifeHot.verticalDir * streetLifeHot.verticalSpeed * coreDeltaTime;
			streetLifeHot.isGrounded = false;
		}

		public void MoveAwayAnim(ref StreetLifeHot streetLifeHot)
		{
			streetLifeHot.currentAnim = streetLifeHot.moveAwayAnimation;
			streetLifeHot.anim.Play(streetLifeHot.currentAnim, 0, 0f);
		}

		public void MoveAwayIdleAnim(ref StreetLifeHot streetLifeHot)
		{
		}

		public int GetNormalAnimation(ref StreetLifeHot streetLifeHot)
		{
			int num = 0;
			if (streetLifeHot.moveAlongPoints != null)
			{
				return streetLifeHot.walkAnimation;
			}
			return streetLifeHot.idleAnimation;
		}

		public void NormalAnim(ref StreetLifeHot streetLifeHot, bool randomOffset = false)
		{
			int normalAnimation = GetNormalAnimation(ref streetLifeHot);
			float normalizedTime = streetLifeHot.anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
			if (randomOffset)
			{
				normalizedTime = Random.value;
			}
			streetLifeHot.currentAnim = normalAnimation;
			streetLifeHot.anim.Play(normalAnimation, 0, normalizedTime);
		}

		private static void PlayAnim(ref StreetLifeHot streetLifeHot, int animation, float normalizedTime = 0f)
		{
			streetLifeHot.currentAnim = animation;
			streetLifeHot.anim.Play(animation);
		}

		public bool IsNotInView(StreetLifeCluster cluster, ref StreetLifeHot streetLifeHot)
		{
			WorldHandler instance = WorldHandler.instance;
			if (!instance.IsKindaInScreen(streetLifeHot.transform.position) && !instance.IsKindaInScreen(streetLifeHot.startPos) && (streetLifeHot.attachedJunk == null || (!instance.IsKindaInScreen(streetLifeHot.attachedJunk.transform.position) && !instance.IsKindaInScreen(streetLifeHot.attachedJunk.originPosition))))
			{
				if (!(streetLifeHot.moveAlongPoints == null))
				{
					return !instance.IsKindaInScreen(cluster.GetMoverLastPos(streetLifeHot.moveAlongPoints));
				}
				return true;
			}
			return false;
		}

		public void ResetBack(StreetLifeCluster cluster, ref StreetLifeHot streetLifeHot, bool restartPath = true)
		{
			streetLifeHot.isShook = false;
			streetLifeHot.wasShook = false;
			streetLifeHot.isGrounded = true;
			streetLifeHot.collided = false;
			streetLifeHot.trigger.enabled = true;
			if (streetLifeHot.moveAlongPoints != null)
			{
				cluster.SetMoverOff(streetLifeHot.moveAlongPoints, setOff: false);
				if (restartPath)
				{
					cluster.RestartMoverPath(streetLifeHot.moveAlongPoints);
				}
			}
			else
			{
				streetLifeHot.transform.position = streetLifeHot.startPos;
				streetLifeHot.transform.rotation = streetLifeHot.startRot;
			}
			if (streetLifeHot.activationGameObject.activeInHierarchy)
			{
				NormalAnim(ref streetLifeHot, randomOffset: true);
			}
			if (streetLifeHot.attachedJunk != null)
			{
				streetLifeHot.attachedJunk.Restore();
			}
		}
	}
}
