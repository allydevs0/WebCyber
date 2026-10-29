using UnityEngine;

namespace Reptile
{
	public class SitAbility : Ability
	{
		public enum State
		{
			SIT = 0,
			END_SIT = 1
		}

		public State state;

		private int stopSitHash = Animator.StringToHash("stopSit");

		private int startSitHash = Animator.StringToHash("startSit");

		public SitAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = true;
			normalRotation = false;
			canStartGrind = false;
			canStartWallrun = false;
			targetSpeed = 0f;
		}

		public override void OnStartAbility()
		{
			p.SetVelocity(Vector3.zero);
			p.ResetVisualRot();
			SetState(State.SIT);
			p.ui.TurnOn(set: false);
		}

		public void SkipStartSit()
		{
			p.PlayAnim(Animator.StringToHash("sit"));
		}

		public void SetState(State setState)
		{
			state = setState;
			switch (state)
			{
			case State.SIT:
				p.PlayAnim(startSitHash);
				break;
			case State.END_SIT:
				p.PlayAnim(stopSitHash);
				break;
			}
		}

		public override void FixedUpdateAbility()
		{
			switch (state)
			{
			case State.SIT:
				if (p.abilityTimer > 1f && (p.moveTap != Vector3.zero || p.jumpButtonNew))
				{
					SetState(State.END_SIT);
				}
				break;
			case State.END_SIT:
				if (p.curAnim != stopSitHash)
				{
					p.StopCurrentAbility();
				}
				break;
			}
		}

		public override void OnStopAbility()
		{
			p.ui.TurnOn(set: true);
			base.OnStopAbility();
		}

		public override bool CheckActivation()
		{
			return false;
		}
	}
}
