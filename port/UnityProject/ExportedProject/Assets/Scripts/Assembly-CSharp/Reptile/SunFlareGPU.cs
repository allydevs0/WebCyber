using System.Reflection;
using UnityEngine;

namespace Reptile
{
	public class SunFlareGPU : MonoBehaviour
	{
		[SerializeField]
		private Camera sunOcclusionCamera;

		[SerializeField]
		private Transform sunFlareTransform;

		[SerializeField]
		private Shader sunFlareReplacementShader;

		[SerializeField]
		private RenderTexture sunRenderTexture;

		private GameplayCamera gameplayCamera;

		private FieldInfo canvasWillRenderFieldInfo;

		private Camera currentCamera;

		public void Awake()
		{
			if (sunOcclusionCamera.enabled)
			{
				sunOcclusionCamera.enabled = false;
			}
			Shader.SetGlobalTexture("_RenderTexture", sunRenderTexture);
			canvasWillRenderFieldInfo = typeof(Canvas).GetField("willRenderCanvases", BindingFlags.Static | BindingFlags.NonPublic);
			StageManager.OnStageInitialized += StageManager_OnStageInitialized;
		}

		private void StageManager_OnStageInitialized()
		{
			gameplayCamera = Object.FindObjectOfType<GameplayCamera>();
			SetGameplayCamera(gameplayCamera);
		}

		public void OnDestroy()
		{
			if (gameplayCamera != null)
			{
				gameplayCamera.SetSunCamTransform(null);
			}
			StageManager.OnStageInitialized -= StageManager_OnStageInitialized;
			Core.OnLateUpdate -= Core_OnLateUpdate;
		}

		private void Core_OnLateUpdate()
		{
			Vector3 position = sunOcclusionCamera.transform.position;
			Vector3 vector = base.transform.back();
			sunOcclusionCamera.transform.LookAt(position + vector);
			sunFlareTransform.position = position + vector * 1000f;
			sunFlareTransform.LookAt(position);
			Vector3 vector2 = currentCamera.WorldToViewportPoint(sunFlareTransform.position);
			if (vector2.x >= 0f && vector2.x <= 1f && vector2.y >= 0f && vector2.y <= 1f && vector2.z >= 0f)
			{
				CameraUtility.RenderCameraWithShaderWithoutUIEvents(sunOcclusionCamera, sunFlareReplacementShader, canvasWillRenderFieldInfo);
			}
		}

		private void SetGameplayCamera(GameplayCamera gameplayCamera)
		{
			if (gameplayCamera == null)
			{
				HandleNoGameplayCameraFound();
				return;
			}
			gameplayCamera.SetSunCamTransform(sunOcclusionCamera.transform);
			currentCamera = gameplayCamera.cam;
			Core.OnLateUpdate += Core_OnLateUpdate;
		}

		private void HandleNoGameplayCameraFound()
		{
			if (!(Core.Instance == null))
			{
				_ = Core.Instance.BaseModule == null;
			}
		}
	}
}
