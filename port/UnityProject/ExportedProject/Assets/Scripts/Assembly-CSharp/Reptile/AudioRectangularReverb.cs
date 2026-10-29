using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(BoxCollider))]
	public class AudioRectangularReverb : MonoBehaviour
	{
		public AudioReverbZone audioReverbZone;

		private BoxCollider collider;

		private void Awake()
		{
			collider = GetComponent<BoxCollider>();
			SetZonePosition(base.transform.position);
		}

		public void SetZonePosition(Vector3 listenerPosition)
		{
			Vector3 lhs = listenerPosition - base.transform.position;
			float value = Vector3.Dot(lhs, base.transform.right);
			float value2 = Vector3.Dot(lhs, base.transform.up);
			float value3 = Vector3.Dot(lhs, base.transform.forward);
			float num = 0.5f * base.transform.localScale.x * collider.size.x - audioReverbZone.maxDistance;
			float num2 = 0.5f * base.transform.localScale.y * collider.size.y - audioReverbZone.maxDistance;
			float num3 = 0.5f * base.transform.localScale.z * collider.size.z - audioReverbZone.maxDistance;
			value = Mathf.Clamp(value, 0f - num, num);
			value2 = Mathf.Clamp(value2, 0f - num2, num2);
			value3 = Mathf.Clamp(value3, 0f - num3, num3);
			Vector3 vector = base.transform.right * value + base.transform.up * value2 + base.transform.forward * value3;
			audioReverbZone.transform.position = base.transform.position + vector;
		}
	}
}
