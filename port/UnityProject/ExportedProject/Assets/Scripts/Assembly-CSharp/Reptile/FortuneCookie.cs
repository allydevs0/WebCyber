using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(menuName = "Reptile/Phone/Fortune/New Fortune Cookie")]
	public class FortuneCookie : AUnlockable
	{
		public string advice;

		[Header("Boost value in percentage of max boost")]
		[Tooltip("Amount of boost that a fortune will give to the player.\nIt is in percentage, so 0.5f will fill the player's boost bar half way (50%).")]
		public float boostPercentage;
	}
}
