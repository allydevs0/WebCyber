using System.Collections.Generic;
using UnityEngine;

namespace Reptile.Phone
{
	public class GraffitiScrollView : PhoneScroll
	{
		[SerializeField]
		private AppGraffiti m_GraffitiApp;

		[SerializeField]
		private float BUTTON_SPACING;

		[SerializeField]
		private float BUTTON_TOP_OFFSET;

		protected override void SetButtonContent(PhoneScrollButton button, int contentIndex)
		{
			if (contentIndex < m_GraffitiApp.GraffitiArt.Count)
			{
				GraffitiAppEntry content = m_GraffitiApp.GraffitiArt[contentIndex];
				(button as GraffitiScrollButton).SetContent(content);
			}
		}

		protected override void SetButtonPosition(PhoneScrollButton button, float posIndex)
		{
			RectTransform rectTransform = button.RectTransform();
			float num = rectTransform.sizeDelta.y + BUTTON_SPACING;
			Vector2 anchoredPosition = new Vector2
			{
				x = rectTransform.anchoredPosition.x,
				y = BUTTON_TOP_OFFSET + (0f - (posIndex - (float)(SCROLL_RANGE / 2))) * num - ((SCROLL_RANGE % 2 == 0) ? (num / 2f) : 0f)
			};
			rectTransform.anchoredPosition = anchoredPosition;
		}

		public void FindAndMoveTo(AUnlockable unlockable, List<GraffitiAppEntry> GraffitiArt, AudioManager m_AudioManager)
		{
			int num = -1;
			for (int i = 0; i < GraffitiArt.Count; i++)
			{
				if (GraffitiArt[i].Uid == unlockable.Uid)
				{
					num = i;
					break;
				}
			}
			if (num >= m_ButtonAssignStartIndex && num < m_ButtonAssignStartIndex + SCROLL_RANGE)
			{
				int num2 = num - m_SelectorPos;
				ScrollDirection direction = (ScrollDirection)Mathf.Sign(num2);
				int num3 = Mathf.Abs(num2);
				for (int j = 0; j < num3; j++)
				{
					Move(direction, m_AudioManager);
				}
			}
			else
			{
				m_ButtonAssignStartIndex = Mathf.Clamp(num, 0, base.m_ContentLength - SCROLL_RANGE);
				DeselectButton(m_SelectorPos);
				m_SelectorPos = num - m_ButtonAssignStartIndex;
				UpdateButtonContents();
				SelectButton(m_SelectorPos);
			}
		}
	}
}
