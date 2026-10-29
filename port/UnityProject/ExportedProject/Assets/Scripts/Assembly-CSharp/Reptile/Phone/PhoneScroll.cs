using System.Collections;
using Kaj.SimpleEasing;
using UnityEngine;

namespace Reptile.Phone
{
	public class PhoneScroll : MonoBehaviour
	{
		public enum ScrollDirection
		{
			Previous = -1,
			Next = 1,
			None = 0
		}

		[Header("Layout customization (cannot be changed runtime)")]
		[SerializeField]
		protected int SCROLL_RANGE = 4;

		[Tooltip("[0 = SCROLL_RANGE], length of buttons per scroll")]
		[SerializeField]
		protected int SCROLL_AMOUNT;

		[Tooltip("[0 = SCROLL_AMOUNT], unselectable button amount used for animations")]
		[SerializeField]
		protected int OVERFLOW_BUTTON_AMOUNT;

		[Header("Behaviour customization")]
		[SerializeField]
		protected bool LIST_LOOPS = true;

		[SerializeField]
		protected bool SCROLL_IS_CANCELLABE;

		[SerializeField]
		protected bool RESELECT_WAITS_ON_SCROLL = true;

		[Space(5f)]
		[Header("Animation customization")]
		[SerializeField]
		protected float SCROLL_DURATION = 0.5f;

		[SerializeField]
		protected SimpleEasing.EaseType SCROLL_EASE = SimpleEasing.EaseType.EaseInOutQuad;

		[Header("Components")]
		[SerializeField]
		private RectTransform m_ButtonContainer;

		[Space(5f)]
		[SerializeField]
		protected GameObject m_AppButtonPrefab;

		protected int m_SelectorPos;

		protected int m_ButtonAssignStartIndex;

		protected Coroutine m_ActiveScroll;

		public float ScrollPosition => ((float)m_ButtonAssignStartIndex + (float)SCROLL_RANGE / 2f) / (float)m_ContentLength;

		protected int m_ContentLength { get; set; }

		protected PhoneScrollButton[] m_Buttons { get; private set; }

		public PhoneScrollButton SelectedButtton { get; private set; }

		public RectTransform ButtonContainer => m_ButtonContainer;

		public void InitalizeScrollView()
		{
			if (SCROLL_AMOUNT == 0)
			{
				SCROLL_AMOUNT = SCROLL_RANGE;
			}
			if (OVERFLOW_BUTTON_AMOUNT == 0)
			{
				OVERFLOW_BUTTON_AMOUNT = SCROLL_AMOUNT;
			}
			if (m_Buttons == null)
			{
				int num = SCROLL_RANGE + OVERFLOW_BUTTON_AMOUNT * 2;
				m_Buttons = new PhoneScrollButton[num];
				for (int i = 0; i < num; i++)
				{
					GameObject gameObject = Object.Instantiate(m_AppButtonPrefab, ButtonContainer);
					gameObject.name = $"AppButton {i}";
					m_Buttons[i] = gameObject.GetComponent<PhoneScrollButton>();
					OnButtonCreated(m_Buttons[i]);
				}
				for (int j = 0; j < m_Buttons.Length; j++)
				{
					int num2 = Mathf.FloorToInt(j / 2);
					int num3 = ((j % 2 == 0) ? num2 : (m_Buttons.Length - 1 - num2));
					m_Buttons[num3].RectTransform().SetAsLastSibling();
				}
			}
			UpdateButtonPositions(0f);
			PostInitalize();
		}

		protected virtual void OnButtonCreated(PhoneScrollButton newButton)
		{
			newButton.Initalize();
		}

		public void SetListContent(int contentLength)
		{
			m_SelectorPos = 0;
			m_ButtonAssignStartIndex = 0;
			m_ActiveScroll = null;
			m_ContentLength = contentLength;
			UpdateButtonContents();
			SelectButton(m_SelectorPos, unSelectOtherButtons: true);
		}

		public void UpdateListContent(int contentLength)
		{
			if (m_ButtonAssignStartIndex > contentLength)
			{
				m_ButtonAssignStartIndex = contentLength;
			}
			m_ActiveScroll = null;
			m_ContentLength = contentLength;
			UpdateButtonContents();
			if (m_SelectorPos > contentLength)
			{
				m_SelectorPos = contentLength;
			}
			SelectButton(m_SelectorPos, unSelectOtherButtons: true);
		}

		protected virtual void PostInitalize()
		{
		}

		protected void UpdateButtonPositions(float indexOffset)
		{
			for (int i = 0; i < m_Buttons.Length; i++)
			{
				float posIndex = (float)(i - OVERFLOW_BUTTON_AMOUNT) - indexOffset;
				SetButtonPosition(m_Buttons[i], posIndex);
			}
		}

		protected virtual void SetButtonPosition(PhoneScrollButton button, float posIndex)
		{
		}

		public void UpdateButtonContents()
		{
			for (int i = 0; i < m_Buttons.Length; i++)
			{
				int num = i - OVERFLOW_BUTTON_AMOUNT;
				int contentIndex = GetContentIndex(num);
				if (!LIST_LOOPS)
				{
					if (m_ButtonAssignStartIndex + num < 0 || m_ButtonAssignStartIndex + num >= m_ContentLength)
					{
						m_Buttons[i].IsHidden = true;
						continue;
					}
					m_Buttons[i].IsHidden = false;
				}
				else
				{
					m_Buttons[i].IsHidden = false;
				}
				SetButtonContent(m_Buttons[i], contentIndex);
			}
		}

		protected virtual void SetButtonContent(PhoneScrollButton button, int contentIndex)
		{
		}

		public void Move(ScrollDirection direction, AudioManager audioManager)
		{
			if (!SCROLL_IS_CANCELLABE && m_ActiveScroll != null)
			{
				return;
			}
			int num = (int)(m_SelectorPos + direction);
			int num2 = SCROLL_RANGE;
			if (!LIST_LOOPS)
			{
				num2 = Mathf.Min(SCROLL_RANGE, m_ContentLength - m_ButtonAssignStartIndex);
			}
			audioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_Select);
			if (num < 0 || num >= num2)
			{
				if (m_ActiveScroll != null)
				{
					StopCoroutine(m_ActiveScroll);
				}
				m_ActiveScroll = StartCoroutine(Scroll(direction));
			}
			else
			{
				DeselectButton(m_SelectorPos);
				m_SelectorPos = num;
				SelectButton(m_SelectorPos);
			}
		}

		private IEnumerator Scroll(ScrollDirection direction)
		{
			float easeTimer = 0f;
			float num = m_ButtonAssignStartIndex;
			int scrollAmount = SCROLL_AMOUNT;
			if (LIST_LOOPS)
			{
				m_ButtonAssignStartIndex = Utility.Mod(m_ButtonAssignStartIndex + scrollAmount * (int)direction, m_ContentLength);
			}
			else
			{
				if (direction != ScrollDirection.Previous)
				{
					if (direction == ScrollDirection.Next)
					{
						while (num + (float)scrollAmount >= (float)(m_ContentLength - 1) && scrollAmount > 0)
						{
							scrollAmount--;
						}
					}
				}
				else
				{
					while (num + (float)scrollAmount < 0f && scrollAmount > 0)
					{
						scrollAmount--;
					}
				}
				m_ButtonAssignStartIndex = Mathf.Clamp(m_ButtonAssignStartIndex + scrollAmount * (int)direction, 0, m_ContentLength - SCROLL_RANGE);
				if (m_ContentLength < SCROLL_RANGE)
				{
					m_ButtonAssignStartIndex = 0;
				}
			}
			if ((float)m_ButtonAssignStartIndex != num)
			{
				DeselectButton(m_SelectorPos);
				UpdateButtonContents();
				if (!RESELECT_WAITS_ON_SCROLL)
				{
					SelectButton(m_SelectorPos);
				}
				OnScroll(direction);
				while (easeTimer < SCROLL_DURATION)
				{
					easeTimer += Time.deltaTime;
					easeTimer = Mathf.Clamp(easeTimer, 0f, SCROLL_DURATION);
					float indexOffset = SimpleEasing.SimpleEase(SCROLL_EASE, easeTimer, -((int)direction * scrollAmount), (int)direction * scrollAmount, SCROLL_DURATION);
					UpdateButtonPositions(indexOffset);
					yield return new WaitForEndOfFrame();
				}
				if (RESELECT_WAITS_ON_SCROLL)
				{
					SelectButton(m_SelectorPos);
				}
				m_ActiveScroll = null;
			}
		}

		public int GetContentIndex(int buttonIndex)
		{
			return Utility.Mod(m_ButtonAssignStartIndex + buttonIndex, m_ContentLength);
		}

		public int GetContentIndex()
		{
			return GetContentIndex(m_SelectorPos);
		}

		public PhoneScrollButton GetButtonByRelativeIndex(int index)
		{
			return m_Buttons[Utility.Mod(OVERFLOW_BUTTON_AMOUNT + index, m_Buttons.Length)];
		}

		public PhoneScrollButton GetButtonByAbsoluteIndex(int index)
		{
			return m_Buttons[Utility.Mod(index, m_Buttons.Length)];
		}

		internal void SelectButton(int index, bool unSelectOtherButtons = false, bool skipAnimation = false)
		{
			SelectedButtton = m_Buttons[index + OVERFLOW_BUTTON_AMOUNT];
			m_Buttons[index + OVERFLOW_BUTTON_AMOUNT].IsSelected = true;
			OnButtonSelected(index + OVERFLOW_BUTTON_AMOUNT, skipAnimation);
			if (!unSelectOtherButtons)
			{
				return;
			}
			for (int i = 0; i < m_Buttons.Length; i++)
			{
				if (i != index + OVERFLOW_BUTTON_AMOUNT)
				{
					m_Buttons[i].IsSelected = false;
				}
			}
		}

		internal void DeselectButton(int index)
		{
			SelectedButtton = null;
			m_Buttons[index + OVERFLOW_BUTTON_AMOUNT].IsSelected = false;
		}

		public virtual void OnButtonSelected(int buttonIndex, bool skipAnimation = false)
		{
		}

		protected virtual void OnScroll(ScrollDirection direction)
		{
		}

		public void HoldAnimationSelectedButton()
		{
			SelectedButtton.OnHold();
		}

		public void ActivateAnimationSelectedButton()
		{
			SelectedButtton.OnActivate();
		}

		public int GetScrollRange()
		{
			return SCROLL_RANGE;
		}

		public int GetScrollAmount()
		{
			return SCROLL_AMOUNT;
		}

		public int GetOverflowButtonAmount()
		{
			return OVERFLOW_BUTTON_AMOUNT;
		}

		public int GetSelectorPos()
		{
			return m_SelectorPos;
		}

		public PhoneScrollButton[] GetButtons()
		{
			return m_Buttons;
		}
	}
}
