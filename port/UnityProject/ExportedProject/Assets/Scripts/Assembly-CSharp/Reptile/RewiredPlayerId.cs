using Rewired.Dev;

namespace Reptile
{
	public static class RewiredPlayerId
	{
		[PlayerIdFieldInfo(friendlyName = "System")]
		public const int System = 9999999;

		[PlayerIdFieldInfo(friendlyName = "Player0")]
		public const int Player0 = 0;
	}
}
