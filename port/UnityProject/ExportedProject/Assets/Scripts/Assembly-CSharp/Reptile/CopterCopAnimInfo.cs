using UnityEngine;

namespace Reptile
{
	public class CopterCopAnimInfo : EnemyAnimInfos
	{
		public override void Init(EnemyAnimations target)
		{
			base.Init(target);
			new AnimInfo(target, "CopterMovement");
			new AnimInfo(target, "MovementX");
			new AnimInfo(target, "MovementY");
			new AnimInfo(target, "CopterSpinOut").crossFades.Add(Animator.StringToHash("CopterMovement"), 0.5f);
		}
	}
}
