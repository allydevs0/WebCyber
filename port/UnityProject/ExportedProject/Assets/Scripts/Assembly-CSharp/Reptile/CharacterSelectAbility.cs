namespace Reptile
{
	public class CharacterSelectAbility : Ability
	{
		internal bool spaceEnough;

		public CharacterSelectAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = true;
			normalRotation = false;
			targetSpeed = 0f;
			canStartGrind = false;
			canStartWallrun = false;
		}

		public override void OnStartAbility()
		{
			p.PlayAnim(p.characterVisual.bounceAnimHash);
			p.cam.gameObject.SetActive(value: false);
			p.phone.AllowPhone(isAllowed: false);
			WorldHandler.instance.CreateCharacterSelect(p, p.inCharacterSelectSpot);
			base.OnStartAbility();
		}

		public bool CanStartHere()
		{
			if (WorldHandler.instance.currentEncounter == null)
			{
				return p.inCharacterSelectSpot;
			}
			return false;
		}

		public bool CanStartAtWantedLevel()
		{
			return !WantedManager.instance.Wanted;
		}

		public override void OnStopAbility()
		{
			p.cam.gameObject.SetActive(value: true);
			p.FlushInput();
			p.phone.AllowPhone(isAllowed: true, completelyTurnOff: false, turnOnWhenAllowed: true);
			base.OnStopAbility();
		}
	}
}
