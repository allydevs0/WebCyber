namespace Reptile
{
	public class Pedestrian : StreetLife
	{
		public override StreetLifeHot InitCustomValues(StreetLifeHot streetLifeHot)
		{
			streetLifeHot.verticalAcc = 60f;
			streetLifeHot.maxVerticalSpeed = 10f;
			return streetLifeHot;
		}
	}
}
