using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class SelectEnlargeButton : Button
	{
		public Vector2 enlargeToSize = new Vector2(48f, 48f);

		private Vector2 originalSize = Vector2.zero;

		private RectTransform buttonRectTransform;

		private AudioManager audioManager;

		private bool playSoundOnSelect = true;

		protected override void Awake()
		{
			base.Awake();
			buttonRectTransform = GetComponent<RectTransform>();
			originalSize = buttonRectTransform.sizeDelta;
			base.onClick.AddListener(OnClickSound);
			audioManager = Core.Instance.AudioManager;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			buttonRectTransform = GetComponent<RectTransform>();
			originalSize = buttonRectTransform.sizeDelta;
		}

		protected void OnClickSound()
		{
			audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.confirm);
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			buttonRectTransform.sizeDelta = enlargeToSize;
			if (playSoundOnSelect)
			{
				audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
			}
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			buttonRectTransform.sizeDelta = originalSize;
			playSoundOnSelect = true;
		}

		public override void OnMove(AxisEventData eventData)
		{
			playSoundOnSelect = false;
			base.OnMove(eventData);
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (eventData.selectedObject == base.gameObject)
			{
				DoStateTransition(SelectionState.Highlighted, instant: true);
				buttonRectTransform.sizeDelta = enlargeToSize;
				audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
			}
			else
			{
				playSoundOnSelect = true;
				Select();
			}
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			buttonRectTransform.sizeDelta = originalSize;
			DoStateTransition(SelectionState.Normal, instant: true);
			playSoundOnSelect = true;
		}
	}
}
