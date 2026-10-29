using System.Collections.Generic;
using UnityEngine;

namespace Reptile.Phone
{
	public class MusicPlayerTrackList : PhoneScroll
	{
		public List<int> musicAlfabeticList;

		[SerializeField]
		private float BUTTON_SPACING = 75f;

		[SerializeField]
		private float BUTTON_TOP_OFFSET;

		[SerializeField]
		private AppMusicPlayer m_MusicApp;

		protected override void OnButtonCreated(PhoneScrollButton newButton)
		{
			(newButton as MusicPlayerTrackButton).SetMusicApp(m_MusicApp);
			base.OnButtonCreated(newButton);
		}

		protected override void SetButtonContent(PhoneScrollButton button, int contentIndex)
		{
			(button as MusicPlayerTrackButton).SetContent(m_MusicApp.GetMusicTrack(musicAlfabeticList[contentIndex]));
		}

		protected override void SetButtonPosition(PhoneScrollButton button, float posIndex)
		{
			float num = m_AppButtonPrefab.RectTransform().sizeDelta.y + BUTTON_SPACING;
			RectTransform rectTransform = button.RectTransform();
			Vector2 anchoredPosition = new Vector2
			{
				x = rectTransform.anchoredPosition.x,
				y = BUTTON_TOP_OFFSET + (0f - (posIndex - (float)(SCROLL_RANGE / 2))) * num - ((SCROLL_RANGE % 2 == 0) ? (num / 2f) : 0f)
			};
			rectTransform.anchoredPosition = anchoredPosition;
		}

		public void FindAndMoveTo(AUnlockable unlockable, AudioManager m_AudioManager)
		{
			int num = -1;
			for (int i = 0; i < musicAlfabeticList.Count; i++)
			{
				if (m_MusicApp.GetMusicTrack(musicAlfabeticList[i]).Uid == unlockable.Uid)
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
