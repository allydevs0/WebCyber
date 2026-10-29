using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class AnimInfo
	{
		public Dictionary<int, float> crossFades = new Dictionary<int, float>();

		public int hashedName;

		public int layer;

		public AnimInfo(EnemyAnimations target, string name)
		{
			hashedName = Animator.StringToHash(name);
			target.EnemyAnimationInfos.Add(hashedName, this);
		}
	}
}
