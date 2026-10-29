using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class MultiOptionButton : TextMeshProMenuButton
	{
		public delegate void SelectionChangedDelegate(MultiOptionButton multiOptionButton);

		[SerializeField]
		private TextMeshProUGUI optionText;

		private int currentIndex;

		private int maxOptions;

		public int CurrentIndex => currentIndex;

		public event SelectionChangedDelegate OnChanged;

		public override Selectable FindSelectableOnLeft()
		{
			SelectPreviousOption();
			HandleChangedValue();
			return null;
		}

		public override Selectable FindSelectableOnRight()
		{
			SelectNextOption();
			HandleChangedValue();
			return null;
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			SelectNextOption();
			HandleChangedValue();
		}

		public override void OnSubmit(BaseEventData eventData)
		{
			base.OnSubmit(eventData);
			SelectNextOption();
			HandleChangedValue();
		}

		public void SetupOptions(string currentOption, int currentIndex, int maxOptions)
		{
			this.currentIndex = currentIndex;
			this.maxOptions = maxOptions;
			SetLabelText(currentOption);
		}

		public void SetLabelText(string text)
		{
			optionText.SetText(text);
		}

		public void DisableButton()
		{
			base.gameObject.SetActive(value: false);
			optionText.gameObject.SetActive(value: false);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			OnChanged = null;
		}

		private void SelectNextOption()
		{
			currentIndex++;
			if (currentIndex == maxOptions)
			{
				currentIndex = 0;
			}
		}

		private void SelectPreviousOption()
		{
			currentIndex--;
			if (currentIndex < 0)
			{
				currentIndex = maxOptions - 1;
			}
		}

		private void HandleChangedValue()
		{
			if (OnChanged != null)
			{
				OnChanged(this);
			}
		}
	}
}
