using UnityEngine;

namespace Reptile
{
	public class PlayerAIWaypoint : MonoBehaviour
	{
		public enum JumpPointBehavoir
		{
			REACH_WITH_AIRDASH_IF_FAR = 0,
			AIRDASH_AT_JUMP_POINT = 1,
			TRICK_AT_JUMP_POINT = 2,
			JUST_MOVE_TO_JUMP_POINT = 3
		}

		public bool boost;

		public bool skipIfCantReach;

		public bool walk;

		public bool unequipMovestyle;

		public bool slide;

		[Header("trick input can interfere with jumps from grinds")]
		public bool trick;

		public float wait;

		public float grindTiltCorner;

		[Header("if checked the AI gets teleported to the next waypoint")]
		public bool autoTeleport;

		public bool dontCheatTeleportTo;

		public bool ignoreObstructions;

		internal bool hasTriggerPlayerNeedsToPass;

		public JumpPointBehavoir jumpPointBehavoir;

		internal Vector3 position => base.transform.position;

		internal Vector3 jumpPosition => base.transform.GetChild(0).position;

		internal float jumpPointScale => base.transform.GetChild(0).localScale.x;

		public float GetFullJumpDistance()
		{
			return (position - jumpPosition).magnitude;
		}

		public bool IsJumper()
		{
			return base.transform.childCount > 0;
		}

		public bool IsRunner()
		{
			return base.transform.childCount == 0;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			if (base.transform.parent.gameObject.name.Contains("_B"))
			{
				Gizmos.color = Color.red;
			}
			else if (base.transform.parent.gameObject.name.Contains("_C"))
			{
				Gizmos.color = Color.yellow;
			}
			else if (base.transform.parent.gameObject.name.Contains("_D"))
			{
				Gizmos.color = Color.green;
			}
			else if (base.transform.parent.gameObject.name.Contains("_E"))
			{
				Gizmos.color = Color.magenta;
			}
			else if (base.transform.parent.gameObject.name.Contains("_F"))
			{
				Gizmos.color = Color.black;
			}
			else if (base.transform.parent.gameObject.name.Contains("_NPC"))
			{
				Gizmos.color = Color.cyan;
			}
			if (IsRunner())
			{
				if (base.transform.GetSiblingIndex() == base.transform.parent.childCount - 1)
				{
					Gizmos.color = new Color(Gizmos.color.r - 0.6f, Gizmos.color.g - 0.6f, Gizmos.color.b - 0.6f);
				}
				else if (base.transform.GetSiblingIndex() == 0)
				{
					Gizmos.color += Color.white * 0.25f;
				}
			}
			if (IsJumper())
			{
				if (base.transform.childCount > 0)
				{
					Gizmos.DrawSphere(base.transform.GetChild(0).position, 0.5f * base.transform.GetChild(0).localScale.x);
					Gizmos.DrawLine(base.transform.GetChild(0).position, base.transform.position);
				}
				else
				{
					Gizmos.color = Color.magenta;
				}
			}
			if (base.transform.parent.childCount > base.transform.GetSiblingIndex() + 1)
			{
				Gizmos.DrawLine(base.transform.position, base.transform.parent.GetChild(base.transform.GetSiblingIndex() + 1).position);
			}
			Gizmos.DrawSphere(base.transform.position, base.transform.localScale.x);
		}
	}
}
