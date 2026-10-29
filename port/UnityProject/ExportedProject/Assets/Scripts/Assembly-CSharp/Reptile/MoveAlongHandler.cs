using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Reptile
{
	public class MoveAlongHandler : BaseMoveHandler
	{
		private int queuedUp;

		public Transform path;

		private Vector3[] pathPositions;

		public float placementSpacingBuffer = 2f;

		public bool loopingPath;

		private Player currentPlayer;

		protected List<int> indexToMoveToSpare;

		protected List<MoveAlongPoints_Hot> moversHot;

		protected List<MoveAlongPoints_Hot> moversToSpareHot;

		protected List<MoveAlongPoints> moversCold;

		protected List<MoveAlongPoints> moversToSpareCold;

		public virtual void Awake()
		{
			StageManager.OnStageInitialized += OnStageInitialized;
		}

		public virtual void OnDestroy()
		{
			Core.OnFixedUpdate -= UpdateMoveAlongHandler;
			StageManager.OnStageInitialized -= OnStageInitialized;
			indexToMoveToSpare = null;
			moversHot = null;
			moversCold = null;
			moversToSpareHot = null;
			moversToSpareCold = null;
			currentPlayer = null;
			pathPositions = null;
			path = null;
		}

		public virtual void OnStageInitialized()
		{
			indexToMoveToSpare = new List<int>(10);
			moversHot = new List<MoveAlongPoints_Hot>(10);
			moversCold = new List<MoveAlongPoints>(10);
			InitMoveHandler();
			Core.OnFixedUpdate += UpdateMoveAlongHandler;
		}

		public override void InitMoveHandler()
		{
			int childCount = path.childCount;
			pathPositions = new Vector3[childCount];
			for (int i = 0; i < childCount; i++)
			{
				pathPositions[i] = path.GetChild(i).position;
			}
			MoveAlongPoints[] componentsInChildren = base.transform.GetComponentsInChildren<MoveAlongPoints>();
			moversToSpareHot = new List<MoveAlongPoints_Hot>(componentsInChildren.Length);
			moversToSpareCold = componentsInChildren.ToList();
			int count = moversToSpareCold.Count;
			for (int j = 0; j < count; j++)
			{
				MoveAlongPoints_Hot hotData = SetPath(default, path, pathPositions);
				moversToSpareHot.Add(componentsInChildren[j].InitHotData(this, hotData));
			}
			int num = Random.Range(0, count);
			float num2 = 0f;
			float num3 = CalculateLengthOfPath();
			int num4 = count;
			float b = num3 / (float)(count - 1);
			Vector3 vector = pathPositions[childCount - 1];
			float num5 = 0f;
			int num6 = 0;
			for (int num7 = childCount - 2; num7 >= 0; num7--)
			{
				Vector3 vector2 = pathPositions[num7];
				float magnitude = (vector2 - vector).magnitude;
				num5 += magnitude;
				num2 += magnitude;
				float num8 = num5 / num3 - (float)num6 / (float)num4;
				float num9 = 0f;
				if (moversHot.Count > 0)
				{
					int index = moversHot.Count - 1;
					float t = num8 * (float)num4;
					num9 = Mathf.Lerp(Mathf.Max(moversHot[index].collisionExtents + placementSpacingBuffer, b), moversHot[index].collisionExtents + placementSpacingBuffer, t);
				}
				else
				{
					num9 = Mathf.Max(moversToSpareHot[num].collisionExtents, b);
				}
				if (num2 > num9 || num7 == 0)
				{
					num2 = 0f;
					SetMoverToMove(moversToSpareHot[num], num, num7);
					num6++;
					int count2 = moversToSpareHot.Count;
					if (count2 == 0)
					{
						break;
					}
					num = Random.Range(0, count2);
				}
				vector = vector2;
			}
			if (loopingPath)
			{
				MoveAlongPoints_Hot value = moversHot[0];
				value.moverInFront = moversHot.Count - 1;
				moversHot[0] = value;
			}
			count = moversToSpareCold.Count;
			for (int k = 0; k < count - 1; k++)
			{
				moversToSpareCold[k].gameObject.SetActive(value: false);
			}
		}

		protected void UpdateMoveAlongHandler()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (queuedUp > 0)
			{
				int num = Random.Range(0, moversToSpareHot.Count);
				MoveAlongPoints_Hot moverSpareHot = moversToSpareHot[num];
				if (IsRoomAtStart(moverSpareHot.collisionExtents))
				{
					SetMoverToMove(moverSpareHot, num, 0);
					queuedUp--;
				}
			}
			Vector3 position = GetCurrentPlayer().tf.position;
			float dt = Core.dt;
			bool isBusy = SequenceHandler.instance.isBusy;
			UpdateMovingObject(dt, position, isBusy);
		}

		public void UpdateMovingObject(float deltaTime, Vector3 playerPosition, bool sequenceHandlerIsBusy)
		{
			for (int num = moversHot.Count - 1; num >= 0; num--)
			{
				MoveAlongPoints_Hot moveAlongPoints_Hot = moversHot[num];
				Vector3 position = moveAlongPoints_Hot.tf.position;
				if (moveAlongPoints_Hot.turnedOff || (moveAlongPoints_Hot.hasBody && sequenceHandlerIsBusy) || !moveAlongPoints_Hot.gameObject)
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
				moveAlongPoints_Hot = UpdateMoverLODWithBody(moveAlongPoints_Hot, playerPosition, position);
				Vector3 position2 = moveAlongPoints_Hot.body.position;
				Quaternion rotation = moveAlongPoints_Hot.body.rotation;
				Vector3 vector = moveAlongPoints_Hot.nextPointPos - position2;
				if (vector == Vector3.zero)
				{
					vector = rotation * Vector3.forward;
				}
				if (moveAlongPoints_Hot.timer < 0f)
				{
					position2 = moveAlongPoints_Hot.lastPointPos - vectorUpTenThousand;
					if (!moveAlongPoints_Hot.doNotRotate)
					{
						moveAlongPoints_Hot.body.rotation = Quaternion.LookRotation(moveAlongPoints_Hot.nextPointPos - moveAlongPoints_Hot.lastPointPos);
					}
				}
				else
				{
					position2 = Vector3.Lerp(moveAlongPoints_Hot.lastPointPos, moveAlongPoints_Hot.nextPointPos, moveAlongPoints_Hot.fracJourney);
					moveAlongPoints_Hot.body.MovePosition(position2);
					if (!moveAlongPoints_Hot.doNotRotate)
					{
						moveAlongPoints_Hot.body.MoveRotation(Quaternion.Lerp(rotation, Quaternion.LookRotation(vector), deltaTime * (float)moveAlongPoints_Hot.turnSpeed * moveAlongPoints_Hot.timeSpeed));
					}
				}
				moveAlongPoints_Hot.lastPos = position2;
				moversHot[num] = moveAlongPoints_Hot;
			}
			int num4 = moversHot.Count;
			for (int num5 = indexToMoveToSpare.Count - 1; num5 >= 0; num5--)
			{
				int num6 = indexToMoveToSpare[num5];
				SetMoverToSpare(moversHot[num6], num6);
				indexToMoveToSpare.RemoveAt(num5);
				num4 = num6;
			}
			for (int num7 = moversHot.Count - 1; num7 >= num4; num7--)
			{
				MoveAlongPoints_Hot value = moversHot[num7];
				value.moverInFront--;
				moversHot[num7] = value;
			}
		}

		private float CalculateLengthOfPath()
		{
			float num = 0f;
			int num2 = pathPositions.Length;
			for (int i = 0; i < num2 - 1; i++)
			{
				int num3 = Mathf.Max(i - 1, 0);
				num += (pathPositions[num3] - pathPositions[i]).magnitude;
			}
			return num;
		}

		public bool IsRoomAtStart(float moverLookingForRoomCollisionExtents)
		{
			int num = -1;
			int num2 = int.MaxValue;
			int count = moversHot.Count;
			if (count == 0)
			{
				return true;
			}
			for (int i = 0; i < count; i++)
			{
				int posCount = moversHot[i].posCount;
				if (posCount < num2)
				{
					num2 = posCount;
					num = i;
				}
			}
			MoveAlongPoints_Hot moveAlongPoints_Hot = moversHot[num];
			float magnitude = (moveAlongPoints_Hot.tf.position - pathPositions[0]).magnitude;
			float num3 = ((num == -1) ? 0f : moveAlongPoints_Hot.collisionExtents);
			return !(magnitude - num3 - placementSpacingBuffer < num3);
		}

		protected virtual void SetMoverToMove(MoveAlongPoints_Hot moverSpareHot, int indexOfData, int fromPoint)
		{
			MoveAlongPoints moveAlongPoints = moversToSpareCold[indexOfData];
			moversToSpareHot.RemoveAt(indexOfData);
			moversToSpareCold.RemoveAt(indexOfData);
			moveAlongPoints.gameObject.SetActive(value: true);
			indexOfData = moversHot.Count;
			moverSpareHot = StartFromPoint(moverSpareHot, moveAlongPoints.delay, moveAlongPoints.delayMultiplier, indexOfData, fromPoint);
			moversHot.Add(moverSpareHot);
			moversCold.Add(moveAlongPoints);
		}

		protected virtual void SetMoverToSpare(MoveAlongPoints_Hot moverHot, int indexOfData)
		{
			MoveAlongPoints moveAlongPoints = moversCold[indexOfData];
			moveAlongPoints.gameObject.SetActive(value: false);
			moversHot.RemoveAt(indexOfData);
			moversCold.RemoveAt(indexOfData);
			moversToSpareHot.Add(moverHot);
			moversToSpareCold.Add(moveAlongPoints);
			queuedUp++;
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

		private Player GetCurrentPlayer()
		{
			if (currentPlayer == null)
			{
				currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			}
			return currentPlayer;
		}

		protected override void SetPosToPoint(MoveAlongPoints_Hot mover)
		{
			Quaternion rotation = Quaternion.LookRotation(mover.nextPointPos - mover.lastPointPos);
			if (!mover.doNotRotate)
			{
				mover.body.rotation = rotation;
			}
			mover.body.position = mover.lastPointPos;
		}

		protected override MoveAlongPoints_Hot NewTargetOnEndReached(MoveAlongPoints_Hot moverHot, int indexOfData)
		{
			if (!loopingPath)
			{
				indexToMoveToSpare.Add(indexOfData);
			}
			else
			{
				moverHot.posCount = -1;
				moverHot.nextPointPos = moverHot.pathPositions[0];
			}
			return moverHot;
		}
	}
}
