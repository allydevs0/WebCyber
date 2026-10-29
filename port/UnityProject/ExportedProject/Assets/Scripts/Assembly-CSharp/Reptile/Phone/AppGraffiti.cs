using System.Collections.Generic;
using UnityEngine;

namespace Reptile.Phone
{
	public class AppGraffiti : App
	{
		private bool m_InfoOpen;

		[SerializeField]
		private GraffitiScrollView m_ScrollView;

		[SerializeField]
		private GraffitiInfoPanel m_InfoPanel;

		private float continuousScrollTimer;

		private const float initialScrollInterval = 0.4f;

		private const float continuousScrollInterval = 0.1f;

		public List<GraffitiAppEntry> GraffitiArt { get; private set; }

		protected override void OnAppInit()
		{
			m_ScrollView.InitalizeScrollView();
			CloseInfo();
		}

		public override void OnAppRefresh()
		{
			RefreshList();
		}

		public override void OnAppEnable()
		{
			base.OnAppEnable();
			RefreshList();
		}

		public override void OnPressRight()
		{
			if (!m_InfoOpen)
			{
				m_ScrollView.HoldAnimationSelectedButton();
			}
		}

		public override void OnPressUp()
		{
			if (!m_InfoOpen)
			{
				m_ScrollView.Move(PhoneScroll.ScrollDirection.Previous, m_AudioManager);
				continuousScrollTimer -= 0.4f;
			}
		}

		public override void OnPressDown()
		{
			if (!m_InfoOpen)
			{
				m_ScrollView.Move(PhoneScroll.ScrollDirection.Next, m_AudioManager);
				continuousScrollTimer -= 0.4f;
			}
		}

		public override void OnHoldUp()
		{
			if (!m_InfoOpen)
			{
				continuousScrollTimer += Core.dt;
				if (continuousScrollTimer >= 0.1f)
				{
					continuousScrollTimer = 0f;
					m_ScrollView.Move(PhoneScroll.ScrollDirection.Previous, m_AudioManager);
				}
			}
		}

		public override void OnHoldDown()
		{
			if (!m_InfoOpen)
			{
				continuousScrollTimer += Core.dt;
				if (continuousScrollTimer >= 0.1f)
				{
					continuousScrollTimer = 0f;
					m_ScrollView.Move(PhoneScroll.ScrollDirection.Next, m_AudioManager);
				}
			}
		}

		public override void OnReleaseUp()
		{
			continuousScrollTimer = 0f;
		}

		public override void OnReleaseDown()
		{
			continuousScrollTimer = 0f;
		}

		public override void OnReleaseRight()
		{
			if (!m_InfoOpen)
			{
				OpenInfo();
			}
		}

		public override void OnReleaseLeft()
		{
			if (m_InfoOpen)
			{
				CloseInfo();
			}
			else
			{
				base.MyPhone.CloseCurrentApp();
			}
		}

		private void RefreshList()
		{
			int num = 0;
			if (GraffitiArt != null)
			{
				num = GraffitiArt.Count;
			}
			GraffitiArt = new List<GraffitiAppEntry>();
			for (int i = 0; i < base.Unlockables.Length; i++)
			{
				if (Core.Instance.Platform.User.GetUnlockableSaveDataFor(base.Unlockables[i]).IsUnlocked)
				{
					GraffitiArt.Add(base.Unlockables[i] as GraffitiAppEntry);
				}
			}
			GraffitiArt.Sort((GraffitiAppEntry a, GraffitiAppEntry b) =>
			{
				int num2 = a.Size.CompareTo(b.Size);
				return (num2 == 0) ? a.Artist.CompareTo(b.Artist) : num2;
			});
			if (num == 0)
			{
				m_ScrollView.SetListContent(GraffitiArt.Count);
			}
			else
			{
				m_ScrollView.UpdateListContent(GraffitiArt.Count);
			}
		}

		public void OpenInfo()
		{
			m_InfoPanel.Show(GraffitiArt[m_ScrollView.GetContentIndex()]);
			m_InfoOpen = true;
			PhoneScrollButton[] buttons = m_ScrollView.GetButtons();
			int num = buttons.Length;
			_ = m_ScrollView.SelectedButtton;
			for (int i = 0; i < num; i++)
			{
				(buttons[i] as GraffitiScrollButton).SetDimOpacity(0.7f, 0.15f);
			}
			(m_ScrollView.SelectedButtton as PhoneScrollUnlockableButton).SetUnread(isUnread: false);
			m_ScrollView.ActivateAnimationSelectedButton();
		}

		public void CloseInfo()
		{
			m_InfoPanel.Hide();
			m_InfoOpen = false;
			PhoneScrollButton[] buttons = m_ScrollView.GetButtons();
			int num = buttons.Length;
			for (int i = 0; i < num; i++)
			{
				(buttons[i] as GraffitiScrollButton).SetDimOpacity(0f, 0.15f);
			}
		}

		public override void OpenContent(AUnlockable unlockable, bool appAlreadyOpen)
		{
			if (!(unlockable == null))
			{
				if (appAlreadyOpen)
				{
					RefreshList();
				}
				m_ScrollView.FindAndMoveTo(unlockable, GraffitiArt, m_AudioManager);
				if (!m_InfoOpen)
				{
					OpenInfo();
				}
				else
				{
					m_InfoPanel.Show(GraffitiArt[m_ScrollView.GetContentIndex()]);
				}
			}
		}
	}
}
