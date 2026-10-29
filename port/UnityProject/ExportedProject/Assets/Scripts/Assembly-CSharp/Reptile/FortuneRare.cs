using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(menuName = "Reptile/Phone/Fortune/New Fortune Rare")]
	public class FortuneRare : FortuneCookie
	{
		private FortuneRare()
		{
			boostPercentage = 1f;
		}
	}
}
