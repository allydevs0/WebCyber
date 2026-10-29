using UnityEngine;

namespace Reptile
{
	public class DJCollider : MonoBehaviour
	{
		private bool receivedHit;

		private bool activated;

		public void Activate()
		{
			activated = true;
			receivedHit = false;
		}

		public void Deactivate()
		{
			activated = false;
			receivedHit = false;
		}

		public bool ReceivedHit()
		{
			bool result = receivedHit;
			receivedHit = false;
			return result;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.layer == 18)
			{
				receivedHit = true;
			}
		}
	}
}
