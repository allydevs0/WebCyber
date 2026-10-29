using UnityEngine;

namespace Reptile
{
	public class FlipOutJumpAbility : Ability
	{
		public float duration = 0.5f;

		private int poleFlipHash = Animator.StringToHash("poleFlip");

		private string skateboardTrickName = "Body Varial 540";

		private string bmxTrickName = "Flair-Whip";

		private string inlineTrickName = " Varial 540";

		public FlipOutJumpAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalRotation = false;
			decc = p.stats.airDecc;
			acc = 0f;
			targetSpeed = 0f;
			allowNormalJump = true;
			keepJumping = true;
		}

		public override void OnStartAbility()
		{
			p.SetDustEmission(0);
			p.PlayAnim(poleFlipHash);
			string text = "";
			text = ((p.moveStyle == MoveStyle.SKATEBOARD) ? skateboardTrickName : ((p.moveStyle != MoveStyle.BMX) ? inlineTrickName : bmxTrickName));
			p.DoTrick(Player.TrickType.POLE_FLIP, text);
		}

		public override void FixedUpdateAbility()
		{
			p.targetMovement = Player.MovementType.NONE;
			if (!p.slideAbility.CheckActivation() && (!(p.abilityTimer > 0.25f) || !p.airDashAbility.CheckActivation()) && !p.ledgeClimbAbility.CheckActivation())
			{
				if (p.abilityTimer < 0.4f)
				{
					p.hitbox.SetActive(value: true);
				}
				else
				{
					p.hitbox.SetActive(value: false);
				}
				if (p.IsGrounded() || p.abilityTimer > duration)
				{
					p.StopCurrentAbility();
					return;
				}
				_ = p.abilityTimer;
				_ = duration - 0.3f;
			}
		}

		public override void OnStopAbility()
		{
			p.hitbox.SetActive(value: false);
		}
	}
}
