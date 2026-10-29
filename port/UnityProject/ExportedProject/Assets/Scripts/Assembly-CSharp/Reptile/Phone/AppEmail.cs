using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Reptile.Phone
{
	public class AppEmail : App
	{
		private enum States
		{
			Closed = 0,
			ListView = 1,
			EmailView = 2
		}

		private States m_State;

		public const float EMAIL_MAX_HEIGHT = 1400f;

		[Header("Layout")]
		[SerializeField]
		private float m_CardOpenY;

		[Header("Components")]
		[SerializeField]
		private EmailScrollView m_CardsScroll;

		[Space(2f)]
		[SerializeField]
		private EmailMessagePanel messagePanel;

		[SerializeField]
		private PhoneContacts phoneContacts;

		private List<EmailMessage> m_Emails;

		private int previousEmailCount;

		private float continuousScrollTimer;

		private const float initialScrollInterval = 0.4f;

		private const float continuousScrollInterval = 0.1f;

		private Vector2[] m_OldButtonPositions;

		public List<EmailMessage> Emails => m_Emails;

		protected override void OnAppInit()
		{
			m_State = States.ListView;
			m_CardsScroll.InitalizeScrollView();
			m_OldButtonPositions = new Vector2[m_CardsScroll.GetButtons().Length];
			EmailCard.PhoneContacts = phoneContacts;
			messagePanel.HidePanel();
			RefreshList();
		}

		public override void OnAppRefresh()
		{
			RefreshList();
		}

		protected override void OnAppTerminate()
		{
			base.OnAppTerminate();
			EmailCard.PhoneContacts = null;
		}

		public override void OnAppEnable()
		{
			base.OnAppEnable();
			RefreshList();
			if (m_State == States.Closed)
			{
				if (messagePanel.gameObject.activeInHierarchy)
				{
					m_State = States.EmailView;
				}
				else
				{
					m_State = States.ListView;
				}
			}
		}

		public override void OnAppDisable()
		{
			base.OnAppDisable();
			m_State = States.Closed;
		}

		public override void OnPressUp()
		{
			switch (m_State)
			{
			case States.ListView:
				m_CardsScroll.Move(PhoneScroll.ScrollDirection.Previous, m_AudioManager);
				continuousScrollTimer -= 0.4f;
				break;
			case States.EmailView:
				messagePanel.PageUp();
				break;
			}
		}

		public override void OnPressDown()
		{
			switch (m_State)
			{
			case States.ListView:
				m_CardsScroll.Move(PhoneScroll.ScrollDirection.Next, m_AudioManager);
				continuousScrollTimer -= 0.4f;
				break;
			case States.EmailView:
				messagePanel.PageDown();
				break;
			}
		}

		public override void OnPressRight()
		{
			m_CardsScroll.HoldAnimationSelectedButton();
		}

		public override void OnHoldUp()
		{
			if (m_State == States.ListView)
			{
				continuousScrollTimer += Core.dt;
				if (continuousScrollTimer >= 0.1f)
				{
					continuousScrollTimer = 0f;
					m_CardsScroll.Move(PhoneScroll.ScrollDirection.Previous, m_AudioManager);
				}
			}
		}

		public override void OnHoldDown()
		{
			if (m_State == States.ListView)
			{
				continuousScrollTimer += Core.dt;
				if (continuousScrollTimer >= 0.1f)
				{
					continuousScrollTimer = 0f;
					m_CardsScroll.Move(PhoneScroll.ScrollDirection.Next, m_AudioManager);
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
			switch (m_State)
			{
			case States.ListView:
				OpenSelectedEmail();
				break;
			case States.EmailView:
				OpenListView();
				break;
			}
			m_CardsScroll.ActivateAnimationSelectedButton();
		}

		public override void OnReleaseLeft()
		{
			switch (m_State)
			{
			case States.ListView:
				base.MyPhone.CloseCurrentApp();
				break;
			case States.EmailView:
				OpenListView();
				m_CardsScroll.ActivateAnimationSelectedButton();
				break;
			}
		}

		private void RefreshList()
		{
			Dictionary<string, int> emailIndexTable = Core.Instance.SaveManager.CurrentSaveSlot.emailIndexTable;
			m_Emails = new List<EmailMessage>(emailIndexTable.Count);
			for (int i = 0; i < emailIndexTable.Count; i++)
			{
				m_Emails.Add(null);
			}
			for (int j = 0; j < base.Unlockables.Length; j++)
			{
				EmailMessage emailMessage = base.Unlockables[j] as EmailMessage;
				if (Core.Instance.Platform.User.GetUnlockableSaveDataFor(emailMessage).IsUnlocked)
				{
					if (!emailIndexTable.ContainsKey(emailMessage.Uid))
					{
						emailIndexTable.Add(emailMessage.Uid, emailIndexTable.Count);
						m_Emails.Add(emailMessage);
					}
					else
					{
						int index = emailIndexTable[emailMessage.Uid];
						m_Emails[index] = emailMessage;
					}
				}
			}
			m_Emails.Reverse();
			if (previousEmailCount == 0)
			{
				m_CardsScroll.SetListContent(m_Emails.Count);
			}
			else
			{
				m_CardsScroll.UpdateListContent(m_Emails.Count);
			}
			previousEmailCount = m_Emails.Count;
		}

		public override void OpenContent(AUnlockable unlockable, bool appAlreadyOpen)
		{
			if (!(unlockable != null))
			{
				return;
			}
			if (appAlreadyOpen)
			{
				RefreshList();
				if (m_State == States.EmailView)
				{
					PhoneScrollButton[] buttons = m_CardsScroll.GetButtons();
					for (int i = 0; i < buttons.Length; i++)
					{
						(buttons[i] as EmailCard).RectTransform().anchoredPosition = m_OldButtonPositions[i];
					}
				}
			}
			PhoneScrollButton[] buttons2 = m_CardsScroll.GetButtons();
			int num = buttons2.Length;
			int num2 = -1;
			for (int j = 0; j < num; j++)
			{
				EmailCard emailCard = buttons2[j] as EmailCard;
				if (emailCard.AssignedContent != null && emailCard.AssignedContent.Uid == unlockable.Uid)
				{
					num2 = j - m_CardsScroll.GetOverflowButtonAmount();
					break;
				}
			}
			if (num2 >= 0)
			{
				int selectorPos = m_CardsScroll.GetSelectorPos();
				int num3 = num2 - selectorPos;
				PhoneScroll.ScrollDirection direction = (PhoneScroll.ScrollDirection)Mathf.Sign(num3);
				for (int k = 0; k < Mathf.Abs(num3); k++)
				{
					m_CardsScroll.Move(direction, m_AudioManager);
				}
				OpenSelectedEmail();
			}
		}

		private void OpenSelectedEmail()
		{
			if (m_Emails.Count == 0)
			{
				return;
			}
			(m_CardsScroll.SelectedButtton as PhoneScrollUnlockableButton).SetUnread(isUnread: false);
			int selectorPos = m_CardsScroll.GetSelectorPos();
			Sequence sequence = DOTween.Sequence();
			sequence.AppendCallback(() =>
			{
				base.HandleInput = false;
			});
			PhoneScrollButton[] buttons = m_CardsScroll.GetButtons();
			foreach (PhoneScrollButton button in buttons)
			{
				if (button != m_CardsScroll.SelectedButtton)
				{
					sequence.AppendCallback(() =>
					{
						(button as EmailCard).SetDimOpacity(0.7f, 0.15f);
					});
				}
			}
			for (int num2 = 0; num2 < m_CardsScroll.GetButtons().Length; num2++)
			{
				int num3 = num2 - m_CardsScroll.GetOverflowButtonAmount();
				PhoneScrollButton buttonByAbsoluteIndex = m_CardsScroll.GetButtonByAbsoluteIndex(num2);
				m_OldButtonPositions[num2] = buttonByAbsoluteIndex.RectTransform().anchoredPosition;
				if (num3 <= selectorPos)
				{
					sequence.Join(buttonByAbsoluteIndex.RectTransform().DOAnchorPosY(m_CardOpenY - (float)(num3 - selectorPos) * (buttonByAbsoluteIndex.RectTransform().sizeDelta.y + m_CardsScroll.BUTTON_SPACING), 0.15f)).SetEase(Ease.Linear);
				}
				if (num3 > selectorPos)
				{
					float num4 = m_CardOpenY - 1400f - buttonByAbsoluteIndex.RectTransform().sizeDelta.y - m_CardsScroll.BUTTON_SPACING;
					int num5 = num3 - selectorPos - 1;
					sequence.Join(buttonByAbsoluteIndex.RectTransform().DOAnchorPosY(num4 - (float)num5 * (buttonByAbsoluteIndex.RectTransform().sizeDelta.y + m_CardsScroll.BUTTON_SPACING), 0.15f)).SetEase(Ease.Linear);
				}
			}
			messagePanel.ShowPanel((m_CardsScroll.SelectedButtton as EmailCard).AssignedContent as EmailMessage);
			sequence.OnComplete(() =>
			{
				base.HandleInput = true;
			});
			m_State = States.EmailView;
		}

		private void OpenListView()
		{
			Sequence sequence = DOTween.Sequence();
			sequence.AppendCallback(() =>
			{
				base.HandleInput = false;
			});
			sequence.AppendCallback(() =>
			{
				messagePanel.HidePanel();
			});
			sequence.AppendInterval(0.1f);
			PhoneScrollButton[] buttons = m_CardsScroll.GetButtons();
			for (int num = 0; num < buttons.Length; num++)
			{
				EmailCard emailCard = buttons[num] as EmailCard;
				sequence.AppendCallback(() =>
				{
					emailCard.SetDimOpacity(0f, 0.15f);
				});
				if (num == 0)
				{
					sequence.Append(emailCard.RectTransform().DOAnchorPosY(m_OldButtonPositions[num].y, 0.15f)).SetEase(Ease.Linear);
				}
				else
				{
					sequence.Join(emailCard.RectTransform().DOAnchorPosY(m_OldButtonPositions[num].y, 0.15f)).SetEase(Ease.Linear);
				}
			}
			sequence.OnComplete(() =>
			{
				base.HandleInput = true;
			});
			m_State = States.ListView;
		}

		public void PushNotification(EmailMessage email)
		{
			base.MyPhone.PushNotification(this, Core.Instance.Localizer.GetCharacterName(email.characterNameIDOfSender), email);
		}
	}
}
