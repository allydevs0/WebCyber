using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class EmailMessagePanel : MonoBehaviour
	{
		[Header("Header")]
		[SerializeField]
		private TextMeshProUGUI senderText;

		[SerializeField]
		private TextMeshProUGUI subjectText;

		[SerializeField]
		private Image senderAvatar;

		[Space]
		[Header("Message")]
		[SerializeField]
		private RectTransform messageRectTransform;

		[SerializeField]
		private TextMeshProUGUI messageArea;

		[SerializeField]
		private HackerText messageAreaHackerText;

		[SerializeField]
		private RectTransform upArrow;

		[SerializeField]
		private RectTransform downArrow;

		[Space]
		[SerializeField]
		private Vector2 messageStartScale = Vector2.zero;

		[SerializeField]
		private float messageRevealTime = 0.2f;

		[Space]
		[Header("Other")]
		[SerializeField]
		private PhoneContacts phoneContacts;

		[SerializeField]
		private Vector2 panelStartPosition = new Vector3(0f, 520f);

		[SerializeField]
		private float panelRevealTime = 0.3f;

		[SerializeField]
		private float arrowAnimationTime = 0.1f;

		[SerializeField]
		private float arrowPositionOffset = 50f;

		[SerializeField]
		private bool alwaysShowHackerText;

		private IGameTextLocalizer localizer;

		private RectTransform rectTransform;

		private EmailMessage currentEmail;

		private int messageCurrentPage;

		private int messagePageCount;

		private Vector2 messageEndScale = new Vector2(0f, 1240f);

		internal string subjectLocalizationKey;

		internal string textLocalizationKey;

		private Sequence openPanelSequence;

		private Sequence closePanelSequence;

		private Sequence arrowUpSequence;

		private Sequence arrowDownSequence;

		private Vector2 originalPositionUpArrow;

		private Vector2 originalPositionDownArrow;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			openPanelSequence = DOTween.Sequence();
			openPanelSequence.Pause();
			openPanelSequence.SetAutoKill(autoKillOnCompletion: false);
			messageEndScale = messageRectTransform.sizeDelta;
			openPanelSequence.AppendCallback(() =>
			{
				rectTransform.anchoredPosition = panelStartPosition;
				messageRectTransform.sizeDelta = messageStartScale;
				base.gameObject.SetActive(value: true);
				upArrow.gameObject.SetActive(value: false);
				downArrow.gameObject.SetActive(value: true);
			});
			openPanelSequence.Append(rectTransform.DOAnchorPosY(0f, messageRevealTime).SetDelay(0.1f));
			openPanelSequence.Append(messageRectTransform.DOSizeDelta(messageEndScale, panelRevealTime));
			openPanelSequence.AppendCallback(() =>
			{
				UpdatePage(showHackerText: true);
			});
			openPanelSequence.AppendCallback(() =>
			{
				SetArrows();
			});
			closePanelSequence = DOTween.Sequence();
			closePanelSequence.Pause();
			closePanelSequence.SetAutoKill(autoKillOnCompletion: false);
			closePanelSequence.AppendCallback(() =>
			{
				messageAreaHackerText.StopAllCoroutines();
			});
			closePanelSequence.AppendCallback(() =>
			{
				messageArea.gameObject.SetActive(value: false);
			});
			closePanelSequence.Append(messageRectTransform.DOSizeDelta(messageStartScale, panelRevealTime));
			closePanelSequence.Append(rectTransform.DOAnchorPosY(panelStartPosition.y, messageRevealTime));
			closePanelSequence.AppendCallback(() =>
			{
				base.gameObject.SetActive(value: false);
			});
			originalPositionUpArrow = upArrow.anchoredPosition;
			arrowUpSequence = DOTween.Sequence();
			arrowUpSequence.SetAutoKill(autoKillOnCompletion: false);
			arrowUpSequence.Pause();
			arrowUpSequence.Append(upArrow.DOAnchorPosY(originalPositionUpArrow.y + arrowPositionOffset, arrowAnimationTime));
			arrowUpSequence.AppendCallback(() =>
			{
				UpdatePage(alwaysShowHackerText);
			});
			arrowUpSequence.Append(upArrow.DOAnchorPosY(originalPositionUpArrow.y, arrowAnimationTime));
			arrowUpSequence.AppendCallback(() =>
			{
				SetArrows();
			});
			originalPositionDownArrow = downArrow.anchoredPosition;
			arrowDownSequence = DOTween.Sequence();
			arrowDownSequence.SetAutoKill(autoKillOnCompletion: false);
			arrowDownSequence.Pause();
			arrowDownSequence.Append(downArrow.DOAnchorPosY(originalPositionDownArrow.y - arrowPositionOffset, arrowAnimationTime));
			arrowDownSequence.AppendCallback(() =>
			{
				UpdatePage(alwaysShowHackerText);
			});
			arrowDownSequence.Append(downArrow.DOAnchorPosY(originalPositionDownArrow.y, arrowAnimationTime));
			arrowDownSequence.AppendCallback(() =>
			{
				SetArrows();
			});
			localizer = Core.Instance.Localizer;
			localizer.OnLanguageChanged += OnLocalizationChanged;
		}

		private void OnDestroy()
		{
			openPanelSequence.Kill();
			closePanelSequence.Kill();
			arrowUpSequence.Kill();
			arrowDownSequence.Kill();
			localizer.OnLanguageChanged -= OnLocalizationChanged;
		}

		public void ShowPanel(EmailMessage email)
		{
			SetContent(email);
			openPanelSequence.Restart();
		}

		public void HidePanel()
		{
			closePanelSequence.Restart();
		}

		private void SetContent(EmailMessage email)
		{
			messageCurrentPage = 0;
			messagePageCount = email.message.Length;
			currentEmail = email;
			Sprite avatar = phoneContacts.contacts[email.characterNameIDOfSender].avatar;
			string characterName = localizer.GetCharacterName(email.characterNameIDOfSender);
			senderText.text = characterName;
			subjectText.text = localizer.GetEmailText(email.subject);
			senderAvatar.sprite = avatar;
			messageArea.gameObject.SetActive(value: false);
		}

		private void UpdatePage(bool showHackerText = false)
		{
			messageArea.gameObject.SetActive(value: true);
			messageArea.text = localizer.GetEmailText(currentEmail.message[messageCurrentPage]);
			Core.Instance.UIManager.UpdateTextMeshProCurrentControllerButtonGlyph(messageArea);
			messageAreaHackerText.SetText(messageArea.text);
			if (showHackerText)
			{
				messageAreaHackerText.PlayGenerateAnimation(0.15f, 0.25f);
			}
		}

		internal void OnLocalizationChanged(IGameTextLocalizer localizer)
		{
			if (currentEmail != null)
			{
				SetContent(currentEmail);
				if (base.gameObject.activeSelf && (closePanelSequence == null || !closePanelSequence.active || !closePanelSequence.IsPlaying()))
				{
					UpdatePage();
				}
			}
		}

		private void SetArrows()
		{
			upArrow.gameObject.SetActive(messageCurrentPage != 0);
			downArrow.gameObject.SetActive(messagePageCount > 1 && messageCurrentPage != messagePageCount - 1);
		}

		public void PageUp()
		{
			if (messageCurrentPage != 0)
			{
				messageCurrentPage--;
				arrowUpSequence.Restart();
			}
		}

		public void PageDown()
		{
			if (messageCurrentPage < messagePageCount - 1)
			{
				messageCurrentPage++;
				arrowDownSequence.Restart();
			}
		}
	}
}
