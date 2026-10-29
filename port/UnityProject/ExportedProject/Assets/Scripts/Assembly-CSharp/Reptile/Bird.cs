using UnityEngine;

namespace Reptile
{
	public class Bird : StreetLife
	{
		public override StreetLifeHot InitCustomValues(StreetLifeHot streetLifeHot)
		{
			streetLifeHot.moveAwaySpeed = 7f;
			streetLifeHot.verticalAcc = 30f;
			streetLifeHot.maxVerticalSpeed = 7.5f;
			streetLifeHot.verticalDir = Vector3.up;
			return streetLifeHot;
		}
	}
}
