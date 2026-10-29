using UnityEngine;

namespace Reptile
{
	public class SnakeBossAnimInfo : EnemyAnimInfos
	{
		public override void Init(EnemyAnimations target)
		{
			base.Init(target);
			new AnimInfo(target, "idle")
			{
				crossFades = 
				{
					{
						Animator.StringToHash("mortarShoot"),
						1f
					},
					{
						Animator.StringToHash("stopMortar"),
						0.5f
					},
					{
						Animator.StringToHash("MachineGunVentEnd"),
						0.5f
					},
					{
						Animator.StringToHash("MachineGunPrepare"),
						1f
					}
				}
			};
			new AnimInfo(target, "idleNoArms");
			new AnimInfo(target, "startMortar");
			new AnimInfo(target, "mortarShoot").crossFades.Add(Animator.StringToHash("startMortar"), 0.2f);
			new AnimInfo(target, "stopMortar").crossFades.Add(Animator.StringToHash("mortarShoot"), 0.2f);
			new AnimInfo(target, "MachineGunPrepare").crossFades.Add(Animator.StringToHash("idle"), 0.5f);
			new AnimInfo(target, "MachineGunAim");
			new AnimInfo(target, "MachineGunToVent").crossFades.Add(Animator.StringToHash("MachineGunAim"), 0.52f);
			new AnimInfo(target, "MachineGunStop");
			new AnimInfo(target, "MachineGunVenting");
			new AnimInfo(target, "MachineGunVentEnd");
			new AnimInfo(target, "LoseArmRecoil");
			new AnimInfo(target, "LoseArmRecoilLowLeft");
			new AnimInfo(target, "LoseArmRecoilLowRight");
			new AnimInfo(target, "LoseArmRecoilMidLeft");
			new AnimInfo(target, "LoseArmRecoilMidRight");
			new AnimInfo(target, "LoseArmRecoilHighLeft");
			new AnimInfo(target, "LoseArmRecoilHighRight");
			new AnimInfo(target, "mortarAimDir");
			new AnimInfo(target, "chestClose").layer = 1;
			new AnimInfo(target, "chestOpen").layer = 1;
			new AnimInfo(target, "chestPeak").layer = 1;
			new AnimInfo(target, "RailRidersLowStart");
			new AnimInfo(target, "RailRidersMidStart");
			new AnimInfo(target, "RailRidersHighStart");
			new AnimInfo(target, "RailRidersHighStop");
			new AnimInfo(target, "RailRidersMidStop");
			new AnimInfo(target, "RailRidersLowStop");
			new AnimInfo(target, "RailRidersHighToVent");
			new AnimInfo(target, "RailRidersHighVentEnd");
			new AnimInfo(target, "RailRidersMidToVent");
			new AnimInfo(target, "RailRidersMidVentStart");
			new AnimInfo(target, "RailRidersMidVentEnd");
			new AnimInfo(target, "RailRidersLowToVent");
			new AnimInfo(target, "RailRidersLowVentStart");
			new AnimInfo(target, "RailRidersLowVentEnd");
			new AnimInfo(target, "die");
		}
	}
}
