using Kaj.SimpleEasing;
using UnityEngine;

namespace Reptile
{
	public class CameraShakeComponent : MonoBehaviour
	{
		public GameplayCamera.ScreenShake screenShakeProfileExtraLight;

		public GameplayCamera.ScreenShake screenShakeProfileLight;

		public GameplayCamera.ScreenShake screenShakeProfileMedium;

		public GameplayCamera.ScreenShake screenShakeProfileHeavy;

		private bool shaking;

		private float screenShakeTime;

		private float screenShakeDuration;

		private int screenShakeFrameCounter;

		private Vector3 screenShakeDir = Vector3.right;

		private Vector3 lastScreenShakeLocalPos;

		private Vector3 screenShakeOriginPos;

		private Vector3 screenShakeLocalOriginRot;

		private float shakePosMagnitude = 0.08f;

		private float shakeRotMagnitude = 2f;

		private float shakePosSpeed = 150f;

		private float shakeRotSpeed = 27.5f;

		private SimpleEasing.EaseType shakeDecayEase = SimpleEasing.EaseType.EaseLinear;

		private ScreenShakeType curShakeType;

		private bool animatedCamera;

		private Transform cameraT;

		private void Awake()
		{
			cameraT = base.transform;
		}

		public void StopShake()
		{
			shaking = false;
			if (!animatedCamera)
			{
				cameraT.position = screenShakeOriginPos;
				cameraT.localRotation = Quaternion.Euler(screenShakeLocalOriginRot);
			}
		}

		public void StartShake(ScreenShakeType shakeType, Vector3 dir, bool animatedCam, float duration)
		{
			shaking = true;
			curShakeType = shakeType;
			GameplayCamera.ScreenShake screenShake = screenShakeProfileLight;
			animatedCamera = animatedCam;
			switch (curShakeType)
			{
			case ScreenShakeType.EXTRALIGHT:
				screenShake = screenShakeProfileExtraLight;
				break;
			case ScreenShakeType.LIGHT:
				screenShake = screenShakeProfileLight;
				break;
			case ScreenShakeType.MEDIUM:
				screenShake = screenShakeProfileMedium;
				break;
			case ScreenShakeType.HEAVY:
				screenShake = screenShakeProfileHeavy;
				break;
			}
			shakePosMagnitude = screenShake.shakePosMagnitude;
			shakeRotMagnitude = screenShake.shakeRotMagnitude;
			shakePosSpeed = screenShake.shakePosSpeed;
			shakeRotSpeed = screenShake.shakeRotSpeed;
			shakeDecayEase = screenShake.shakeDecayEase;
			if (cameraT == null)
			{
				cameraT = base.transform;
			}
			screenShakeOriginPos = cameraT.position;
			screenShakeLocalOriginRot = cameraT.localEulerAngles;
			screenShakeDir = dir;
			screenShakeDuration = duration;
		}

		public void SetTime(float time)
		{
			screenShakeTime = time;
		}

		private void LateUpdate()
		{
			if (shaking)
			{
				float num = SimpleEasing.SimpleEase(shakeDecayEase, screenShakeTime, 1f, -1f, screenShakeDuration);
				float z = 15f * Mathf.Sin(Time.time * (shakePosSpeed * 0.45f));
				Vector3 vector = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0f, 0f, z)), Vector3.one).MultiplyPoint(Vector3.Reflect(screenShakeDir, Vector3.up)) * shakePosMagnitude * (Mathf.Cos(screenShakeTime * shakePosSpeed) * num);
				if (screenShakeFrameCounter == 0)
				{
					vector = lastScreenShakeLocalPos;
				}
				else
				{
					lastScreenShakeLocalPos = vector;
				}
				screenShakeFrameCounter++;
				if (screenShakeFrameCounter >= 3)
				{
					screenShakeFrameCounter = 0;
				}
				Vector3 vector2 = new Vector3
				{
					x = (shakeRotMagnitude * -0.5f + Mathf.PerlinNoise(Time.time * shakeRotSpeed, 6f) * shakeRotMagnitude) * num,
					y = (shakeRotMagnitude * -0.5f + Mathf.PerlinNoise(Time.time * shakeRotSpeed, 7f) * shakeRotMagnitude) * num,
					z = 0f
				};
				if (animatedCamera)
				{
					cameraT.position += cameraT.TransformVector(vector);
					cameraT.localRotation = Quaternion.Euler(cameraT.localEulerAngles + vector2);
				}
				else
				{
					cameraT.position = screenShakeOriginPos + cameraT.TransformVector(vector);
					cameraT.localRotation = Quaternion.Euler(screenShakeLocalOriginRot + vector2);
				}
			}
		}
	}
}
