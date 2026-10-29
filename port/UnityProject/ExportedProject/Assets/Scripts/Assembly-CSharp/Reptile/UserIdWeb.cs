using System;

namespace Reptile
{
	public class UserIdWeb : IUserId, IEquatable<IUserId>
	{
		private string userId;
		private string userName;

		public object Identifier => userId;
		public string UserName => userName;
		public bool IsValid => true;

		public UserIdWeb(string userName = "Player")
		{
			this.userId = "web_" + userName;
			this.userName = userName;
		}

		public bool Equals(IUserId other)
		{
			if (other == null) return false;
			return string.Equals(userId, other.Identifier as string);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as IUserId);
		}

		public override int GetHashCode()
		{
			return userId != null ? userId.GetHashCode() : 0;
		}
	}
}
