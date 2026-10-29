using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class UserStatusHandler : MonoBehaviour
	{
		public Text currentUserName;

		public Text currentUserId;

		private AUser currentUser;

		public void SetCurrentUser()
		{
			currentUser = Core.Instance.Platform.User;
		}

		public void SetCurrentUserStatus()
		{
			currentUserName.text = "Current username: " + currentUser.UserName;
			SetUserId();
		}

		private void SetUserId()
		{
			currentUserId.text = "Current user id: " + currentUser.UserIdentifier;
		}
	}
}
