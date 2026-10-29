using UnityEngine;

namespace Reptile.Phone
{
	public class PhoneScrollUnlockableButton : PhoneScrollButton
	{
		[SerializeField]
		private UnreadIndicator m_UnreadIndicator;

		public AUnlockable AssignedContent { get; private set; }

		public virtual void SetContent(AUnlockable content)
		{
			AssignedContent = content;
			SetIndicatorState(!Core.Instance.Platform.User.GetUnlockableSaveDataFor(AssignedContent).HasSeen);
		}

		public void SetUnread(bool isUnread)
		{
			Core.Instance.Platform.User.GetUnlockableSaveDataFor(AssignedContent).HasSeen = !isUnread;
			SetIndicatorState(isUnread);
		}

		private void SetIndicatorState(bool isActive)
		{
			m_UnreadIndicator.SetEnabled(isActive);
		}
	}
}
