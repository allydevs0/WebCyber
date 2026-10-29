using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class CameraRegisterer : MonoBehaviour
	{
		private const float ASPECT_RATIO_16_9 = 1.78f;

		private const float SCENE_VIEW_PERCENTAGE = 0.78f;

		private const float MIN_FOV_WIDTH_FACTOR = 0f;

		private const float MAX_FOV_WIDTH_FACTOR = 0.33f;

		private const float LERP_RANGE_WIDTH_MULTIPLIER = 0.2f;

		private const float MIN_FOV_HEIGHT_FACTOR = -0.22f;

		private const float MAX_FOV_HEIGHT_FACTOR = 0.25f;

		private const float LERP_RANGE_HEIGHT_MULTIPLIER = 1.44f;

		[SerializeField]
		private bool useCustomCullingMask;

		[SerializeField]
		private LayerMask customCullingMask;

		private Camera storyCamera;

		private float calibratedCameraFov;

		private float currentCameraFov;

		private float currentAspectRatio;

		private void OnEnable()
		{
			storyCamera = GetComponent<Camera>();
			WorldHandler instance = WorldHandler.instance;
			instance.SetCurrentCamera(storyCamera);
			Vector3 position = base.transform.position;
			List<StageChunk> stageChunks = instance.SceneObjectsRegister.stageChunks;
			for (int i = 0; i < stageChunks.Count; i++)
			{
				StageChunk.ActivateIfPointIsInChunk(stageChunks[i], position);
			}
			storyCamera.cullingMask = (useCustomCullingMask ? customCullingMask.value : WorldHandler.GetGameplayCameraCullingMask());
			storyCamera.depth = -1f;
			storyCamera.nearClipPlane = 0.1f;
			EnableSensorSizeChangeDetection();
		}

		private void OnDisable()
		{
			WorldHandler instance = WorldHandler.instance;
			if ((bool)instance && instance.CurrentCamera == storyCamera)
			{
				instance.SetCurrentCamera(null);
			}
			Core.OnScreenSizeChanged -= OnScreenSizeChanged;
			Core.OnLateUpdate -= OnLateUpdate;
		}

		private void OnDestroy()
		{
			Core.OnScreenSizeChanged -= OnScreenSizeChanged;
			Core.OnLateUpdate -= OnLateUpdate;
		}

		private void EnableSensorSizeChangeDetection()
		{
			calibratedCameraFov = storyCamera.fieldOfView;
			UpdateCameraProperties();
			Core.OnScreenSizeChanged += OnScreenSizeChanged;
			Core.OnLateUpdate += OnLateUpdate;
		}

		private void UpdateCameraProperties()
		{
			if (storyCamera.usePhysicalProperties)
			{
				SetCameraSensorSize(storyCamera.aspect, storyCamera.fieldOfView, storyCamera.focalLength);
			}
			else
			{
				UpdateCameraFov();
			}
			currentCameraFov = storyCamera.fieldOfView;
			currentAspectRatio = storyCamera.aspect;
		}

		private void UpdateCameraFov()
		{
			if (currentCameraFov != storyCamera.fieldOfView)
			{
				calibratedCameraFov = storyCamera.fieldOfView;
			}
			if (currentAspectRatio != storyCamera.aspect)
			{
				SetCameraFieldOfView(storyCamera.aspect, calibratedCameraFov);
			}
		}

		private void SetCameraFieldOfView(float cameraAspectRatio, float fov)
		{
			if (MathF.Round(cameraAspectRatio, 2) < 1.78f)
			{
				storyCamera.fieldOfView = CalculateHeightFov(cameraAspectRatio, fov);
			}
			else
			{
				storyCamera.fieldOfView = calibratedCameraFov;
			}
		}

		private float CalculateHeightFov(float cameraAspectRatio, float fov)
		{
			float num = 1.78f / cameraAspectRatio;
			return fov * num;
		}

		private void SetCameraSensorSize(float cameraAspectRatio, float verticalCameraFov, float focalLength)
		{
			float x = DetermineCameraHorizontalSensorSize(cameraAspectRatio, verticalCameraFov, focalLength);
			storyCamera.sensorSize = new Vector2(x, storyCamera.sensorSize.y);
		}

		private float DetermineCameraHorizontalSensorSize(float aspectRatio, float verticalFov, float focalLength)
		{
			float num2;
			if (MathF.Round(aspectRatio, 2) >= 1.78f)
			{
				float num = CalculateWidthFovFactor(aspectRatio);
				num2 = Camera.VerticalToHorizontalFieldOfView(verticalFov * num, aspectRatio);
			}
			else
			{
				float num3 = CalculateHeightFovFactor(aspectRatio);
				num2 = Camera.VerticalToHorizontalFieldOfView(verticalFov * num3, aspectRatio);
			}
			return Mathf.Tan(num2 * 0.5f * (MathF.PI / 180f)) * focalLength * 2f;
		}

		private float CalculateWidthFovFactor(float aspectRatio)
		{
			float t = (aspectRatio - 1.78f) * 0.2f;
			float num = Mathf.Lerp(0f, 0.33f, t);
			return 0.78f + num;
		}

		private float CalculateHeightFovFactor(float aspectRatio)
		{
			float t = (1.78f - aspectRatio) * 1.44f;
			float num = Mathf.Lerp(-0.22f, 0.25f, t);
			return 1f + num;
		}

		private void OnScreenSizeChanged(int newWidth, int newHeight)
		{
			if (newHeight != 0 && newWidth != 0 && !(storyCamera == null))
			{
				UpdateCameraProperties();
			}
		}

		private void OnLateUpdate()
		{
			if (!(storyCamera == null) && storyCamera.enabled)
			{
				UpdateCameraProperties();
			}
		}
	}
}
