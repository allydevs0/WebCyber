using UnityEngine;

namespace Reptile
{
	public class CaughtAbility : Ability
	{
		public bool caught;

		private float duration = 6.3f;

		private int inputs;

		public Transform target;

		public TurretEnemy turret;

		private int turretCaughtHash = Animator.StringToHash("turretCaught");

		private int turretCaughtNoChainsHash = Animator.StringToHash("turretCaughtNoChains");

		private int turretStruggleAHash = Animator.StringToHash("turretStruggleA");

		private int turretStruggleNoChainsAHash = Animator.StringToHash("turretStruggleNoChainsA");

		private int turretStruggleBHash = Animator.StringToHash("turretStruggleB");

		private int turretStruggleNoChainsBHash = Animator.StringToHash("turretStruggleNoChainsB");

		private Vector3 startPos = Vector3.zero;

		private bool currentStruggleAnim;

		private float lastStrugTime;

		private bool struggling;

		public CaughtAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = false;
			normalRotation = false;
			canStartGrind = false;
			canStartWallrun = false;
		}

		public override void OnStartAbility()
		{
			p.SwitchToEquippedMovestyle(set: false);
			p.StartScreenShake(ScreenShakeType.MEDIUM, 0.6f);
			startPos = p.tf.position;
			p.SetPosAndRotHard(target.position, target.rotation);
			if (p.attachedCuffs[0] != null && p.attachedCuffs[1] != null)
			{
				p.PlayAnim(turretCaughtHash);
			}
			else
			{
				p.PlayAnim(turretCaughtNoChainsHash);
			}
			p.breakChainContextAvailable = 10;
			targetSpeed = 0f;
			inputs = 0;
			caught = false;
			WorldHandler.instance.AddCombatBlocker(6.3f);
		}

		public override void UpdateAbility()
		{
			p.breakChainContextAvailable = 10;
			p.SetPosAndRotHard(target.position - Vector3.up * 0.9f, target.rotation);
			p.OrientVisualInstantReset();
			p.characterVisual.feetIK = false;
			if (p.abilityTimer > duration)
			{
				caught = true;
				p.ChangeHP(6);
			}
			if (p.AnyTrickInput() && p.abilityTimer < 5.5f)
			{
				struggling = true;
				if (Time.time > lastStrugTime + 0.2f)
				{
					if (p.attachedCuffs[0] != null && p.attachedCuffs[1] != null)
					{
						p.PlayAnim(currentStruggleAnim ? turretStruggleAHash : turretStruggleBHash);
					}
					else
					{
						p.PlayAnim(currentStruggleAnim ? turretStruggleNoChainsAHash : turretStruggleNoChainsBHash);
					}
					currentStruggleAnim = !currentStruggleAnim;
					lastStrugTime = Time.time;
					p.RemoveCuff();
					inputs++;
				}
			}
			else if (Time.time > lastStrugTime + 0.5f && struggling)
			{
				struggling = false;
				if (p.attachedCuffs[0] != null && p.attachedCuffs[1] != null)
				{
					p.PlayAnim(turretCaughtHash, forceOverwrite: false, instant: false, p.abilityTimer / 6.2f);
				}
				else
				{
					p.PlayAnim(turretCaughtNoChainsHash, forceOverwrite: false, instant: false, p.abilityTimer / 6.2f);
				}
			}
			if (p.abilityTimer > 5.5f)
			{
				p.StartScreenShake(ScreenShakeType.MEDIUM, 0.6f);
				p.breakChainContextAvailable = 0;
			}
			if (inputs > 5)
			{
				BreakFree();
			}
		}

		public void BreakFree()
		{
			p.StopCurrentAbility();
			p.hitBounceAbility.FreeformBounce(HitBounceAbility.BounceType.TURRET_CLAW_BOUNCE);
			p.breakChainContextAvailable = 0;
			turret.PlayerEscaped();
			p.RemoveAllCuffs();
		}

		public override void OnStopAbility()
		{
			base.OnStopAbility();
			p.breakChainContextAvailable = 0;
		}
	}
}
