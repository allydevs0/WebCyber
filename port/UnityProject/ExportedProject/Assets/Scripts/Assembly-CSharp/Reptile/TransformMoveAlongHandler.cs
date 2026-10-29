using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public abstract class TransformMoveAlongHandler : BaseMoveHandler
	{
		protected List<MoveAlongPoints_Hot> moversHot;

		protected List<MoveAlongPoints> moversCold;

		public override void InitMoveHandler()
		{
		}

		public virtual void OnDestroy()
		{
			if (moversHot != null)
			{
				moversHot.Clear();
			}
			if (moversCold != null)
			{
				moversCold.Clear();
			}
			moversHot = null;
			moversCold = null;
		}

		public void UpdateMovingObjects(float deltaTime, Vector3 playerPosition, bool sequenceHandlerIsBusy)
		{
			for (int num = moversHot.Count - 1; num >= 0; num--)
			{
				MoveAlongPoints_Hot moveAlongPoints_Hot = moversHot[num];
				Vector3 position = moveAlongPoints_Hot.tf.position;
				if (moveAlongPoints_Hot.turnedOff || (sequenceHandlerIsBusy && moveAlongPoints_Hot.hasBody) || !moveAlongPoints_Hot.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (moveAlongPoints_Hot.timer > 0f)
				{
					if (moveAlongPoints_Hot.checkNotToHitOthersOnThePath && !IsRoomOnPathInFront(position, moveAlongPoints_Hot, moversHot))
					{
						moveAlongPoints_Hot.activelyMoving = false;
						moveAlongPoints_Hot.stopTimer = 1f;
					}
					if (moveAlongPoints_Hot.posCount == 0)
					{
						moveAlongPoints_Hot.waitAtStartTimer += deltaTime;
						if (moveAlongPoints_Hot.waitAtStartTimer <= moveAlongPoints_Hot.waitAtStart)
						{
							moversHot[num] = moveAlongPoints_Hot;
							continue;
						}
					}
				}
				if (moveAlongPoints_Hot.stopTimer > 0f)
				{
					if (moveAlongPoints_Hot.stopTimer < 1f && moveAlongPoints_Hot.stoppedForPlayer)
					{
						moveAlongPoints_Hot.stoppedForPlayer = false;
					}
					moveAlongPoints_Hot.stopTimer -= deltaTime;
					if (moveAlongPoints_Hot.stopTimer <= 0f)
					{
						moveAlongPoints_Hot.activelyMoving = true;
					}
				}
				if (!moveAlongPoints_Hot.activelyMoving)
				{
					moveAlongPoints_Hot.timeSpeed = Mathf.Max(moveAlongPoints_Hot.timeSpeed - moveAlongPoints_Hot.decc * deltaTime, 0f);
				}
				else
				{
					moveAlongPoints_Hot.timeSpeed = Mathf.Min(moveAlongPoints_Hot.timeSpeed + moveAlongPoints_Hot.acc * deltaTime, 1f);
				}
				moveAlongPoints_Hot.timer += deltaTime * moveAlongPoints_Hot.timeSpeed;
				float num2 = moveAlongPoints_Hot.timer * moveAlongPoints_Hot.speed;
				moveAlongPoints_Hot.fracJourney = num2 / moveAlongPoints_Hot.journeyLength + moveAlongPoints_Hot.leftOver;
				if (moveAlongPoints_Hot.fracJourney >= 1f || moveAlongPoints_Hot.journeyLength <= float.MinValue)
				{
					float num3 = (moveAlongPoints_Hot.fracJourney - 1f) * moveAlongPoints_Hot.journeyLength;
					if (moveAlongPoints_Hot.posCount + 1 >= moveAlongPoints_Hot.pathPositions.Length - 1)
					{
						moveAlongPoints_Hot.fracJourney = 1f;
						moveAlongPoints_Hot.waitAtEndTimer += deltaTime;
						if (moveAlongPoints_Hot.waitAtEndTimer > moveAlongPoints_Hot.waitAtEnd)
						{
							moveAlongPoints_Hot = NewTarget(moveAlongPoints_Hot, num);
							moveAlongPoints_Hot.fracJourney = 0f;
							moveAlongPoints_Hot.leftOver = 0f;
						}
					}
					else
					{
						moveAlongPoints_Hot = NewTarget(moveAlongPoints_Hot, num);
						moveAlongPoints_Hot.leftOver = num3 / moveAlongPoints_Hot.journeyLength;
						moveAlongPoints_Hot.fracJourney = moveAlongPoints_Hot.leftOver;
					}
				}
				Vector3 vector = moveAlongPoints_Hot.nextPointPos - position;
				if (vector == Vector3.zero)
				{
					vector = moveAlongPoints_Hot.tf.forward;
				}
				if (moveAlongPoints_Hot.timer < 0f)
				{
					moveAlongPoints_Hot.tf.position = moveAlongPoints_Hot.lastPointPos - vectorUpTenThousand;
					if (!moveAlongPoints_Hot.doNotRotate)
					{
						moveAlongPoints_Hot.tf.rotation = Quaternion.LookRotation(moveAlongPoints_Hot.nextPointPos - moveAlongPoints_Hot.lastPointPos);
					}
				}
				else
				{
					moveAlongPoints_Hot.tf.position = Vector3.Lerp(moveAlongPoints_Hot.lastPointPos, moveAlongPoints_Hot.nextPointPos, moveAlongPoints_Hot.fracJourney);
					if (!moveAlongPoints_Hot.doNotRotate)
					{
						moveAlongPoints_Hot.tf.rotation = Quaternion.Lerp(moveAlongPoints_Hot.tf.rotation, Quaternion.LookRotation(vector), deltaTime * (float)moveAlongPoints_Hot.turnSpeed * moveAlongPoints_Hot.timeSpeed);
					}
				}
				moveAlongPoints_Hot.lastPos = position;
				moversHot[num] = moveAlongPoints_Hot;
			}
		}

		protected override MoveAlongPoints_Hot NewTargetOnEndReached(MoveAlongPoints_Hot moverHot, int indexOfData)
		{
			if (moverHot.popBack)
			{
				moverHot.lastPoint = 0;
				moverHot.lastPointPos = moverHot.pathPositions[0];
				moverHot.nextPointPos = moverHot.pathPositions[1];
				moverHot.posCount = 0;
				SetPosToPoint(moverHot);
			}
			else
			{
				moverHot.nextPointPos = moverHot.pathPositions[0];
				moverHot.posCount = -1;
			}
			return moverHot;
		}

		protected override void SetPosToPoint(MoveAlongPoints_Hot mover)
		{
			Quaternion rotation = Quaternion.LookRotation(mover.nextPointPos - mover.lastPointPos);
			if (!mover.doNotRotate)
			{
				mover.tf.rotation = rotation;
			}
			mover.tf.position = mover.lastPointPos;
		}

		public override void HitboxHitPlayer(Player player, MoveAlongPoints hitMover)
		{
			int num = moversCold.FindIndexOf(hitMover);
			if (num != -1)
			{
				MoveAlongPoints_Hot moveAlongPoints_Hot = moversHot[num];
				if (moveAlongPoints_Hot.activelyMoving)
				{
					player.GetHit(1, (moveAlongPoints_Hot.tf.position - player.tf.position).normalized, KnockbackAbility.KnockbackType.FAR);
				}
			}
		}

		public override void TriggerDetectLayer(int layer, MoveAlongPoints hitMover)
		{
			int num = moversCold.FindIndexOf(hitMover);
			if (num != -1)
			{
				MoveAlongPoints_Hot value = moversHot[num];
				if (!value.turnedOff && ((layer == 9 && hitMover.stopForPlayer) || (layer == 25 && hitMover.stopForStreetLife) || (layer == 17 && hitMover.stopForEnemies)))
				{
					value.activelyMoving = false;
					value.stopTimer = 1f;
					value.stoppedForPlayer = true;
					moversHot[num] = value;
				}
			}
		}
	}
}
