using System;
using DG.Tweening;
using UnityEngine;

namespace Reptile.Phone
{
	public class AppHomeScreen : App
	{
		private const float TOP_HEIGHT_CLOSED = 215f;

		private const float TOP_HEIGHT_OPEN = 775f;

		[SerializeField]
		private HomeScreenApp[] m_Apps;

		[Space(5f)]
		[Header("Components")]
		[SerializeField]
		private RectTransform m_TopView;

		[SerializeField]
		private HomescreenScrollView m_ScrollView;

		private const float AWAKE_SINGLE_DURATION = 0.06f;

		private const Ease AWAKE_ELEMENT_EASE = Ease.Linear;

		private bool isListeningOnLanguageChanged;

		private HomeScreenApp[] availableHomeScreenApps;

		private PhotosManager photosManager;

		public HomeScreenApp[] Apps => m_Apps;

		public override void Awake()
		{
			base.Awake();
			availableHomeScreenApps = new HomeScreenApp[m_Apps.Length];
			for (int i = 0; i < availableHomeScreenApps.Length; i++)
			{
				availableHomeScreenApps[i] = m_Apps[i];
			}
		}

		protected override void OnAppInit()
		{
			m_ScrollView.InitalizeScrollView();
			m_ScrollView.SetListContent(m_Apps.Length);
			photosManager = base.MyPhone.GetAppInstance<AppPhotos>().PhotosManager;
			LockApp(set: true, HomeScreenApp.HomeScreenAppType.FORTUNE);
		}

		public override void OnAppEnable()
		{
			base.OnAppEnable();
			DeterminePhotoAppLock();
			AwakeAnimation();
			m_ScrollView.UpdateButtonContents();
			Mapcontroller instance = Mapcontroller.Instance;
			if (instance != null)
			{
				instance.EnableMapCamera();
				instance.RestoreDefaultMapControllerState();
			}
			if (!isListeningOnLanguageChanged)
			{
				isListeningOnLanguageChanged = true;
				Core.Instance.Localizer.OnLanguageChanged += OnLanguageChanged;
			}
		}

		public override void OnAppDisable()
		{
			if (Core.Instance != null && Core.Instance.Localizer != null)
			{
				Core.Instance.Localizer.OnLanguageChanged -= OnLanguageChanged;
				isListeningOnLanguageChanged = false;
			}
			Mapcontroller instance = Mapcontroller.Instance;
			if (instance != null)
			{
				instance.DisableMapCamera();
			}
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			m_ScrollView.SetListContent(m_Apps.Length);
		}

		public override void OnPressUp()
		{
			m_ScrollView.Move(PhoneScroll.ScrollDirection.Previous, m_AudioManager);
		}

		public override void OnPressDown()
		{
			m_ScrollView.Move(PhoneScroll.ScrollDirection.Next, m_AudioManager);
		}

		public override void OnPressRight()
		{
			LeanSelector();
			m_ScrollView.HoldAnimationSelectedButton();
			if (m_ScrollView.SelectedButtton != null)
			{
				OpenApp(m_ScrollView.SelectedButtton as HomescreenButton);
			}
		}

		public override void OnReleaseLeft()
		{
			base.MyPhone.TurnOff(playSound: true);
		}

		private void AwakeAnimation()
		{
			Sequence sequence = DOTween.Sequence();
			sequence.AppendCallback(() =>
			{
				base.HandleInput = false;
			});
			sequence.AppendCallback(() =>
			{
				m_ScrollView.OtherElementsParent.gameObject.SetActive(value: true);
			});
			sequence.AppendCallback(() =>
			{
				(m_ScrollView.SelectedButtton as HomescreenButton).IsSelected = false;
			});
			m_TopView.sizeDelta = new Vector2(m_TopView.sizeDelta.x, 215f);
			sequence.Append(m_TopView.DOSizeDelta(new Vector2(m_TopView.sizeDelta.x, 775f), 0.06f)).SetEase(Ease.Linear);
			for (int num = 0; num < m_ScrollView.GetScrollRange(); num++)
			{
				RectTransform rectTransform = m_ScrollView.GetButtonByRelativeIndex(num).RectTransform();
				Vector2 endValue = m_ScrollView.ButtonPos(num);
				rectTransform.anchoredPosition = new Vector2(base.MyPhone.ScreenSize.x, rectTransform.anchoredPosition.y);
				sequence.Append(rectTransform.DOAnchorPos(endValue, 0.06f).SetEase(Ease.Linear));
			}
			Vector2 anchoredPosition = m_ScrollView.Selector.RectTransform().anchoredPosition;
			anchoredPosition.y = m_ScrollView.SelectedButtton.RectTransform().anchoredPosition.y;
			m_ScrollView.Selector.RectTransform().anchoredPosition = new Vector2(base.MyPhone.ScreenSize.x, anchoredPosition.y);
			m_ScrollView.Selector.RectTransform().localScale = Vector2.one;
			sequence.AppendCallback(() =>
			{
				(m_ScrollView.SelectedButtton as HomescreenButton).IsSelected = true;
			});
			sequence.Append(m_ScrollView.Selector.RectTransform().DOAnchorPos(anchoredPosition, 0.06f).SetEase(Ease.Linear));
			sequence.OnComplete(() =>
			{
				base.HandleInput = true;
			});
		}

		private void OpenApp(HomescreenButton appToOpen)
		{
			Sequence sequence = DOTween.Sequence();
			Sequence sequence2 = DOTween.Sequence();
			Sequence sequence3 = DOTween.Sequence();
			Sequence sequence4 = DOTween.Sequence();
			if (appToOpen.AssignedApp.AppType == HomeScreenApp.HomeScreenAppType.PHOTO_ALBUM)
			{
				base.MyPhone.GetAppInstance<AppPhotos>().OnAppEnable();
			}
			for (int num = m_ScrollView.GetScrollRange() - 1; num >= 0; num--)
			{
				RectTransform target = m_ScrollView.GetButtonByRelativeIndex(num).RectTransform();
				if (num != m_ScrollView.GetSelectorPos())
				{
					sequence2.Append(target.DOAnchorPosX(0f - base.MyPhone.ScreenSize.x, 0.06f).SetEase(Ease.Linear));
				}
			}
			sequence4.AppendCallback(() =>
			{
				m_ScrollView.Selector.RectTransform().DOKill();
			});
			sequence4.Append(m_ScrollView.Selector.RectTransform().DOAnchorPosX(0f, 0.3f).SetEase(Ease.OutBounce));
			int num2 = 2;
			float interval = 0.1f;
			for (int num3 = 0; num3 < num2; num3++)
			{
				sequence3.Append(m_ScrollView.Selector.Background.DOFade(0f, 0f));
				sequence3.AppendInterval(interval);
				sequence3.Append(m_ScrollView.Selector.Background.DOFade(1f, 0f));
				sequence3.AppendInterval(interval);
			}
			sequence3.Append(m_ScrollView.Selector.RectTransform().DOScale(0f, 0f));
			sequence3.Append(m_ScrollView.SelectedButtton.RectTransform().DOAnchorPosX(base.MyPhone.ScreenSize.x, 0f));
			sequence3.AppendCallback(() =>
			{
				m_ScrollView.OtherElementsParent.gameObject.SetActive(value: false);
			});
			sequence.AppendCallback(() =>
			{
				base.HandleInput = false;
			});
			sequence.Append(sequence2);
			sequence.Join(sequence4);
			sequence.Join(sequence3);
			sequence.Append(m_TopView.DOSizeDelta(new Vector2(m_TopView.sizeDelta.x, 215f), 0.06f)).SetEase(Ease.Linear);
			sequence.AppendCallback(() =>
			{
				base.HandleInput = true;
			});
			sequence.OnComplete(() =>
			{
				base.MyPhone.OpenApp(Type.GetType("Reptile.Phone." + appToOpen.AssignedApp.AppName));
			});
			m_AudioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_Confirm);
		}

		private void LeanSelector()
		{
			m_ScrollView.Selector.RectTransform().DOAnchorPosX(50f, 0.3f);
		}

		public void PushNotification()
		{
			base.MyPhone.PushNotification(this, Core.Instance.Localizer.GetText("PHONE_NOTIFICATION_MAP"), null);
		}

		public override void OpenContent(AUnlockable unlockable, bool appAlreadyOpen)
		{
			base.MyPhone.ReturnToHome();
		}

		public void LockApp(bool set, HomeScreenApp.HomeScreenAppType homeScreenAppType)
		{
			HomeScreenApp homeScreenApp = FindHomeScreenApp(homeScreenAppType);
			if (!(homeScreenApp == null))
			{
				if (set)
				{
					RemoveApp(homeScreenApp);
				}
				else
				{
					AddApp(homeScreenApp);
				}
			}
		}

		private HomeScreenApp FindHomeScreenApp(HomeScreenApp.HomeScreenAppType homeScreenAppType)
		{
			for (int i = 0; i < availableHomeScreenApps.Length; i++)
			{
				if (availableHomeScreenApps[i].AppType == homeScreenAppType)
				{
					return availableHomeScreenApps[i];
				}
			}
			return null;
		}

		private bool IsPartOfCurrentApps(HomeScreenApp homeScreenApp)
		{
			for (int i = 0; i < m_Apps.Length; i++)
			{
				if (m_Apps[i].Equals(homeScreenApp))
				{
					return true;
				}
			}
			return false;
		}

		private void RemoveApp(HomeScreenApp homeScreenApp)
		{
			if (!IsPartOfCurrentApps(homeScreenApp))
			{
				return;
			}
			HomeScreenApp[] array = new HomeScreenApp[m_Apps.Length - 1];
			int num = 0;
			for (int i = 0; i < m_Apps.Length; i++)
			{
				if (!m_Apps[i].Equals(homeScreenApp))
				{
					array[num] = m_Apps[i];
					num++;
				}
			}
			m_Apps = array;
			m_ScrollView.SetListContent(m_Apps.Length);
			m_ScrollView.UpdateButtonContents();
		}

		private void AddApp(HomeScreenApp homeScreenApp)
		{
			if (IsPartOfCurrentApps(homeScreenApp))
			{
				return;
			}
			HomeScreenApp[] array = new HomeScreenApp[m_Apps.Length + 1];
			for (int i = 0; i < array.Length; i++)
			{
				if (i == m_Apps.Length)
				{
					array[i] = homeScreenApp;
				}
				else
				{
					array[i] = m_Apps[i];
				}
			}
			m_Apps = array;
			m_ScrollView.SetListContent(m_Apps.Length);
			m_ScrollView.UpdateButtonContents();
		}

		private void LockPhotosFeature()
		{
			AppHomeScreen appInstance = base.MyPhone.GetAppInstance<AppHomeScreen>();
			appInstance.LockApp(set: true, HomeScreenApp.HomeScreenAppType.PHOTO_ALBUM);
			appInstance.LockApp(set: true, HomeScreenApp.HomeScreenAppType.CAMERA);
			m_ScrollView.UpdateArrows();
		}

		private void UnlockPhotosFeature()
		{
			AppHomeScreen appInstance = base.MyPhone.GetAppInstance<AppHomeScreen>();
			appInstance.LockApp(set: false, HomeScreenApp.HomeScreenAppType.PHOTO_ALBUM);
			appInstance.LockApp(set: false, HomeScreenApp.HomeScreenAppType.CAMERA);
			m_ScrollView.UpdateArrows();
		}

		private void DeterminePhotoAppLock()
		{
			if (Core.Instance.SaveManager.CurrentSaveSlot.cameraAppLocked)
			{
				LockPhotosFeature();
			}
			else
			{
				UnlockPhotosFeature();
			}
		}

		public App AppInstance(string appName)
		{
			return base.MyPhone.AppInstances[appName];
		}
	}
}
