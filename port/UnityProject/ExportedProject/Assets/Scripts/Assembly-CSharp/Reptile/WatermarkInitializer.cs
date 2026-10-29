using UnityEngine;

namespace Reptile
{
	public class WatermarkInitializer : MonoBehaviour
	{
		public float maxStayInPositionTime = 10f;

		private bool watermarkIsSet;

		private void Awake()
		{
			Object.Destroy(base.gameObject);
		}

		private void Update()
		{
			if (!watermarkIsSet)
			{
				SetUserWatermark();
			}
		}

		private void InitializeWatermark()
		{
		}

		private void SetUserWatermark()
		{
		}
	}
}
