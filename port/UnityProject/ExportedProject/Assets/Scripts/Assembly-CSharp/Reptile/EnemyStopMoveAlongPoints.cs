using UnityEngine;

namespace Reptile
{
	public class EnemyStopMoveAlongPoints : MonoBehaviour
	{
		public void OnTriggerStay(Collider other)
		{
			if (other.gameObject.CompareTag("MovingObject"))
			{
				other.gameObject.GetComponentInParent<MoveAlongPoints>().TriggerDetectLayer(17);
			}
		}
	}
}
