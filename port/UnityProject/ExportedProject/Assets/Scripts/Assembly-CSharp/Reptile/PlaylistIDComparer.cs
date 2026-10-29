using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Reptile
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PlaylistIDComparer : IEqualityComparer<PlaylistID>
	{
		public bool Equals(PlaylistID x, PlaylistID y)
		{
			return x == y;
		}

		public int GetHashCode(PlaylistID obj)
		{
			return (int)obj;
		}
	}
}
