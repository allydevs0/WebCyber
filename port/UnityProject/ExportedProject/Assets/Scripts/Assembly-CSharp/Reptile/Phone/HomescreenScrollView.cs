using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class HomescreenScrollView : PhoneScroll
	{
		[Header("Button pos customization")]
		[SerializeField]
		private float BUTTON_SPACING;

		[Space(5f)]
		[Header("Selector customization")]
		[SerializeField]
		public HomescreenSelector Selector;

		public const Ease SELECTOR_EASE = Ease.Linear;

		public const float SELECTOR_DURATION = 0.08f;

		[Space(5f)]
		[SerializeField]
		private AppHomeScreen m_HomeScreen;

		[SerializeField]
		public RectTransform OtherElementsParent;

		[Space(5f)]
		[SerializeField]
		private Image m_ArrowUp;

		[SerializeField]
		private Image m_ArrowDown;

		public Vector2 ButtonPos(float posIndex)
		{
			float num = m_AppButtonPrefab.RectTransform().sizeDelta.y + BUTTON_SPACING;
			return new Vector2
			{
				x = 0f,
				y = (0f - (posIndex - (float)(SCROLL_RANGE / 2))) * num - ((SCROLL_RANGE % 2 == 0) ? (num / 2f) : 0f)
			};
		}

		protected override void PostInitalize()
		{
			UpdateArrows();
			MoveSelector(skipAnimation: true);
		}

		protected override void SetButtonPosition(PhoneScrollButton button, float posIndex)
		{
			button.RectTransform().anchoredPosition = ButtonPos(posIndex);
			float alpha = 1f;
			if (posIndex <= 0f)
			{
				alpha = 1f - Mathf.Abs(posIndex) / 1f;
			}
			else if (posIndex >= (float)(SCROLL_RANGE - 1))
			{
				alpha = 1f - (posIndex - (float)(SCROLL_RANGE - 1)) / 1f;
			}
			(button as HomescreenButton).SetAlpha(alpha);
		}

		protected override void SetButtonContent(PhoneScrollButton button, int contentIndex)
		{
			HomeScreenApp homeScreenApp = m_HomeScreen.Apps[contentIndex];
			int num = 0;
			AUnlockable[] unlockables = m_HomeScreen.AppInstance(homeScreenApp.AppName).Unlockables;
			foreach (AUnlockable unlockable in unlockables)
			{
				UnlockableSaveData unlockableSaveDataFor = Core.Instance.Platform.User.GetUnlockableSaveDataFor(unlockable);
				if (!unlockableSaveDataFor.HasSeen && unlockableSaveDataFor.IsUnlocked)
				{
					num++;
				}
			}
			(button as HomescreenButton).SetContent(homeScreenApp, num);
		}

		public override void OnButtonSelected(int buttonIndex, bool skipAnimation = false)
		{
			MoveSelector();
		}

		protected override void OnScroll(ScrollDirection direction)
		{
			UpdateArrows();
		}

		internal void UpdateArrows()
		{
			if (GetSelectorPos() + m_ButtonAssignStartIndex == 0)
			{
				m_ArrowUp.DOFade(0f, 0.3f);
			}
			else
			{
				m_ArrowUp.DOFade(1f, 0.3f);
			}
			if (GetSelectorPos() + m_ButtonAssignStartIndex == base.m_ContentLength - 1 || base.m_ContentLength <= SCROLL_RANGE)
			{
				m_ArrowDown.DOFade(0f, 0.3f);
			}
			else
			{
				m_ArrowDown.DOFade(1f, 0.3f);
			}
		}

		public void MoveSelector(bool skipAnimation = false)
		{
			Selector.RectTransform().DOKill();
			Selector.RectTransform().DOAnchorPosY(ButtonPos(m_SelectorPos).y, 0.08f * (float)Convert.ToInt32(!skipAnimation)).SetEase(Ease.Linear);
		}
	}
}
