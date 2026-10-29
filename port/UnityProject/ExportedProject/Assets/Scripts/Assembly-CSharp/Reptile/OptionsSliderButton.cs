using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class OptionsSliderButton : TextMeshProMenuButton
	{
		[SerializeField]
		private TextMeshProSlider slider;

		public override Selectable FindSelectableOnLeft()
		{
			MoveSliderHandleLeft();
			return null;
		}

		public override Selectable FindSelectableOnRight()
		{
			MoveSliderHandlerRight();
			return null;
		}

		public override void OnSubmit(BaseEventData eventData)
		{
			base.OnSubmit(eventData);
			OnSliderSubmit();
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			OnSliderSubmit();
		}

		private void OnSliderSubmit()
		{
			if (slider.IsAtMinValue)
			{
				slider.ResetToDefaultValue();
			}
			else
			{
				slider.SetToMinimalValue();
			}
		}

		private void MoveSliderHandleLeft()
		{
			slider.MoveSliderHandleLeft();
		}

		private void MoveSliderHandlerRight()
		{
			slider.MoveSliderHandleRight();
		}
	}
}
