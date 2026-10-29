using UnityEngine;

namespace Reptile.Phone
{
	[RequireComponent(typeof(Canvas))]
	public class App : MonoBehaviour
	{
		protected AudioManager m_AudioManager;

		[SerializeField]
		private HackerText m_HeaderLabel;

		[SerializeField]
		protected AUnlockable[] m_Unlockables;

		protected Notification m_Notification;

		protected Phone MyPhone { get; private set; }

		public RectTransform Content { get; private set; }

		public bool HandleInput { get; protected set; }

		public AUnlockable[] Unlockables => m_Unlockables;

		public Notification Notification => m_Notification;

		public virtual void Awake()
		{
			Content = base.transform.Find("Content") as RectTransform;
			m_Notification = GetComponent<Notification>();
			if ((bool)m_Notification)
			{
				m_Notification.InitNotification(this);
			}
		}

		public void Init(Phone phone, AudioManager audioManager)
		{
			MyPhone = phone;
			HandleInput = true;
			m_AudioManager = audioManager;
			if (m_Notification != null)
			{
				m_Notification.gameObject.SetActive(value: false);
			}
			OnAppInit();
		}

		public void Terminate()
		{
			MyPhone = null;
			OnAppTerminate();
		}

		protected virtual void OnAppInit()
		{
		}

		protected virtual void OnAppTerminate()
		{
		}

		public virtual void OnAppEnable()
		{
			if (m_HeaderLabel != null)
			{
				m_HeaderLabel.PlayGenerateAnimation(0.15f, 0.1f);
			}
		}

		public virtual void OnAppUpdate()
		{
		}

		public virtual void OnAppLateUpdate()
		{
		}

		public virtual void OnAppDisable()
		{
		}

		public virtual void OnAppRefresh()
		{
		}

		public virtual void OnPressLeft()
		{
		}

		public virtual void OnPressRight()
		{
		}

		public virtual void OnPressUp()
		{
		}

		public virtual void OnPressDown()
		{
		}

		public virtual void OnHoldLeft()
		{
		}

		public virtual void OnHoldRight()
		{
		}

		public virtual void OnHoldUp()
		{
		}

		public virtual void OnHoldDown()
		{
		}

		public virtual void OnReleaseLeft()
		{
			MyPhone.CloseCurrentApp();
		}

		public virtual void OnReleaseRight()
		{
		}

		public virtual void OnReleaseUp()
		{
		}

		public virtual void OnReleaseDown()
		{
		}

		public virtual void OpenContent(AUnlockable unlockable, bool appAlreadyOpen)
		{
		}

		public void PushNotification(string topBarText, AUnlockable unlockable)
		{
			MyPhone.PushNotification(this, topBarText, unlockable);
		}
	}
}
