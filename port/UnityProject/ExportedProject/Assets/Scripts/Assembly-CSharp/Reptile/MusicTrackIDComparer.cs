using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Reptile
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct MusicTrackIDComparer : IEqualityComparer<MusicTrackID>
	{
		public bool Equals(MusicTrackID x, MusicTrackID y)
		{
			return x == y;
		}

		public int GetHashCode(MusicTrackID obj)
		{
			return (int)obj;
		}
	}
}
