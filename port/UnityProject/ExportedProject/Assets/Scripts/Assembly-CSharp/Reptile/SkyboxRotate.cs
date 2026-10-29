using UnityEngine;

namespace Reptile
{
	public class SkyboxRotate : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
			RenderSettings.skybox.SetFloat("_Rotation", Time.time);
		}
	}
}
