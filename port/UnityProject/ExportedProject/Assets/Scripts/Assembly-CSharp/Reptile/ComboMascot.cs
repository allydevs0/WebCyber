using UnityEngine;

namespace Reptile
{
	public class ComboMascot : MonoBehaviour
	{
		public enum State
		{
			NONE = 0,
			STANDBY = 1,
			READY = 2,
			HIT = 3,
			COMPLETE = 4
		}

		internal State state;

		internal ComboMascotSystem comboMascotSystem;

		internal int num;

		public Transform highfivePoint;

		private string curAnim;

		public Animation anim;

		public GameObject hitEffect;

		public AudioClipID hitSound;

		public AudioSource audioSource;

		public void OnTriggerEnter(Collider other)
		{
			if (state == State.READY && (other.gameObject.layer == 18 || other.gameObject.layer == 9))
			{
				Player componentInParent = other.gameObject.GetComponentInParent<Player>();
				if (!componentInParent.isAI && (componentInParent.IsComboing() || componentInParent.ability == componentInParent.airTrickAbility))
				{
					SetState(State.HIT);
				}
			}
		}

		public void SetState(State setState)
		{
			if (setState == state)
			{
				return;
			}
			state = setState;
			switch (state)
			{
			case State.STANDBY:
				PlayAnim("idle");
				break;
			case State.READY:
				PlayAnim("idle");
				break;
			case State.HIT:
				comboMascotSystem.player.HitHighfive();
				PlayAnim("spin");
				comboMascotSystem.player.StartHitpause(0.05f);
				comboMascotSystem.player.StartScreenShake(ScreenShakeType.LIGHT, 0.2f);
				if ((bool)hitEffect)
				{
					Object.Instantiate(hitEffect, highfivePoint.position, highfivePoint.rotation);
				}
				if (hitSound != AudioClipID.NONE)
				{
					Core.Instance.AudioManager.PlaySfxGameplayPitched(SfxCollectionID.EnvironmentSfx, hitSound, audioSource, 1f + 0.07f * (float)comboMascotSystem.mascotsHit);
				}
				comboMascotSystem.SignalMascotHit();
				break;
			case State.COMPLETE:
				break;
			}
		}

		public void PlayAnim(string animName)
		{
			curAnim = animName;
			anim.CrossFade(animName, 0.35f);
		}
	}
}
