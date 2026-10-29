using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class DieAbility : Ability
	{
		private int dieHash = Animator.StringToHash("die");

		private UIManager uIManager;

		public DieAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = true;
			normalRotation = false;
			canStartGrind = false;
			canStartWallrun = false;
			uIManager = Core.Instance.UIManager;
			targetSpeed = 0f;
		}

		public override void OnStartAbility()
		{
			p.SwitchToEquippedMovestyle(set: false);
			p.StartScreenShake(ScreenShakeType.MEDIUM, 0.6f);
			p.PlayVoice(AudioClipID.VoiceDie, VoicePriority.DIE);
			p.PlayAnim(dieHash);
			p.RemoveAllCuffs();
			p.StartCoroutine(UpdateAbilityDuringPause());
		}

		private IEnumerator UpdateAbilityDuringPause()
		{
			float timer = 0f;
			if (!p.caughtAbility.caught)
			{
				while (timer <= 0.5f)
				{
					timer += Time.deltaTime;
					yield return null;
				}
			}
			WantedManager.instance.StopPlayerWantedStatus(showMessage: true);
			uIManager.PopAllMenusInstant();
			p.phone.TurnOff();
			p.ui.TurnOn(set: false);
			Core instance = Core.Instance;
			int cullingMask = 32768;
			p.cam.cam.clearFlags = CameraClearFlags.Color;
			p.cam.cam.backgroundColor = EffectsUI.niceBlack;
			p.cam.cam.cullingMask = cullingMask;
			instance.BaseModule.PauseGame(PauseType.GameOver);
			p.anim.speed = 1f;
			timer = 0f;
			while (timer <= 3f)
			{
				timer += Time.deltaTime;
				yield return null;
			}
			uIManager.ShowDieMenu();
			timer = 0f;
			while (timer <= 2f)
			{
				timer += Time.deltaTime;
				yield return null;
			}
			uIManager.ActivateDieMenu();
		}
	}
}
