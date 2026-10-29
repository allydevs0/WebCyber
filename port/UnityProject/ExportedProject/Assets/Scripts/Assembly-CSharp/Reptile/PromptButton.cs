using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class PromptButton : Button
	{
		[SerializeField]
		private PromptButton companionButton;

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			Select();
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			DoStateTransition(SelectionState.Pressed, instant: false);
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			DoStateTransition(SelectionState.Selected, instant: false);
			companionButton.DoStateTransition(SelectionState.Normal, instant: false);
		}
	}
}
