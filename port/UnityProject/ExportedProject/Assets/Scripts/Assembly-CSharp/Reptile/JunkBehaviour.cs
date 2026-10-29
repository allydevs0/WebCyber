using System.Runtime.CompilerServices;
using UnityEngine;

namespace Reptile
{
	public class JunkBehaviour
	{
		private const float tooFarDistanceSquared = 14400f;

		internal Junk[] totalJunk;

		internal int kickedJunkIndex;

		internal int nonupdatingJunkIndex;

		internal float junkCheckTimer;

		internal void Init(Junk[] junk)
		{
			totalJunk = junk;
			kickedJunkIndex = 0;
			nonupdatingJunkIndex = 0;
			junkCheckTimer = 0f;
			for (int i = 0; i < totalJunk.Length; i++)
			{
				totalJunk[i].Init(this, i);
			}
		}

		internal static void JunkUpdate(JunkBehaviour junkBehaviour, float deltaTime)
		{
			junkBehaviour.junkCheckTimer -= deltaTime;
			for (int num = junkBehaviour.kickedJunkIndex - 1; num >= 0; num--)
			{
				Junk junk = junkBehaviour.totalJunk[num];
				junk.rehitTimer = Mathf.Max(0f, junk.rehitTimer - deltaTime);
				if (junkBehaviour.junkCheckTimer <= 0f && (junk.originPosition - junk.transform.position).sqrMagnitude > 14400f)
				{
					SetJunkToNotUpdate(junkBehaviour, junk);
				}
				if (junk.destroyTimer > 0f)
				{
					junk.destroyTimer = Mathf.Max(0f, junk.destroyTimer - deltaTime);
					if (junk.destroyTimer <= 0f)
					{
						SetJunkToNotUpdate(junkBehaviour, junk);
					}
				}
			}
			int num2 = junkBehaviour.kickedJunkIndex;
			while (num2 < junkBehaviour.nonupdatingJunkIndex)
			{
				Junk junk2 = junkBehaviour.totalJunk[num2];
				junk2.hitPauseTimer -= deltaTime;
				num2++;
				if (junk2.hitPauseTimer <= 0f)
				{
					FallApart(junkBehaviour, junk2, byBump: false);
					junk2.rigidBody.AddForceAtPosition(junk2.hitForce, junk2.hitPosition, ForceMode.Impulse);
					num2 = junkBehaviour.kickedJunkIndex;
				}
			}
			if (junkBehaviour.junkCheckTimer <= 0f)
			{
				junkBehaviour.junkCheckTimer = 1f;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsJunkNonUpdating(int nonupdatingJunkIndex, int junkIndex)
		{
			return junkIndex >= nonupdatingJunkIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsJunkMoving(int kickedJunkIndex, int junkIndex)
		{
			return junkIndex < kickedJunkIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsJunkKicked(int kickedJunkIndex, int nonupdatingJunkIndex, int junkIndex)
		{
			if (junkIndex >= kickedJunkIndex)
			{
				return junkIndex < nonupdatingJunkIndex;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void SwapJunk(Junk[] totalJunk, int junkToMoveIndex, int indexToMoveTo)
		{
			Junk junk = totalJunk[junkToMoveIndex];
			(totalJunk[junkToMoveIndex] = totalJunk[indexToMoveTo])._index = junkToMoveIndex;
			totalJunk[indexToMoveTo] = junk;
			junk._index = indexToMoveTo;
		}

		internal static void MoveToKickedJunk(JunkBehaviour junkBehaviour, Junk junkToMove, float hitPauseDuration, float rehitTimer)
		{
			int index = junkToMove._index;
			if (IsJunkNonUpdating(junkBehaviour.nonupdatingJunkIndex, index))
			{
				SwapJunk(junkBehaviour.totalJunk, index, junkBehaviour.nonupdatingJunkIndex);
				junkBehaviour.nonupdatingJunkIndex++;
			}
			else
			{
				SwapJunk(junkBehaviour.totalJunk, index, junkBehaviour.kickedJunkIndex - 1);
				junkBehaviour.kickedJunkIndex--;
			}
			junkToMove.hitPauseTimer = hitPauseDuration;
			junkToMove.rehitTimer = rehitTimer;
			junkToMove.rigidBody.isKinematic = true;
		}

		internal static void MoveToMovingJunk(JunkBehaviour junkBehaviour, Junk junkToMove)
		{
			int index = junkToMove._index;
			SwapJunk(junkBehaviour.totalJunk, index, junkBehaviour.kickedJunkIndex);
			junkBehaviour.kickedJunkIndex++;
			junkBehaviour.nonupdatingJunkIndex = Mathf.Max(junkBehaviour.kickedJunkIndex, junkBehaviour.nonupdatingJunkIndex);
		}

		internal static void MoveToInactiveWithoutDeactivate(JunkBehaviour junkBehaviour, Junk junkToMove)
		{
			int index = junkToMove._index;
			int num = Mathf.Max(0, junkBehaviour.kickedJunkIndex - 1);
			int indexToMoveTo = Mathf.Max(0, junkBehaviour.nonupdatingJunkIndex - 1);
			SwapJunk(junkBehaviour.totalJunk, index, num);
			SwapJunk(junkBehaviour.totalJunk, num, indexToMoveTo);
			junkBehaviour.kickedJunkIndex--;
			junkBehaviour.nonupdatingJunkIndex--;
			junkToMove.rehitTimer = 0f;
			junkToMove.deactiveCounter = 0;
		}

		internal static void SetJunkToNotUpdate(JunkBehaviour junkBehaviour, Junk junkToMove)
		{
			MoveToInactiveWithoutDeactivate(junkBehaviour, junkToMove);
			junkToMove.gameObject.SetActive(value: false);
		}

		internal static void SetJunkToNotUpdateButActive(JunkBehaviour junkBehaviour, Junk junkToMove)
		{
			int index = junkToMove._index;
			if (IsJunkNonUpdating(junkBehaviour.nonupdatingJunkIndex, index))
			{
				junkToMove.gameObject.SetActive(value: true);
				return;
			}
			if (IsJunkKicked(junkBehaviour.kickedJunkIndex, junkBehaviour.nonupdatingJunkIndex, index))
			{
				SwapJunk(junkBehaviour.totalJunk, index, junkBehaviour.nonupdatingJunkIndex - 1);
				junkBehaviour.nonupdatingJunkIndex--;
				return;
			}
			SwapJunk(junkBehaviour.totalJunk, index, junkBehaviour.kickedJunkIndex - 1);
			SwapJunk(junkBehaviour.totalJunk, junkBehaviour.kickedJunkIndex - 1, junkBehaviour.nonupdatingJunkIndex - 1);
			junkBehaviour.kickedJunkIndex--;
			junkBehaviour.nonupdatingJunkIndex--;
		}

		internal static void FallApart(JunkBehaviour junkBehaviour, Junk junk, bool byBump)
		{
			if (!byBump || junk.interactOn != Junk.Interact.ON_HITBOX)
			{
				junk.rigidBody.isKinematic = false;
				MoveToMovingJunk(junkBehaviour, junk);
				if (junk.junkHolder != null && !junk.junkHolder.moved)
				{
					junk.junkHolder.FallApart(junk.hitPosition, byBump);
				}
			}
		}

		internal static void FallApartFromJunk(JunkBehaviour junkBehaviour, Junk junk, bool byBump)
		{
			if (!IsJunkMoving(junkBehaviour.kickedJunkIndex, junk._index))
			{
				FallApart(junkBehaviour, junk, byBump);
			}
		}

		internal static void RestoreSingle(JunkBehaviour junkBehaviour, Junk junk)
		{
			junk.transform.position = junk.originPosition;
			junk.transform.rotation = junk.originRotation;
			junk.rigidBody.velocity = Vector3.zero;
			junk.rigidBody.angularVelocity = Vector3.zero;
			junk.rigidBody.isKinematic = true;
			junk.rehitTimer = 0f;
			junk.deactiveCounter = 0;
			SetJunkToNotUpdateButActive(junkBehaviour, junk);
			if (junk.junkHolder != null)
			{
				junk.junkHolder.Restore();
			}
			if (junk.interactOn == Junk.Interact.ON_AWAKE)
			{
				FallApart(junkBehaviour, junk, byBump: false);
			}
		}

		internal static void RestoreAll(JunkBehaviour junkBehaviour)
		{
			junkBehaviour.kickedJunkIndex = 0;
			junkBehaviour.nonupdatingJunkIndex = 0;
			for (int i = 1; i < junkBehaviour.totalJunk.Length; i++)
			{
				Junk junk = junkBehaviour.totalJunk[i];
				RestoreSingle(junkBehaviour, junk);
			}
		}
	}
}
