using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Reptile
{
	public class SettingsMultiOptionsUI : Selectable
	{
		public List<string> options = new List<string>();

		public TextMeshProUGUI text;

		public Button leftArrow;

		public Button rightArrow;

		private string currentOption;

		public int currentIndex;

		public OnMultiOptionsUIChangedEvent OnChanged = new OnMultiOptionsUIChangedEvent();

		protected override void Awake()
		{
			base.Awake();
			currentOption = options[0];
			leftArrow.onClick.AddListener(() =>
			{
				FindSelectableOnLeft();
			});
			rightArrow.onClick.AddListener(() =>
			{
				FindSelectableOnRight();
			});
		}

		protected override void OnDestroy()
		{
			leftArrow.onClick.RemoveAllListeners();
			rightArrow.onClick.RemoveAllListeners();
		}

		public override Selectable FindSelectableOnLeft()
		{
			SetNewOption(increase: false);
			return null;
		}

		public override Selectable FindSelectableOnRight()
		{
			SetNewOption(increase: true);
			return null;
		}

		private void SetNewOption(bool increase)
		{
			if (increase)
			{
				currentIndex++;
				if (currentIndex == options.Count)
				{
					currentIndex = 0;
				}
			}
			else
			{
				currentIndex--;
				if (currentIndex < 0)
				{
					currentIndex = options.Count - 1;
				}
			}
			UpdateCurrentOption(currentIndex);
			if (OnChanged != null)
			{
				OnChanged.Invoke(currentOption);
			}
		}

		public void SetupOptions(List<string> newOptions, int newIndex)
		{
			options = newOptions;
			currentIndex = newIndex;
			currentOption = options[currentIndex];
			text.text = currentOption;
		}

		public void UpdateCurrentOption(int index)
		{
			currentOption = options[currentIndex];
			text.text = currentOption;
		}
	}
}
