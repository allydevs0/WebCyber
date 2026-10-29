using UnityEngine;

namespace Reptile
{
	public class BirdBehaviour : StreetLifeBehaviour
	{
		private float[] stopVerticalSpeedTimers;

		public BirdBehaviour(int length)
		{
			stopVerticalSpeedTimers = new float[length];
		}

		~BirdBehaviour()
		{
			stopVerticalSpeedTimers = null;
		}

		public override void UpdateStreetLife(StreetLifeCluster cluster, StreetLifeHot[] streetLifeHotArray, int startIndex, int length)
		{
			float dt = Core.dt;
			for (int i = startIndex; i < startIndex + length; i++)
			{
				float num = stopVerticalSpeedTimers[i - startIndex];
				if (streetLifeHotArray[i].isShook)
				{
					VerticalSpeed(ref streetLifeHotArray[i], dt);
					num += dt;
					if (num > 3.8f)
					{
						streetLifeHotArray[i].verticalAcc = 0f;
						streetLifeHotArray[i].verticalSpeed = Mathf.Max(streetLifeHotArray[i].verticalSpeed - 2f * dt, 0f);
					}
					streetLifeHotArray[i].transform.position += streetLifeHotArray[i].moveDir * streetLifeHotArray[i].moveAwaySpeed * dt;
				}
				if (streetLifeHotArray[i].wasShook && IsNotInView(cluster, ref streetLifeHotArray[i]))
				{
					num = 0f;
					streetLifeHotArray[i].verticalAcc = 30f;
					ResetBack(cluster, ref streetLifeHotArray[i]);
				}
				stopVerticalSpeedTimers[i - startIndex] = num;
			}
		}

		public override void Move(StreetLifeCluster cluster, StreetLife life, ref StreetLifeHot streetLifeHot, Transform otherTransform)
		{
			streetLifeHot.verticalSpeed = streetLifeHot.maxVerticalSpeed * 0.6f;
			base.Move(cluster, life, ref streetLifeHot, otherTransform);
		}
	}
}
