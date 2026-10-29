using UnityEngine.Events;

namespace Reptile
{
	public class YesNoPanel : AMenuController
	{
		public TextMeshProMenuButton yesButton;

		public TextMeshProMenuButton noButton;

		public void SetCallbacks(UnityAction yesAction, UnityAction noAction)
		{
			if (yesButton.useDelayedButtonClick)
			{
				yesButton.onDelayedButtonClick.RemoveAllListeners();
				yesButton.onDelayedButtonClick.AddListener(yesAction);
			}
			else
			{
				yesButton.onClick.RemoveAllListeners();
				yesButton.onClick.AddListener(yesAction);
			}
			if (noButton.useDelayedButtonClick)
			{
				noButton.onDelayedButtonClick.RemoveAllListeners();
				noButton.onDelayedButtonClick.AddListener(noAction);
			}
			else
			{
				noButton.onClick.RemoveAllListeners();
				noButton.onClick.AddListener(noAction);
			}
		}

		public void ResetCallbacks()
		{
			if (yesButton.useDelayedButtonClick)
			{
				yesButton.onDelayedButtonClick.RemoveAllListeners();
			}
			else
			{
				yesButton.onClick.RemoveAllListeners();
			}
			if (noButton.useDelayedButtonClick)
			{
				noButton.onDelayedButtonClick.RemoveAllListeners();
			}
			else
			{
				noButton.onClick.RemoveAllListeners();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (Core.Instance != null && uIManager != null)
			{
				uIManager.AbsolutePopIfPresent(this);
			}
		}
	}
}
