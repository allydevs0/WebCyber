using UnityEngine;

namespace Reptile
{
	public class RecoverAbility : Ability
	{
		private int getUpHash = Animator.StringToHash("getUp");

		public RecoverAbility(Player player)
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
			p.SwitchToEquippedMovestyle(set: false);
			p.PlayAnim(getUpHash);
			WorldHandler.instance.AddCombatBlocker(7f);
		}

		public override void UpdateAbility()
		{
			if (p.abilityTimer > 3.5f)
			{
				p.StopCurrentAbility();
			}
		}
	}
}
