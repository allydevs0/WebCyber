using UnityEngine;

namespace Reptile
{
	public class EnemyShield : MonoBehaviour
	{
		[SerializeField]
		private GameObject visualRoot;

		private bool broken;

		public bool Broken => broken;

		public void Init()
		{
			visualRoot.SetActive(value: true);
		}

		public void ResetShield()
		{
			visualRoot.SetActive(value: true);
			broken = false;
		}

		public void Break()
		{
			broken = true;
			visualRoot.SetActive(value: false);
		}
	}
}
