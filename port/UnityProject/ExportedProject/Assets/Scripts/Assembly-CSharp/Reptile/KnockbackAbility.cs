using UnityEngine;

namespace Reptile
{
	public class KnockbackAbility : Ability
	{
		public enum KnockbackType
		{
			STATIONARY = 0,
			FAR = 1,
			BIG = 2,
			SHIELDBASH = 3,
			FALL = 4,
			NO_KNOCKBACK = 5
		}

		private float startHeight;

		private Vector3 knockBackDir;

		private int knockBackFarHash = Animator.StringToHash("getHitFar");

		private int knockBackBigHash = Animator.StringToHash("getHitBig");

		private int getHitHash = Animator.StringToHash("getHit");

		private int knockBackShieldBashHash = Animator.StringToHash("getHitShieldBash");

		public KnockbackType knockbackType;

		private float duration;

		public KnockbackAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			acc = 0f;
			rotateSpeed = 0f;
			targetSpeed = 0f;
			canStartGrind = false;
			canStartWallrun = false;
		}

		public override void OnStartAbility()
		{
			p.SwitchToEquippedMovestyle(set: false, doAirTrick: false, changeProp: true, showEffect: false);
			Vector3 velocity = knockBackDir;
			startHeight = p.transform.position.y;
			if (knockbackType == KnockbackType.FAR)
			{
				p.ForceUnground();
				p.SetRotHard(knockBackDir);
				p.SetVisualRot(Quaternion.LookRotation(-knockBackDir, Vector3.up));
				p.PlayAnim(knockBackFarHash, forceOverwrite: true, instant: true, 0f);
				p.StartScreenShake(ScreenShakeType.MEDIUM, 0.2f);
				velocity *= 14f;
				velocity.y = 2.5f;
			}
			else if (knockbackType == KnockbackType.BIG)
			{
				p.ForceUnground();
				p.SetRotHard(knockBackDir);
				p.SetVisualRot(Quaternion.LookRotation(-knockBackDir, Vector3.up));
				p.PlayAnim(knockBackBigHash, forceOverwrite: true, instant: true, 0f);
				p.StartScreenShake(ScreenShakeType.HEAVY, 0.2f);
				velocity *= 8f;
				velocity.y = 9.5f;
			}
			else if (knockbackType == KnockbackType.STATIONARY)
			{
				p.PlayAnim(getHitHash);
				p.StartScreenShake(ScreenShakeType.LIGHT, 0.15f);
			}
			else if (knockbackType == KnockbackType.SHIELDBASH)
			{
				p.ForceUnground();
				p.SetRotHard(knockBackDir);
				p.SetVisualRot(Quaternion.LookRotation(-knockBackDir, Vector3.up));
				p.PlayAnim(knockBackShieldBashHash, forceOverwrite: true, instant: true, 0f);
				p.StartScreenShake(ScreenShakeType.HEAVY, 0.2f);
				velocity *= 4f;
				velocity.y = 2.5f;
			}
			p.SetVelocity(velocity);
		}

		public override void FixedUpdateAbility()
		{
			if (p.IsGrounded())
			{
				decc = 15f;
			}
			else
			{
				decc = p.stats.airDecc;
			}
			if (!p.IsGrounded() && p.GetVelocity().y < 0f && p.transform.position.y < startHeight - 0.3f)
			{
				p.StopCurrentAbility();
				p.PlayAnim(Animator.StringToHash("fall"));
			}
			if (p.abilityTimer > duration || (p.curAnim != knockBackFarHash && p.curAnim != knockBackBigHash && p.curAnim != knockBackShieldBashHash && p.curAnim != getHitHash && p.IsGrounded()))
			{
				p.SetSpeedFlat(0f);
				p.StopCurrentAbility();
			}
		}

		public void SetKnockback(Vector3 fromDir, KnockbackType type)
		{
			knockbackType = type;
			knockBackDir = -fromDir;
			knockBackDir.y = 0f;
			if (knockbackType == KnockbackType.FAR)
			{
				duration = 2f;
			}
			else if (knockbackType == KnockbackType.BIG)
			{
				duration = 3f;
			}
			else if (knockbackType == KnockbackType.STATIONARY)
			{
				duration = 1f;
			}
			else if (knockbackType == KnockbackType.SHIELDBASH)
			{
				duration = 1.75f;
			}
		}

		public override void OnStopAbility()
		{
			if (knockbackType != KnockbackType.STATIONARY)
			{
				p.SetRotHard(-p.dir);
				p.ResetVisualRot();
			}
			base.OnStopAbility();
		}
	}
}
