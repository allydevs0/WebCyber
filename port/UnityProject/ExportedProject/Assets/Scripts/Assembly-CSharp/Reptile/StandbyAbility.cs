namespace Reptile
{
	public class StandbyAbility : Ability
	{
		public StandbyAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = false;
			normalRotation = false;
		}

		public override void OnStartAbility()
		{
			p.gameObject.SetActive(value: false);
		}

		public override void OnStopAbility()
		{
			p.gameObject.SetActive(value: true);
		}
	}
}
