using UnityEngine;

namespace Reptile
{
	public class AmbientTrigger : MonoBehaviour
	{
		internal AmbientManager ambientManager;

		public Color AmbientEnvLight;

		public Color AmbientEnvShadow;

		public float transitionDuration = 2f;

		public float revertTransitionDuration = 2f;

		private void Start()
		{
			ambientManager = RenderSettings.sun.GetComponent<AmbientManager>();
		}

		private void OnTriggerEnter(Collider trigger)
		{
			ambientManager.InAmbientTrigger(this);
		}

		private void OnTriggerExit(Collider trigger)
		{
			ambientManager.OutAmbientTrigger(this);
		}
	}
}
