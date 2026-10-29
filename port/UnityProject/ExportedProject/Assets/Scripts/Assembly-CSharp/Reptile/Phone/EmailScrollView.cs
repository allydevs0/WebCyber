using UnityEngine;

namespace Reptile.Phone
{
	public class EmailScrollView : PhoneScroll
	{
		[Header("Layout customization")]
		[SerializeField]
		public float BUTTON_SPACING = 10f;

		[SerializeField]
		private float BUTTON_TOP_OFFSET;

		[Header("Components")]
		[SerializeField]
		private AppEmail m_AppEmail;

		public float ButtonY(float posIndex)
		{
			float num = m_AppButtonPrefab.RectTransform().sizeDelta.y + BUTTON_SPACING;
			return BUTTON_TOP_OFFSET + (0f - (posIndex - (float)(SCROLL_RANGE / 2))) * num - ((SCROLL_RANGE % 2 == 0) ? (num / 2f) : 0f);
		}

		protected override void SetButtonContent(PhoneScrollButton button, int contentIndex)
		{
			if (contentIndex < m_AppEmail.Emails.Count)
			{
				(button as EmailCard).SetContent(m_AppEmail.Emails[contentIndex]);
			}
		}

		protected override void SetButtonPosition(PhoneScrollButton button, float posIndex)
		{
			button.RectTransform().anchoredPosition = new Vector2(button.RectTransform().anchoredPosition.x, ButtonY(posIndex));
		}

		public void SetListContentWithoutPosition(int contentLength)
		{
			m_ButtonAssignStartIndex = 0;
			m_ActiveScroll = null;
			base.m_ContentLength = contentLength;
			UpdateButtonContents();
		}
	}
}
