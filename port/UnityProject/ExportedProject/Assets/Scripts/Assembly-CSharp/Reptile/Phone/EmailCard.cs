using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class EmailCard : PhoneScrollUnlockableButton
	{
		[SerializeField]
		private Sprite m_InactiveBackground;

		[SerializeField]
		private Sprite m_ActiveBackground;

		[SerializeField]
		private Color[] m_SenderColors;

		[SerializeField]
		private Color[] m_SubjectColors;

		[Header("Components")]
		[SerializeField]
		private Image m_BackgroundImage;

		[SerializeField]
		private Image m_DimOverlay;

		[SerializeField]
		private TextMeshProUGUI m_SubjectLabel;

		[SerializeField]
		private TextMeshProUGUI m_SenderLabel;

		[SerializeField]
		private Image m_ProfilePicture;

		private EmailMessage currentEmailMessage;

		private IGameTextLocalizer localizer;

		public static PhoneContacts PhoneContacts;

		public float CardHeight => base.transform.RectTransform().sizeDelta.y;

		private void Awake()
		{
			localizer = Core.Instance.Localizer;
			localizer.OnLanguageChanged += OnLanguageChanged;
			SetDimOpacity(0f);
		}

		private void OnDestroy()
		{
			if (localizer != null)
			{
				localizer.OnLanguageChanged -= OnLanguageChanged;
			}
		}

		protected override void OnSelect(bool skipAnimations = false)
		{
			base.OnSelect();
			m_BackgroundImage.sprite = m_ActiveBackground;
			m_SenderLabel.color = m_SenderColors[1];
			m_SubjectLabel.color = m_SubjectColors[1];
		}

		protected override void OnDeselect(bool skipAnimations = false)
		{
			base.OnDeselect();
			m_BackgroundImage.sprite = m_InactiveBackground;
			m_SenderLabel.color = m_SenderColors[0];
			m_SubjectLabel.color = m_SubjectColors[0];
		}

		public override void SetContent(AUnlockable content)
		{
			base.SetContent(content);
			if (localizer == null)
			{
				localizer = Core.Instance.Localizer;
			}
			currentEmailMessage = content as EmailMessage;
			Sprite avatar = PhoneContacts.contacts[currentEmailMessage.characterNameIDOfSender].avatar;
			SetEmailCardTexts();
			m_ProfilePicture.sprite = avatar;
		}

		public void SetDimOpacity(float alpha, float fadeDuration = 0f)
		{
			m_DimOverlay.DOFade(alpha, fadeDuration).SetEase(Ease.Linear);
		}

		private void SetEmailCardTexts()
		{
			m_SubjectLabel.text = localizer.GetEmailText(currentEmailMessage.subject);
			m_SenderLabel.text = localizer.GetCharacterName(currentEmailMessage.characterNameIDOfSender);
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			SetEmailCardTexts();
		}
	}
}
