using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class HomescreenButton : PhoneScrollButton
	{
		[SerializeField]
		private TextMeshProUGUI m_TitleLabel;

		[SerializeField]
		private Image m_IconImage;

		[SerializeField]
		private Image m_UnreadBubble;

		[SerializeField]
		private TextMeshProUGUI m_UnreadLabel;

		[Space(5f)]
		[SerializeField]
		private Color InactiveColor = Color.black;

		[SerializeField]
		private Color SelectedColor = Color.white;

		private Color m_PreviousColor = Color.magenta;

		private Color m_CurrentColor = Color.white;

		private float m_CurrentAlpha = 1f;

		private int m_UnreadAmount;

		public HomeScreenApp AssignedApp { get; private set; }

		protected override void OnCreate()
		{
			FadeColor(InactiveColor, animate: false);
			m_IconImage.rectTransform.localScale = Vector3.one;
			m_IconImage.rectTransform.localEulerAngles = Vector3.zero;
		}

		private void Update()
		{
			Color currentColor = m_CurrentColor;
			currentColor.a = m_CurrentAlpha;
			m_PreviousColor = currentColor;
			Color white = Color.white;
			white.a = m_CurrentAlpha;
			m_UnreadBubble.color = white;
			m_UnreadLabel.color = white;
		}

		public void SetContent(HomeScreenApp content, int unread)
		{
			AssignedApp = content;
			m_TitleLabel.text = Core.Instance.Localizer.GetUserInterfaceText(AssignedApp.DisplayName);
			m_IconImage.sprite = AssignedApp.DisplayIcon;
			m_UnreadAmount = unread;
			m_UnreadBubble.gameObject.SetActive(Convert.ToBoolean(m_UnreadAmount > 0));
			m_UnreadLabel.text = m_UnreadAmount.ToString();
		}

		protected override void OnSelect(bool skipAnimations = false)
		{
			FadeColor(SelectedColor);
		}

		protected override void OnDeselect(bool skipAnimations = false)
		{
			FadeColor(InactiveColor);
			m_IconImage.rectTransform.localScale = Vector3.one;
			m_IconImage.rectTransform.localEulerAngles = Vector3.zero;
		}

		public override void OnActivate()
		{
		}

		public override void OnHold()
		{
		}

		protected override void SelectedUpdate()
		{
			m_IconImage.rectTransform.localScale = new Vector3(Mathf.PingPong(Time.time, 0.25f) + 1f, Mathf.PingPong(Time.time, 0.25f) + 1f, 1f);
		}

		private void SetContentColor(Color newColor)
		{
			m_TitleLabel.color = newColor;
			m_IconImage.color = newColor;
		}

		public void SetColor(Color newColor)
		{
			m_CurrentColor = newColor;
		}

		public void FadeColor(Color newColor, bool animate = true)
		{
			DOTween.To(() => m_CurrentColor, (Color x) =>
			{
				m_CurrentColor = x;
			}, newColor, 0.08f * (float)Convert.ToInt32(animate)).SetEase(Ease.Linear);
		}

		public void SetAlpha(float newAlpha)
		{
			m_CurrentAlpha = newAlpha;
		}

		public void FadeAlpha(float newAlpha)
		{
			DOTween.To(() => m_CurrentAlpha, (float x) =>
			{
				m_CurrentAlpha = x;
			}, newAlpha, 0.08f).SetEase(Ease.Linear);
		}
	}
}
