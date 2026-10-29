using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(LensFlare))]
	[RequireComponent(typeof(Light))]
	public class SunGlare : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float minLightIntensityFactor = 0.25f;

		[Range(0f, 100f)]
		public float maxFlareBrightness = 50f;

		[Range(0f, 0.2f)]
		public float smoothValue = 0.05f;

		[Range(10f, 55f)]
		public float minLookAtAngle = 45f;

		private Light lightComponent;

		private float targetFactor = 1f;

		private float currentFactor = 1f;

		private float currentLookAtAngle;

		private float maxLightIntensityFactor = 1f;

		private bool isLensFlareVisible;

		public void Awake()
		{
			lightComponent = base.gameObject.GetComponent<Light>();
			maxLightIntensityFactor = lightComponent.intensity;
			StageManager.OnStageInitialized += OnStageInitialized;
		}

		public void OnDestroy()
		{
			Core.OnUpdate -= OnCoreUpdate;
			Core.OnFixedUpdate -= OnCoreFixedUpdate;
			StageManager.OnStageInitialized -= OnStageInitialized;
		}

		private void OnStageInitialized()
		{
			Core.OnUpdate += OnCoreUpdate;
			Core.OnFixedUpdate += OnCoreFixedUpdate;
		}

		private void OnCoreUpdate()
		{
			DetermineFactorByLookAtAngle();
			currentFactor = SmoothFactor(currentFactor, targetFactor, smoothValue);
			lightComponent.intensity = Mathf.Lerp(minLightIntensityFactor, maxLightIntensityFactor, currentFactor);
		}

		private void OnCoreFixedUpdate()
		{
			DetermineLensFlareVisibility();
		}

		private void DetermineFactorByLookAtAngle()
		{
			currentLookAtAngle = Vector3.Angle(base.transform.back(), WorldHandler.instance.CurrentCameraTransform.forward);
			if (currentLookAtAngle <= minLookAtAngle)
			{
				if (isLensFlareVisible)
				{
					if (minLookAtAngle > 0f)
					{
						targetFactor = currentLookAtAngle / minLookAtAngle;
					}
					else
					{
						targetFactor = 0f;
					}
				}
				else
				{
					targetFactor = 1f;
				}
			}
			else
			{
				targetFactor = 1f;
			}
		}

		private float SmoothFactor(float factor, float targetFactor, float smoothValue)
		{
			if (factor < targetFactor)
			{
				factor += smoothValue;
				if (factor >= targetFactor)
				{
					factor = targetFactor;
				}
			}
			else
			{
				factor -= smoothValue;
				if (factor <= targetFactor)
				{
					factor = targetFactor;
				}
			}
			return factor;
		}

		private void DetermineLensFlareVisibility()
		{
			if (currentLookAtAngle <= minLookAtAngle)
			{
				isLensFlareVisible = !Physics.Raycast(WorldHandler.instance.CurrentCameraPosition, base.transform.back(), float.PositiveInfinity, 1);
			}
			else
			{
				isLensFlareVisible = true;
			}
		}
	}
}
