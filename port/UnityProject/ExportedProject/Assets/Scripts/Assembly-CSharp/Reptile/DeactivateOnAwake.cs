using UnityEngine;

namespace Reptile
{
	public class DeactivateOnAwake : MonoBehaviour
	{
		private void Awake()
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
