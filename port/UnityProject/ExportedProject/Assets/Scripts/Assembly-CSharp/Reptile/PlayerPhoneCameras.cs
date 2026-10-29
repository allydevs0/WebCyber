using System.Collections.Generic;
using Reptile.Phone;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Reptile
{
	public class PlayerPhoneCameras : MonoBehaviour
	{
		public enum PhoneCam
		{
			REAR = 0,
			FRONT = 1
		}

		public static PlayerPhoneCameras Instance;

		[SerializeField]
		private Vector2Int m_TextureSize;

		[SerializeField]
		private PostProcessVolume m_PostProcessVolume;

		private bool m_IsActive;

		private PhoneCam m_CurrentCamera;

		public RenderTexture CameraTexture { get; private set; }

		public Dictionary<PhoneCam, Camera> Cameras { get; private set; }

		public PhoneCam CurrentCamera => m_CurrentCamera;

		public Vector2Int TextureSize => m_TextureSize;

		public bool GetActive => m_IsActive;

		private void Awake()
		{
			bool flag = false;
			if (Instance != null)
			{
				if (Instance.GetActive)
				{
					flag = true;
					m_CurrentCamera = Instance.CurrentCamera;
				}
				Instance.SetActive(IsActive: false);
			}
			Instance = this;
			m_CurrentCamera = PhoneCam.REAR;
			Cameras = new Dictionary<PhoneCam, Camera>
			{
				{
					PhoneCam.REAR,
					base.transform.Find("rearCamera").GetComponent<Camera>()
				},
				{
					PhoneCam.FRONT,
					base.transform.Find("frontCamera").GetComponent<Camera>()
				}
			};
			if (CameraTexture == null || !CameraTexture.IsCreated())
			{
				CameraTexture = new RenderTexture(m_TextureSize.x, m_TextureSize.y, 32, RenderTextureFormat.Default);
				CameraTexture.name = "CameraRenderTexture";
				CameraTexture.enableRandomWrite = true;
				CameraTexture.Create();
			}
			foreach (KeyValuePair<PhoneCam, Camera> camera in Cameras)
			{
				camera.Value.targetTexture = CameraTexture;
			}
			Cameras[PhoneCam.REAR].cullingMask = WorldHandler.GetGameplayCameraCullingMask();
			Cameras[PhoneCam.FRONT].cullingMask = WorldHandler.GetGameplayCameraCullingMask();
			Cameras[PhoneCam.REAR].enabled = false;
			Cameras[PhoneCam.FRONT].enabled = false;
			WorldHandler instance = WorldHandler.instance;
			if (instance != null)
			{
				Player currentPlayer = instance.GetCurrentPlayer();
				if (currentPlayer != null)
				{
					Reptile.Phone.Phone phone = currentPlayer.phone;
					if (phone != null)
					{
						phone.GetAppInstance<AppCamera>().SetCameraTexture(CameraTexture);
						flag |= phone.IsCurrentAppAndOpen(typeof(AppCamera));
					}
				}
			}
			SetActive(flag);
			Core.OnCoreUpdatePaused += Core_OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused += Core_OnCoreUpdateUnPaused;
		}

		private void Core_OnCoreUpdateUnPaused()
		{
			Cameras[PhoneCam.REAR].enabled = m_IsActive && m_CurrentCamera == PhoneCam.REAR;
			Cameras[PhoneCam.FRONT].enabled = m_IsActive && m_CurrentCamera == PhoneCam.FRONT;
		}

		private void Core_OnCoreUpdatePaused()
		{
			Cameras[PhoneCam.REAR].enabled = false;
			Cameras[PhoneCam.FRONT].enabled = false;
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
			Core.OnCoreUpdatePaused -= Core_OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused -= Core_OnCoreUpdateUnPaused;
		}

		public void SetActive(bool IsActive)
		{
			m_IsActive = IsActive;
			Cameras[PhoneCam.REAR].enabled = IsActive && m_CurrentCamera == PhoneCam.REAR;
			Cameras[PhoneCam.FRONT].enabled = IsActive && m_CurrentCamera == PhoneCam.FRONT;
		}

		public void SwapCamera()
		{
			PhoneCam camera = (PhoneCam)((int)(m_CurrentCamera + 1) % 2);
			SetCamera(camera);
		}

		public void SetCamera(PhoneCam newCam)
		{
			Cameras[m_CurrentCamera].enabled = false;
			Cameras[newCam].enabled = true;
			m_CurrentCamera = newCam;
		}

		public Camera GetCurrentCamera()
		{
			return Cameras[m_CurrentCamera];
		}

		public void ApplyFilter(PostProcessProfile filterProfile)
		{
			m_PostProcessVolume.profile = filterProfile;
		}
	}
}
