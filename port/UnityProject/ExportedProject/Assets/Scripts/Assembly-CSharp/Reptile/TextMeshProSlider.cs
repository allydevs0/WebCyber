using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class TextMeshProSlider : Slider, ITextSelectable
	{
		public bool IsAtMinValue => value == base.minValue;

		public void ResetSelectableState()
		{
			OnHandleRemoveSliderHighlight();
		}

		public void DisableSelectableAsRaycastTarget()
		{
			if (!(base.image == null))
			{
				base.image.raycastTarget = false;
			}
		}

		public void EnableSelectableAsRaycastTarget()
		{
			if (!(base.image == null))
			{
				base.image.raycastTarget = true;
			}
		}

		public float CalculatePercentile()
		{
			float num = CalculateSliderRange();
			if (num == 0f)
			{
				return 0f;
			}
			return value / num;
		}

		public void SetSliderValue(float valueToSet)
		{
			float num = CalculateSliderRange();
			value = Mathf.Round(num * valueToSet);
		}

		public void MoveSliderHandleLeft()
		{
			if (value != base.minValue)
			{
				value--;
				if (base.onValueChanged != null)
				{
					base.onValueChanged.Invoke(value);
				}
			}
		}

		public void MoveSliderHandleRight()
		{
			if (value != base.maxValue)
			{
				value++;
				if (base.onValueChanged != null)
				{
					base.onValueChanged.Invoke(value);
				}
			}
		}

		public void SetToMinimalValue()
		{
			value = base.minValue;
			if (base.onValueChanged != null)
			{
				base.onValueChanged.Invoke(value);
			}
		}

		public void ResetToDefaultValue()
		{
			value = Mathf.Round(CalculateSliderRange() * 0.5f);
			if (base.onValueChanged != null)
			{
				base.onValueChanged.Invoke(value);
			}
		}

		private void OnHandleRemoveSliderHighlight()
		{
		}

		private float CalculateSliderRange()
		{
			return base.maxValue - base.minValue;
		}
	}
}
