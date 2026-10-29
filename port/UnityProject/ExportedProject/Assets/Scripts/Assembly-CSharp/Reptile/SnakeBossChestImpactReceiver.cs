using UnityEngine;

namespace Reptile
{
	public class SnakeBossChestImpactReceiver : MonoBehaviour
	{
		private bool wasHit;

		public bool WasHit
		{
			get
			{
				return wasHit;
			}
			set
			{
				wasHit = value;
			}
		}
	}
}
