using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Reptile
{
	public class Teleport : MonoBehaviour
	{
		private Coroutine teleportRoutine;

		[Header("In case of a teleport, it will place you at the spawner of that teleport")]
		[Header("You can put a teleport OR a spawner in here, both work")]
		public Transform teleportTo;

		public bool giveSpeedAtSpawn = true;

		public bool letAIGoToTheNextWaypoint;

		public bool automaticallyReturnPlayerToLastSafeLocation;

		public bool doDamage;

		private float fadeToBlackDurationDeathzone = 0.5f;

		private float blackDurationDeathzone = 1.75f;

		private float fadeOpenDurationDeathzone = 4.5f;

		private float fadeToBlackDurationDoor = 0.5f;

		private float blackDurationDoor = 0.8f;

		private float fadeOpenDurationDoor = 1.6f;

		private float fadeToBlackDuration;

		private float blackDuration;

		private float fadeOpenDuration;

		public PlayerAIWaypoint letAIGoToThisWaypointInstead;

		private int runHash = Animator.StringToHash("run");

		private void OnEnable()
		{
			teleportRoutine = null;
		}

		public void DoTeleport(Player player)
		{
			if (player.isAI)
			{
				player.CompletelyStop();
				PlacePlayer(player);
				if (letAIGoToThisWaypointInstead != null)
				{
					player.AI.SetNextWaypointClean(letAIGoToThisWaypointInstead);
				}
				else if (letAIGoToTheNextWaypoint)
				{
					player.AI.SkipToNextTarget();
				}
			}
			else if (teleportRoutine == null)
			{
				if (doDamage)
				{
					player.PlayVoice(AudioClipID.VoiceDieFall, VoicePriority.COMBAT);
				}
				if (automaticallyReturnPlayerToLastSafeLocation)
				{
					fadeToBlackDuration = fadeToBlackDurationDeathzone;
					blackDuration = blackDurationDeathzone;
					fadeOpenDuration = fadeOpenDurationDeathzone;
				}
				else
				{
					fadeToBlackDuration = fadeToBlackDurationDoor;
					blackDuration = blackDurationDoor;
					fadeOpenDuration = fadeOpenDurationDoor;
				}
				teleportRoutine = StartCoroutine(TeleportRoutine(player));
			}
		}

		internal static bool IsInSequenceOrTransition()
		{
			if (!SequenceHandler.instance.IsInSequence())
			{
				return SequenceHandler.instance.IsInSequenceTransition();
			}
			return true;
		}

		private IEnumerator TeleportRoutine(Player player)
		{
			bool inEncounter = WorldHandler.instance.currentEncounter != null;
			player.userInputEnabled = false;
			Tween t = Core.Instance.UIManager.effects.FadeToBlack(fadeToBlackDuration);
			yield return t.WaitForCompletion();
			player.CompletelyStop();
			Core.Instance.UIManager.effects.fullScreenFade.gameObject.SetActive(value: true);
			Core.Instance.UIManager.effects.fullScreenFade.color = EffectsUI.niceBlack;
			yield return new WaitForSeconds(blackDuration);
			bool flag = IsInSequenceOrTransition();
			if (automaticallyReturnPlayerToLastSafeLocation & inEncounter & flag)
			{
				teleportRoutine = null;
				yield break;
			}
			if (flag)
			{
				Core.Instance.UIManager.effects.FadeOpen(fadeOpenDuration);
				while (flag)
				{
					yield return null;
					flag = IsInSequenceOrTransition();
				}
				PlacePlayer(player);
				player.userInputEnabled = true;
				teleportRoutine = null;
				yield break;
			}
			PlacePlayer(player);
			player.userInputEnabled = true;
			teleportRoutine = null;
			yield return null;
			yield return null;
			if (!IsInSequenceOrTransition())
			{
				Core.Instance.UIManager.effects.FadeOpen(fadeOpenDuration);
			}
		}

		private void PlacePlayer(Player player)
		{
			if (automaticallyReturnPlayerToLastSafeLocation && player.GetSafeLocation().timeStamp != -1f)
			{
				SafeLocation safeLocation = player.GetSafeLocation();
				WorldHandler.instance.PlacePlayerAt(player, safeLocation.position, safeLocation.rotation);
			}
			else if (WorldHandler.instance.currentEncounter is CombatEncounter && WorldHandler.instance.currentEncounter.state == Encounter.EncounterState.MAIN_EVENT && WorldHandler.instance.currentEncounter.startSpawner != null)
			{
				WorldHandler.instance.PlacePlayerAt(player, WorldHandler.instance.currentEncounter.startSpawner);
			}
			else if (teleportTo == null)
			{
				WorldHandler.instance.PlacePlayerAt(player, base.transform.GetChild(0));
			}
			else if (teleportTo.GetComponent<Teleport>() == null)
			{
				WorldHandler.instance.PlacePlayerAt(player, teleportTo);
			}
			else
			{
				WorldHandler.instance.PlacePlayerAt(player, teleportTo.GetChild(0));
			}
			if (player.IsGrounded())
			{
				player.PlayAnim(runHash);
			}
			if (giveSpeedAtSpawn)
			{
				player.SetVelocityToMax();
			}
			if (doDamage)
			{
				player.GetHit(2, Vector3.zero, KnockbackAbility.KnockbackType.FALL);
				if (player.HP > 0f)
				{
					player.ActivateAbility(player.recoverAbility);
				}
			}
			player.RemoveAllCuffs();
		}
	}
}
