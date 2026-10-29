using UnityEngine;

namespace Reptile
{
	public class TankwalkerAnimInfo : EnemyAnimInfos
	{
		public override void Init(EnemyAnimations target)
		{
			base.Init(target);
			new AnimInfo(target, "idle")
			{
				crossFades = 
				{
					{
						Animator.StringToHash("walk"),
						0.5f
					},
					{
						Animator.StringToHash("headStab"),
						1f
					}
				}
			};
			new AnimInfo(target, "walk").crossFades.Add(Animator.StringToHash("idle"), 0.5f);
			new AnimInfo(target, "pound");
			new AnimInfo(target, "slapRight");
			new AnimInfo(target, "slapLeft");
			new AnimInfo(target, "headStab");
			new AnimInfo(target, "prepareMachineguns");
			new AnimInfo(target, "aimMachineguns").crossFades.Add(Animator.StringToHash("prepareMachineguns"), 0.5f);
			new AnimInfo(target, "machinegunsEnd");
			new AnimInfo(target, "ventStart");
			new AnimInfo(target, "ventEnd");
			new AnimInfo(target, "AimDistance");
			new AnimInfo(target, "AimAngle");
			new AnimInfo(target, "tankwalkerLoseLeftArm");
			new AnimInfo(target, "tankwalkerLoseRightArm");
			new AnimInfo(target, "tankwalkerDie");
			new AnimInfo(target, "tankwalkerMortarShoot");
			new AnimInfo(target, "MoveX");
			new AnimInfo(target, "MoveY");
		}
	}
}
