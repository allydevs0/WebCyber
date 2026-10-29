using System.Collections.Generic;

namespace Reptile
{
	public class ComparerHash : IComparer<AnimationInstanceInfo>
	{
		public int Compare(AnimationInstanceInfo x, AnimationInstanceInfo y)
		{
			return x.animationNameHash.CompareTo(y.animationNameHash);
		}
	}
}
