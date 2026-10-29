using UnityEngine;

namespace Reptile
{
	public class RestrictedArea : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			WantedManager.instance.CrimePerpetrated(WantedManager.Crime.RESTRICTED_AREA);
		}
	}
}
