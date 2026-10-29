using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public abstract class BaseMoveHandler : MonoBehaviour
	{
		protected readonly Vector3 vectorUpTenThousand = Vector3.up * 10000f;

		public abstract void InitMoveHandler();

		public abstract void HitboxHitPlayer(Player player, MoveAlongPoints hitMover);

		public abstract void TriggerDetectLayer(int layer, MoveAlongPoints hitMover);

		protected abstract void SetPosToPoint(MoveAlongPoints_Hot mover);

		protected abstract MoveAlongPoints_Hot NewTargetOnEndReached(MoveAlongPoints_Hot mover, int indexOfData);

		public MoveAlongPoints_Hot SetPath(MoveAlongPoints_Hot moverHot, Transform pathToSet, Vector3[] pathPositionToSet)
		{
			moverHot.path = pathToSet;
			if (pathPositionToSet == null)
			{
				Transform[] allChildren = pathToSet.GetAllChildren();
				pathPositionToSet = new Vector3[allChildren.Length];
				for (int i = 0; i < allChildren.Length; i++)
				{
					pathPositionToSet[i] = allChildren[i].position;
				}
			}
			moverHot.pathPositions = pathPositionToSet;
			return moverHot;
		}

		public MoveAlongPoints_Hot StartFromPoint(MoveAlongPoints_Hot moverHot, float delay, float delayMultiplier, int indexOfData, int point = 0)
		{
			int num = (moverHot.lastPoint = point);
			moverHot.lastPointPos = moverHot.pathPositions[num];
			moverHot.nextPointPos = moverHot.pathPositions[num + 1];
			SetPosToPoint(moverHot);
			moverHot.journeyLength = Vector3.Distance(moverHot.lastPointPos, moverHot.nextPointPos);
			moverHot.timer = 0f - delay * delayMultiplier;
			moverHot.timeSpeed = 1f;
			moverHot.waitAtEndTimer = 0f;
			moverHot.posCount = moverHot.lastPoint;
			moverHot.leftOver = 0f;
			moverHot.moverInFront = indexOfData - 1;
			return moverHot;
		}

		protected bool IsRoomOnPathInFront(Vector3 myPosition, MoveAlongPoints_Hot moverHot, List<MoveAlongPoints_Hot> moversHot)
		{
			if (moverHot.moverInFront != -1)
			{
				MoveAlongPoints_Hot moveAlongPoints_Hot = moversHot[moverHot.moverInFront];
				if (!moveAlongPoints_Hot.activelyMoving)
				{
					return !((moveAlongPoints_Hot.tf.position - myPosition).sqrMagnitude - moverHot.collisionExtents < moveAlongPoints_Hot.collisionExtents);
				}
			}
			return true;
		}

		protected MoveAlongPoints_Hot NewTarget(MoveAlongPoints_Hot moverHot, int indexOfData)
		{
			moverHot.posCount++;
			moverHot.timer = 0f;
			moverHot.lastPoint = moverHot.posCount;
			moverHot.lastPointPos = moverHot.pathPositions[moverHot.posCount];
			if (moverHot.posCount < moverHot.pathPositions.Length - 1)
			{
				moverHot.nextPointPos = moverHot.pathPositions[moverHot.posCount + 1];
			}
			else
			{
				moverHot = NewTargetOnEndReached(moverHot, indexOfData);
				moverHot.waitAtEndTimer = 0f;
				moverHot.waitAtStartTimer = 0f;
			}
			moverHot.journeyLength = Vector3.Distance(moverHot.lastPointPos, moverHot.nextPointPos);
			return moverHot;
		}

		public virtual MoveAlongPoints_Hot UpdateMoverLODWithBody(MoveAlongPoints_Hot moverHot, Vector3 playerPos, Vector3 myPosition)
		{
			MoverLOD moverLOD = ((!((myPosition - playerPos).sqrMagnitude < 1000f)) ? MoverLOD.NO_DETAILS : MoverLOD.DETAILS);
			if (moverLOD != moverHot.moverLOD)
			{
				moverHot.moverLOD = moverLOD;
				moverHot.body.interpolation = ((moverHot.moverLOD == MoverLOD.DETAILS) ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
			}
			return moverHot;
		}
	}
}
