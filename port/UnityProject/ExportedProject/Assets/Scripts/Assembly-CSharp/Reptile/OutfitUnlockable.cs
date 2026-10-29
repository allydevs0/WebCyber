using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(menuName = "Reptile/VFX/Player outfit unlockable")]
	public class OutfitUnlockable : AUnlockable
	{
		public string outfitName;

		public Characters character;

		public int outfitIndex;
	}
}
