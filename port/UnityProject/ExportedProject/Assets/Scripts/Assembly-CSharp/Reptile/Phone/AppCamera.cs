using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class AppCamera : App
	{
		public delegate void OnSavedPhotoDelegate(Camera camera);

		public delegate void OnTakenPhotoDelegate(Camera camera);

		private const float FlashDuration = 0.2f;

		private bool m_TakingPhoto;

		private bool m_Allowed = true;

		[SerializeField]
		private PostProcessProfile[] m_PhoneFilters;

		private int m_FilterIndex;

		[Space(5f)]
		[Header("Components")]
		[SerializeField]
		private RawImage m_CameraDisplay;

		[SerializeField]
		private Image m_FlashOverlay;

		[SerializeField]
		private GameObject m_PictureframePrefab;

		[SerializeField]
		private GameObject m_MessagePanel;

		[SerializeField]
		private CameraAperture m_Aperture;

		[SerializeField]
		private CameraButton m_PhotoButton;

		[SerializeField]
		private CameraButton m_FilterButton;

		[SerializeField]
		private CameraButton m_SwapButton;

		[SerializeField]
		private TMProLocalizationAddOn m_MessageText;

		private PhotosManager m_PhotosManager;

		private Sequence photoSequence;

		private float apertureDuration = 0.07f;

		private float aperturePause = 0.04f;

		private Texture2D pic;

		private bool pictureSaveResult;

		public static event OnSavedPhotoDelegate OnSavedPhoto;

		public static event OnTakenPhotoDelegate OnTakenPhoto;

		protected override void OnAppTerminate()
		{
			photoSequence?.Kill();
			m_PhotosManager = null;
			m_CameraDisplay.texture = null;
		}

		public override void OnAppEnable()
		{
			base.OnAppEnable();
			m_Allowed = true;
			m_MessagePanel.SetActive(value: false);
			m_PhotosManager = base.MyPhone.GetAppInstance<AppPhotos>().PhotosManager;
			PlayerPhoneCameras.Instance.SetActive(IsActive: true);
			TakePhotoSetup();
			if (m_CameraDisplay.texture == null)
			{
				m_CameraDisplay.texture = PlayerPhoneCameras.Instance.CameraTexture;
			}
			if (m_PhotosManager.IsPhotoStorageFull())
			{
				ShowMessagePanel("PHOTOS_FULL");
			}
		}

		public override void OnAppDisable()
		{
			if (photoSequence.IsPlaying())
			{
				photoSequence.Rewind();
				m_TakingPhoto = false;
			}
			PlayerPhoneCameras instance = PlayerPhoneCameras.Instance;
			if (instance != null)
			{
				instance.SetActive(IsActive: false);
			}
			WorldHandler instance2 = WorldHandler.instance;
			if (instance2 != null)
			{
				Player currentPlayer = instance2.GetCurrentPlayer();
				if (currentPlayer != null)
				{
					currentPlayer.PhoneHand = Vector2.zero;
				}
			}
		}

		public override void OnAppUpdate()
		{
			UpdatePlayerHand();
		}

		public override void OnPressDown()
		{
			m_SwapButton.Hold();
		}

		public override void OnReleaseDown()
		{
			PlayerPhoneCameras.Instance.SwapCamera();
			m_SwapButton.Release();
		}

		public override void OnPressRight()
		{
			m_PhotoButton.Hold();
		}

		public override void OnReleaseRight()
		{
			if (!m_TakingPhoto)
			{
				TakePhoto();
			}
			m_PhotoButton.Release();
		}

		public override void OnPressUp()
		{
			m_FilterButton.Hold();
		}

		public override void OnReleaseUp()
		{
			m_FilterIndex = (m_FilterIndex + 1) % m_PhoneFilters.Length;
			ApplyFilter(m_FilterIndex);
			m_FilterButton.Release();
		}

		internal void SetCameraTexture(Texture cameraTexture)
		{
			m_CameraDisplay.texture = cameraTexture;
		}

		private void UpdatePlayerHand()
		{
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			float x = Vector2.Dot(new Vector2(currentPlayer.forward.x, currentPlayer.forward.z), new Vector2(currentPlayer.cam.transform.left().x, currentPlayer.cam.transform.left().z));
			float num = currentPlayer.cam.cam.transform.eulerAngles.x;
			if (num > 180f)
			{
				num -= 360f;
			}
			currentPlayer.PhoneHand = new Vector2(x, 0f - num / 40f);
		}

		private void TakePhoto()
		{
			if (m_Allowed && !m_TakingPhoto)
			{
				if (OnTakenPhoto != null)
				{
					Camera currentCamera = PlayerPhoneCameras.Instance.GetCurrentCamera();
					OnTakenPhoto(currentCamera);
				}
				photoSequence.Restart();
			}
		}

		private void TakePhotoSetup()
		{
			if (photoSequence != null)
			{
				return;
			}
			photoSequence = DOTween.Sequence();
			photoSequence.SetAutoKill(autoKillOnCompletion: false);
			photoSequence.Pause();
			photoSequence.AppendCallback(() =>
			{
				m_TakingPhoto = true;
			});
			photoSequence.Append(DOTween.To(() => m_Aperture.AptertureAngle, (float x) =>
			{
				m_Aperture.AptertureAngle = x;
			}, 1.8f, apertureDuration).SetEase(Ease.Linear));
			photoSequence.AppendCallback(() =>
			{
				PlayerPhoneCameras instance = PlayerPhoneCameras.Instance;
				pictureSaveResult = m_PhotosManager.SavePhoto(instance.CameraTexture, instance.TextureSize, out pic);
				if (!pictureSaveResult)
				{
					ShowMessagePanel("PHOTOS_ERROR");
					m_TakingPhoto = false;
					photoSequence.Pause();
				}
				else if (m_PhotosManager.IsPhotoStorageFull())
				{
					ShowMessagePanel("PHOTOS_FULL");
				}
			});
			photoSequence.AppendInterval(aperturePause);
			photoSequence.AppendCallback(() =>
			{
				PictureTakenEffect(pic);
			});
			photoSequence.Append(DOTween.To(() => m_Aperture.AptertureAngle, (float x) =>
			{
				m_Aperture.AptertureAngle = x;
			}, 45f, apertureDuration).SetEase(Ease.Linear));
			photoSequence.AppendInterval(1f);
			photoSequence.AppendCallback(() =>
			{
				Core.Instance.StartCoroutine(WaitForPhotoSavingComplete());
			});
		}

		private IEnumerator WaitForPhotoSavingComplete()
		{
			ATransaction.TransactionStatus result = ATransaction.TransactionStatus.BUSY;
			while (m_PhotosManager != null && result == ATransaction.TransactionStatus.BUSY)
			{
				result = m_PhotosManager.lastSaveResult;
				yield return null;
			}
			if (m_PhotosManager != null)
			{
				TriggerPostPhotoSaveSuccesEvent();
			}
			m_TakingPhoto = false;
		}

		public void PictureTakenEffect(Texture2D tex)
		{
			Pictureframe component = Object.Instantiate(m_PictureframePrefab, base.Content).GetComponent<Pictureframe>();
			component.SetPicture(tex);
			component.StartAnimation();
			m_FlashOverlay.color = Color.white;
			m_FlashOverlay.DOFade(0f, 0.2f);
		}

		public void ApplyFilter(int filterIndex)
		{
			PlayerPhoneCameras.Instance.ApplyFilter(m_PhoneFilters[filterIndex]);
		}

		private void ShowMessagePanel(string localizationKey)
		{
			m_MessagePanel.SetActive(value: true);
			m_MessageText.AssignAndUpdateText(localizationKey, GroupOptions.Text);
			m_Allowed = false;
		}

		private void HideMessagePanel(bool allowed)
		{
			m_MessagePanel.SetActive(value: false);
			m_Allowed = allowed;
		}

		private void TriggerPostPhotoSaveSuccesEvent()
		{
			if (OnSavedPhoto != null)
			{
				Camera currentCamera = PlayerPhoneCameras.Instance.GetCurrentCamera();
				OnSavedPhoto(currentCamera);
			}
		}
	}
}
