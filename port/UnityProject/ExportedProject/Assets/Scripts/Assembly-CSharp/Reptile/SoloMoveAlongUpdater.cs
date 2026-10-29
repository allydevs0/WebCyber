using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(MoveAlongPoints))]
	public class SoloMoveAlongUpdater : BaseMoveHandler
	{
		private MoveAlongPoints_Hot moverHot;

		private MoveAlongPoints moverCold;

		private void Awake()
		{
			StageManager.OnStageInitialized += OnStageInitialized;
		}

		private void OnStageInitialized()
		{
			InitMoveHandler();
			Core.OnFixedUpdate += UpdateSoloMover;
		}

		public void OnDestroy()
		{
			Core.OnFixedUpdate -= UpdateSoloMover;
			StageManager.OnStageInitialized -= OnStageInitialized;
		}

		public override void HitboxHitPlayer(Player player, MoveAlongPoints hitMover)
		{
			if (moverHot.activelyMoving)
			{
				player.GetHit(1, (moverHot.tf.position - player.tf.position).normalized, KnockbackAbility.KnockbackType.FAR);
			}
		}

		public override void InitMoveHandler()
		{
			moverCold = GetComponentInChildren<MoveAlongPoints>();
			moverHot = SetPath(moverHot, moverCold.path, null);
			moverHot = moverCold.InitHotData(this, moverHot);
			moverHot = StartFromPoint(moverHot, moverCold.delay, moverCold.delayMultiplier, 0, moverCold.startOnRandomPoint ? Random.Range(0, moverHot.pathPositions.Length - 1) : 0);
		}

		public void UpdateSoloMover()
		{
			Vector3 position = WorldHandler.instance.GetCurrentPlayer().tf.position;
			float dt = Core.dt;
			bool isBusy = SequenceHandler.instance.isBusy;
			UpdateMovingObject(dt, position, isBusy);
		}

		public void UpdateMovingObject(float deltaTime, Vector3 playerPosition, bool sequenceHandlerIsBusy)
		{
			Vector3 position = moverHot.tf.position;
			if (moverHot.turnedOff || (sequenceHandlerIsBusy && moverHot.hasBody) || !moverHot.gameObject.activeInHierarchy)
			{
				return;
			}
			if (moverHot.timer > 0f && moverHot.posCount == 0)
			{
				moverHot.waitAtStartTimer += deltaTime;
				if (moverHot.waitAtStartTimer <= moverHot.waitAtStart)
				{
					return;
				}
			}
			if (moverHot.stopTimer > 0f)
			{
				if (moverHot.stopTimer < 1f && moverHot.stoppedForPlayer)
				{
					moverHot.stoppedForPlayer = false;
				}
				moverHot.stopTimer -= deltaTime;
				if (moverHot.stopTimer <= 0f)
				{
					moverHot.activelyMoving = true;
				}
			}
			if (!moverHot.activelyMoving)
			{
				moverHot.timeSpeed = Mathf.Max(moverHot.timeSpeed - moverHot.decc * deltaTime, 0f);
			}
			else
			{
				moverHot.timeSpeed = Mathf.Min(moverHot.timeSpeed + moverHot.acc * deltaTime, 1f);
			}
			moverHot.timer += deltaTime * moverHot.timeSpeed;
			float num = moverHot.timer * moverHot.speed;
			moverHot.fracJourney = num / moverHot.journeyLength + moverHot.leftOver;
			if (moverHot.fracJourney >= 1f || moverHot.journeyLength <= float.MinValue)
			{
				float num2 = (moverHot.fracJourney - 1f) * moverHot.journeyLength;
				if (moverHot.posCount + 1 >= moverHot.pathPositions.Length - 1)
				{
					moverHot.fracJourney = 1f;
					moverHot.waitAtEndTimer += deltaTime;
					if (moverHot.waitAtEndTimer > moverHot.waitAtEnd)
					{
						moverHot = NewTarget(moverHot, 0);
						moverHot.fracJourney = 0f;
						moverHot.leftOver = 0f;
					}
				}
				else
				{
					moverHot = NewTarget(moverHot, 0);
					moverHot.leftOver = num2 / moverHot.journeyLength;
					moverHot.fracJourney = moverHot.leftOver;
				}
			}
			if (moverHot.hasBody)
			{
				moverHot = UpdateMoverLODWithBody(moverHot, playerPosition, position);
				Vector3 position2 = moverHot.body.position;
				Quaternion rotation = moverHot.body.rotation;
				Vector3 vector = moverHot.nextPointPos - position2;
				if (vector == Vector3.zero)
				{
					vector = rotation * Vector3.forward;
				}
				if (moverHot.timer < 0f)
				{
					position2 = moverHot.lastPointPos - vectorUpTenThousand;
					if (!moverHot.doNotRotate)
					{
						moverHot.body.rotation = Quaternion.LookRotation(moverHot.nextPointPos - moverHot.lastPointPos);
					}
				}
				else
				{
					position2 = Vector3.Lerp(moverHot.lastPointPos, moverHot.nextPointPos, moverHot.fracJourney);
					moverHot.body.MovePosition(position2);
					if (!moverHot.doNotRotate)
					{
						moverHot.body.MoveRotation(Quaternion.Lerp(rotation, Quaternion.LookRotation(vector), deltaTime * (float)moverHot.turnSpeed * moverHot.timeSpeed));
					}
				}
				moverHot.lastPos = position2;
				return;
			}
			Vector3 vector2 = moverHot.nextPointPos - position;
			if (vector2 == Vector3.zero)
			{
				vector2 = moverHot.tf.forward;
			}
			if (moverHot.timer < 0f)
			{
				moverHot.tf.position = moverHot.lastPointPos - vectorUpTenThousand;
				if (!moverHot.doNotRotate)
				{
					moverHot.tf.rotation = Quaternion.LookRotation(moverHot.nextPointPos - moverHot.lastPointPos);
				}
			}
			else
			{
				moverHot.tf.position = Vector3.Lerp(moverHot.lastPointPos, moverHot.nextPointPos, moverHot.fracJourney);
				if (!moverHot.doNotRotate)
				{
					moverHot.tf.rotation = Quaternion.Lerp(moverHot.tf.rotation, Quaternion.LookRotation(vector2), deltaTime * (float)moverHot.turnSpeed * moverHot.timeSpeed);
				}
			}
			moverHot.lastPos = position;
		}

		public void ResetToPoint(int index)
		{
			StartFromPoint(moverHot, moverCold.delay, moverCold.delayMultiplier, index);
			moverHot.fracJourney = 0f;
			moverHot.posCount = 0;
		}

		public override void TriggerDetectLayer(int layer, MoveAlongPoints hitMover)
		{
			if (!moverHot.turnedOff && ((layer == 9 && hitMover.stopForPlayer) || (layer == 25 && hitMover.stopForStreetLife) || (layer == 17 && hitMover.stopForEnemies)))
			{
				moverHot.activelyMoving = false;
				moverHot.stopTimer = 1f;
				moverHot.stoppedForPlayer = true;
			}
		}

		protected override MoveAlongPoints_Hot NewTargetOnEndReached(MoveAlongPoints_Hot mover, int indexOfData)
		{
			if (mover.popBack)
			{
				mover.lastPoint = 0;
				mover.lastPointPos = mover.pathPositions[0];
				mover.nextPointPos = mover.pathPositions[1];
				mover.posCount = 0;
				SetPosToPoint(mover);
			}
			else
			{
				mover.nextPointPos = mover.pathPositions[0];
				mover.posCount = -1;
			}
			return mover;
		}

		protected override void SetPosToPoint(MoveAlongPoints_Hot mover)
		{
			Quaternion rotation = Quaternion.LookRotation(mover.nextPointPos - mover.lastPointPos);
			if (mover.hasBody)
			{
				if (!mover.doNotRotate)
				{
					mover.body.rotation = rotation;
				}
				mover.body.position = mover.lastPointPos;
			}
			else
			{
				if (!mover.doNotRotate)
				{
					mover.tf.rotation = rotation;
				}
				mover.tf.position = mover.lastPointPos;
			}
		}
	}
}
