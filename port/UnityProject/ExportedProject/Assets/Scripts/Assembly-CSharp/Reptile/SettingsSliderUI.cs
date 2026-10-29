using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class SettingsSliderUI : Slider
	{
		public TextMeshProUGUI text;

		public OnSliderValueChangedEvent OnChanged = new OnSliderValueChangedEvent();

		public Selectable upSelectableOverride;

		public override Selectable FindSelectableOnDown()
		{
			return base.FindSelectableOnDown();
		}

		public override Selectable FindSelectableOnUp()
		{
			if (upSelectableOverride != null)
			{
				return upSelectableOverride;
			}
			return base.FindSelectableOnUp();
		}

		public override Selectable FindSelectableOnLeft()
		{
			value -= 0.05f;
			ValueChanged();
			return null;
		}

		public override Selectable FindSelectableOnRight()
		{
			value += 0.05f;
			ValueChanged();
			return null;
		}

		public override void OnDrag(PointerEventData eventData)
		{
			base.OnDrag(eventData);
			ValueChanged();
		}

		private void ValueChanged()
		{
			if (OnChanged != null)
			{
				OnChanged.Invoke(value);
			}
		}
	}
}
