using UnityEngine;

namespace Reptile
{
	public class SwirlSkyUpdater : MonoBehaviour
	{
		[SerializeField]
		internal float baseAlphaOfSwirl = 0.192f;

		internal Material swirlSkyMaterial;

		internal int movementAlphaHash = Shader.PropertyToID("_MovementAlpha");

		internal float defaultDeltaTimeInverted;

		internal const int frameTimeEntriesCount = 16;

		internal const float frameTimeEntriesCountInverted = 0.0625f;

		internal float[] frameTimeEntries;

		internal int firstFrameTimeIndex;

		internal float totalFrameTimes;

		private void Awake()
		{
			defaultDeltaTimeInverted = 59.999996f;
			Renderer component = GetComponent<Renderer>();
			swirlSkyMaterial = component.sharedMaterial;
			firstFrameTimeIndex = 0;
			totalFrameTimes = 4f / 15f;
			frameTimeEntries = new float[16];
			for (int i = 0; i < 16; i++)
			{
				frameTimeEntries[i] = 1f / 60f;
			}
		}

		private void LateUpdate()
		{
			if (swirlSkyMaterial != null)
			{
				totalFrameTimes += Time.deltaTime - frameTimeEntries[firstFrameTimeIndex];
				frameTimeEntries[firstFrameTimeIndex] = Time.deltaTime;
				firstFrameTimeIndex = ++firstFrameTimeIndex % 16;
				float num = totalFrameTimes * 0.0625f;
				float num2 = 0f;
				float num3 = num * defaultDeltaTimeInverted;
				num2 = Mathf.Pow(baseAlphaOfSwirl, 1f / num3) - baseAlphaOfSwirl;
				swirlSkyMaterial.SetFloat(movementAlphaHash, num2);
			}
		}
	}
}
