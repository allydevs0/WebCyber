using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class BreakableObject : MonoBehaviour
	{
		public enum BreakOn
		{
			HITBOX = 0,
			BOOST = 1,
			RUSH_MODE = 2,
			MOVESTYLE_IN_AIR = 3,
			INLINE_SLIDE = 4
		}

		private int HP = 100;

		public BreakOn breakOn;

		private Player player;

		private Collider curCollider;

		private Collision curCollision;

		public GameObject breakEffect;

		public AudioClipID breakSounds;

		public AudioSource audioSource;

		private Coroutine waitForBreakingSoundRoutine;

		private Renderer[] breakableRenderers;

		private Collider[] breakableColliders;

		public void OnTriggerStay(Collider other)
		{
			if (other.gameObject.layer == 18)
			{
				CheckSetPlayerFromHitbox(other);
				if (breakOn == BreakOn.HITBOX || breakOn == BreakOn.MOVESTYLE_IN_AIR || (breakOn == BreakOn.BOOST && player.boosting))
				{
					Break(affectPlayer: true);
				}
				else
				{
					player.hitBounceAbility.FailBounce(base.gameObject, base.transform.right);
				}
			}
			else if (other.gameObject.layer == 20 && breakOn == BreakOn.HITBOX)
			{
				Break();
			}
		}

		private void OnCollisionStay(Collision other)
		{
			if (other.gameObject.layer != 15)
			{
				return;
			}
			if (breakOn == BreakOn.MOVESTYLE_IN_AIR)
			{
				CheckSetPlayerCollision(other);
				if (player.moveStyle != MoveStyle.ON_FOOT && !player.IsGrounded())
				{
					Break(affectPlayer: true);
				}
			}
			else
			{
				if (breakOn != BreakOn.INLINE_SLIDE)
				{
					return;
				}
				CheckSetPlayerCollision(other);
				if (player.moveStyle == MoveStyle.INLINE && player.ability == player.slideAbility)
				{
					HP -= 12;
					if (HP <= 0)
					{
						Break(affectPlayer: true);
					}
				}
			}
		}

		private void OnDestroy()
		{
			if (waitForBreakingSoundRoutine != null)
			{
				StopCoroutine(waitForBreakingSoundRoutine);
			}
		}

		private void Awake()
		{
			breakableRenderers = GetComponentsInChildren<Renderer>();
			breakableColliders = GetComponentsInChildren<Collider>();
		}

		public void Break(bool affectPlayer = false)
		{
			if (affectPlayer)
			{
				player.GiveBackSpeedAfterHittingBreakable();
				player.StartHitpause(0.05f);
				player.StartScreenShake(ScreenShakeType.LIGHT, 0.2f);
			}
			ToggleRenderers(toggle: false);
			ToggleColliders(toggle: false);
			if ((bool)breakEffect)
			{
				Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
			}
			if (audioSource == null)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.EnvironmentSfx, breakSounds, audioSource);
			waitForBreakingSoundRoutine = StartCoroutine(WaitForBreakSoundToFinish());
		}

		private void ToggleRenderers(bool toggle)
		{
			for (int i = 0; i < breakableRenderers.Length; i++)
			{
				breakableRenderers[i].enabled = toggle;
			}
		}

		private void ToggleColliders(bool toggle)
		{
			for (int i = 0; i < breakableColliders.Length; i++)
			{
				breakableColliders[i].enabled = toggle;
			}
		}

		private IEnumerator WaitForBreakSoundToFinish()
		{
			while (audioSource.isPlaying)
			{
				yield return null;
			}
			ToggleRenderers(toggle: true);
			ToggleColliders(toggle: true);
			base.gameObject.SetActive(value: false);
		}

		private void CheckSetPlayerFromHitbox(Collider newCollider)
		{
			if (curCollider != newCollider)
			{
				curCollider = newCollider;
				player = newCollider.transform.GetComponentInParent<Player>();
			}
		}

		private void CheckSetPlayerCollision(Collision newCollision)
		{
			if (curCollision != newCollision)
			{
				curCollision = newCollision;
				player = newCollision.gameObject.GetComponent<Player>();
			}
		}
	}
}
