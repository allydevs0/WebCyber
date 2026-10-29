using UnityEngine;

namespace Reptile
{
	public class DJBossAnimInfo : EnemyAnimInfos
	{
		public override void Init(EnemyAnimations target)
		{
			base.Init(target);
			Animator.StringToHash("idle");
			Animator.StringToHash("move");
			Animator.StringToHash("vinylPose");
			int key = Animator.StringToHash("vinylImpact");
			Animator.StringToHash("vinylImpact2");
			Animator.StringToHash("kickImpact");
			int key2 = Animator.StringToHash("vinylThrowA");
			int key3 = Animator.StringToHash("vinylThrowB");
			int key4 = Animator.StringToHash("jump");
			int key5 = Animator.StringToHash("jumpLand");
			int key6 = Animator.StringToHash("vinylImpactDropDown");
			new AnimInfo(target, "closedIdle");
			new AnimInfo(target, "move")
			{
				crossFades = 
				{
					{ key2, 0.1f },
					{ key3, 0.1f },
					{ key, 0.6f }
				}
			};
			new AnimInfo(target, "moveUpsideDown");
			new AnimInfo(target, "openIdle").crossFades.Add(key5, 0.1f);
			new AnimInfo(target, "openIdleUpsideDown");
			new AnimInfo(target, "vinylImpact").crossFades.Add(key6, 0.6f);
			new AnimInfo(target, "vinylImpactDropDown");
			new AnimInfo(target, "kickImpact");
			new AnimInfo(target, "jumpToCeiling");
			new AnimInfo(target, "dropToGround");
			new AnimInfo(target, "vinylThrowA");
			new AnimInfo(target, "vinylThrowB");
			new AnimInfo(target, "vinylThrowUpsideDownA");
			new AnimInfo(target, "vinylThrowUpsideDownB");
			new AnimInfo(target, "jump")
			{
				crossFades = 
				{
					{ key2, 0.1f },
					{ key3, 0.1f }
				}
			};
			new AnimInfo(target, "jumpLand").crossFades.Add(key4, 0.1f);
			new AnimInfo(target, "movementSpeed");
			new AnimInfo(target, "jumpDirection");
			new AnimInfo(target, "kickImpactDeath");
		}
	}
}
